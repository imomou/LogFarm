using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.Runtime;
using NLog.Common;

namespace ShadowBlue.LogFarm.Domain.NLog
{
    public interface ICloudWatchLogsClientWrapper
    {
        void AddLogRequest(List<InputLogEvent> events);
        void InitialiseLogStream();
    }

    public class CloudWatchLogsClientWrapper : ICloudWatchLogsClientWrapper
    {
        private static readonly object LockObject = new object();

        private static string _nextSequence;

        private readonly IAmazonCloudWatchLogs _client;

        private readonly string _logGroup;
        private readonly string _logStream;
        
        public CloudWatchLogsClientWrapper(IAmazonCloudWatchLogs client, string logGroup, string logStream)
        {
            _client = client;
            _logGroup = logGroup;
            _logStream = logStream;
        }

        public void InitialiseLogStream()
        {
            var describeLogStreamsResponse = _client.DescribeLogStreams(
                new DescribeLogStreamsRequest(_logGroup)
                {
                    LogStreamNamePrefix = _logStream,
                });

            if (describeLogStreamsResponse.LogStreams.Any(x => x.LogStreamName == _logStream))
                return;

            try
            {
                _client.CreateLogStream(new CreateLogStreamRequest(_logGroup, _logStream));

                var timer = Stopwatch.StartNew();
                while (true)
                {
                    if (describeLogStreamsResponse.LogStreams.Any(x => x.LogStreamName == _logStream))
                    {
                        timer.Stop();
                        break;
                    }

                    if (timer.ElapsedMilliseconds > 100000)
                    {
                        timer.Stop();
                        throw new ApplicationException("Cannot CrearteLog Stream");
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (ResourceAlreadyExistsException)
            {
                InternalLogger.Debug("creation of logstream already existed {0}", _logStream);
            }
        }

        public void AddLogRequest(List<InputLogEvent> events)
        {      
            var putLogEventsRequest = new PutLogEventsRequest(
                _logGroup, 
                _logStream, 
                events);

            var logstream = _client.DescribeLogStreams
                (
                    new DescribeLogStreamsRequest(_logGroup)
                )
                .LogStreams
                .Where(x => x.LogStreamName == _logStream)
                .ToList();

            if ( !logstream.Any())
                throw new ApplicationException("LogStream doesn't exist");

            _nextSequence = logstream.Single().UploadSequenceToken;

            lock (LockObject)
            {
                AmazonWebServiceResponse ret = null;

                for (var i = 0; i < 10 && ret == null; i++)
                {
                    try
                    {
                        putLogEventsRequest.SequenceToken = _nextSequence;
                        var putLogEventsResponse = _client.PutLogEvents(putLogEventsRequest);

                        if (putLogEventsResponse == null)
                            continue;
                        
                        _nextSequence = putLogEventsResponse.NextSequenceToken;
                        ret = putLogEventsResponse;
                    }
                    catch (OperationAbortedException)
                    {
                    }
                    catch (DataAlreadyAcceptedException e)
                    {
                        var matchCollection = Regex.Matches(e.Message, @"[0-9]{20,}");
                        _nextSequence = matchCollection.Count > 0 ? matchCollection[0].Value : null;
                    }
                    catch (InvalidSequenceTokenException e)
                    {
                        var matchCollection = Regex.Matches(e.Message, @"[0-9]{20,}");
                        _nextSequence = matchCollection.Count > 0 ? matchCollection[0].Value : null;
                    }
                }
            }
        }
    }
}
