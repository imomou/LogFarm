using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.Runtime;
using Amazon.Util;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;

namespace ShadowBlue.LogFarm.Domain.NLog
{
    [Target("AWSCloudWatchLogTarget")]
    public class AwsCloudWatchLogTarget : TargetWithLayout
    {
        private ICloudWatchLogsClientWrapper _client;

        [RequiredParameter]
        public string LogGroup { get; set; }

        [RequiredParameter]
        public string LogStreaam { get; set; }

        public string Environment { get; set; }

        public string Suffix { get; set; }

        public void Setup(ICloudWatchLogsClientWrapper client)
        {
            _client = client;
        }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            var regionConfig = WebConfigurationManager.AppSettings["AWSRegion"] ?? "us-west-1";
            var region = RegionEndpoint.GetBySystemName(regionConfig);

            try
            {
                var awsClient = new AmazonCloudWatchLogsClient(region);
                var logstream = !string.IsNullOrEmpty(Suffix)
                    ? string.Format("{0}-{1}-{2}", LogStreaam, Environment, Suffix) 
                    : !string.IsNullOrEmpty(EC2InstanceMetadata.InstanceId)
                            ? string.Format("{0}-{1}-{2}", LogStreaam, Environment, EC2InstanceMetadata.InstanceId)
                            : string.Format("{0}-{1}", LogStreaam, Environment);

                InternalLogger.Debug("Writing LogStream", logstream);

                _client = new CloudWatchLogsClientWrapper(awsClient, LogGroup, logstream);
                _client.InitialiseLogStream();
            }
            catch (AmazonServiceException ex)
            {
                //purely for runnning it locally for the reason this will be still instantiated even if it's not part of the targe
                if (ex.Message != "Unable to find credentials")
                    throw;
            }
        }

        protected override void Write(LogEventInfo logEvent)
        {
            InternalLogger.Debug("AwsClouWatchLogTarget", "Appending");

            var message = Layout.Render(logEvent);

            var logRequest = new List<InputLogEvent>
                {
                    new InputLogEvent
                    {
                        Timestamp = logEvent
                            .TimeStamp
                            .ToUniversalTime(),
                        Message = message
                    }
                };

            try
            {
                _client.AddLogRequest(logRequest);
            }
            catch (ApplicationException ex)
            {
                if (ex.Message != "LogStream doesn't exist")
                    throw;

                _client.InitialiseLogStream();
                _client.AddLogRequest(logRequest);
            }
        }

        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            InternalLogger.Debug("AwsClouWatchLogTarget", "Appending");

            var logRequest = logEvents
                .Select(x => new InputLogEvent
                {
                    Timestamp = x.LogEvent
                        .TimeStamp
                        .ToUniversalTime(),
                    Message = Layout.Render(x.LogEvent)
                })
                .ToList();

            try
            {
                _client.AddLogRequest(logRequest);
            }
            catch (ApplicationException ex)
            {
                if (ex.Message != "LogStream doesn't exist")
                    throw;

                _client.InitialiseLogStream();
                _client.AddLogRequest(logRequest);
            }
        }
    }
}
