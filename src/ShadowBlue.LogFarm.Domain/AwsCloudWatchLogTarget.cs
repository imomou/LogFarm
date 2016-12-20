using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using Amazon;
using Amazon.CloudWatchLogs.Model;
using Amazon.EC2.Util;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;

namespace ShadowBlue.LogFarm.Domain
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

        public void Setup(ICloudWatchLogsClientWrapper client)
        {
            _client = client;
        }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            InternalLogger.Debug("Amazon Instance Id", EC2Metadata.InstanceId);

            var regionConfig = WebConfigurationManager.AppSettings["AWSRegion"] ??
                               "ap-southeast-2";

            if (string.IsNullOrEmpty(EC2Metadata.InstanceId))
                return;

            var region = RegionEndpoint.GetBySystemName(regionConfig);
            var awsClient = AWSClientFactory.CreateAmazonCloudWatchLogsClient(region);
            var logstream = string.Format("{0}-{1}-{2}", LogStreaam, Environment, EC2Metadata.InstanceId);

            InternalLogger.Debug("Writing LogStream", logstream);

            _client = new CloudWatchLogsClientWrapper(awsClient, LogGroup, logstream);
            _client.InitialiseLogStream();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            InternalLogger.Debug("AwsClouWatchLogTarget", "Appending");

            var message = Layout.Render(logEvent);

            _client.AddLogRequest(new List<InputLogEvent>
            {
                new InputLogEvent
                {
                    Timestamp = logEvent.TimeStamp,
                    Message = message
                }
            });
        }

        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            InternalLogger.Debug("AwsClouWatchLogTarget", "Appending");

            var logRequest = logEvents
                .Select(x => new InputLogEvent
                {
                    Timestamp = x.LogEvent.TimeStamp,
                    Message = Layout.Render(x.LogEvent)
                })
                .ToList();

            _client.AddLogRequest(logRequest);
        }

        public void TestWrite(LogEventInfo logEvent)
        {
            Write(logEvent);
        }
    }
}
