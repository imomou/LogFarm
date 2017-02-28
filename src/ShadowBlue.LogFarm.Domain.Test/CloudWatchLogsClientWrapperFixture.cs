using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.Runtime;
using AutoMoq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ShadowBlue.LogFarm.Domain.NLog;
using Should.Fluent;

namespace ShadowBlue.LogFarm.Domain.Test
{
    [TestFixture]
    public class CloudWatchLogsClientWrapperFixture
    {
        private const string LogGroup = "Dev-LogFarm-NLogGroup-9V1QT6AKKOST";
        private const string Logstream = "NLogTestStream";

        public AmazonCloudWatchLogsClient InitialiseClient()
        {
            try
            {
                var cloudwatchClient = new AmazonCloudWatchLogsClient(RegionEndpoint.USWest1);

                return cloudwatchClient;
            }
            catch (AmazonServiceException)
            {
                var lines = System.IO.File.ReadAllLines(@"..\..\..\TestArtifacts\credentials.dec");

                var key = lines.First();
                var secret = lines.ElementAt(1);

                try
                {
                    var cloudwatchClient = new AmazonCloudWatchLogsClient(new BasicAWSCredentials(key, secret),
                        RegionEndpoint.USWest1);

                    return cloudwatchClient;
                }
                catch (Exception)
                {
                    throw new Exception(string.Format("The security token included in the request is invalid key {0}", key));
                }         
            }
        }

        [SetUp]
        public void Setup()
        {
            var client = InitialiseClient();

            var logstreams = client.DescribeLogStreams(
                new DescribeLogStreamsRequest
                {
                    LogGroupName = LogGroup
                }
                );

            foreach (var deleteLogStreamRequest in logstreams.LogStreams.Select(logstream => new DeleteLogStreamRequest(
                LogGroup,
                logstream.LogStreamName
                )))
            {
                client.DeleteLogStream(deleteLogStreamRequest);
            }
        }

        /// <summary>
        /// When application Initialise LogStream, if logstream doesnt exist it should create a new one
        /// </summary>
        [Test]
        public void InitialiseLogStream_Verify_CreateLogStream()
        {
            //arrange
            var container = new AutoMoqer();

            var amazonclientMock = container.GetMock<IAmazonCloudWatchLogs>();

            amazonclientMock.Setup(x => x.DescribeLogStreams(It.IsAny<DescribeLogStreamsRequest>()))
                .Returns(new DescribeLogStreamsResponse
                {
                    LogStreams = new List<LogStream>()
                });

            var target = new CloudWatchLogsClientWrapper(amazonclientMock.Object, LogGroup, Logstream);

            //act
            target.InitialiseLogStream();

            //assert
            amazonclientMock.Verify(x => x.CreateLogStream(It.IsAny<CreateLogStreamRequest>()));
        }

        [Test]
        public void InitialiseLogStream_Case1_Verify_NotToCreateLogStream()
        {
            //arrange
            var container = new AutoMoqer();

            var amazonclientMock = container.GetMock<IAmazonCloudWatchLogs>();

            amazonclientMock.Setup(x => x.DescribeLogStreams(It.IsAny<DescribeLogStreamsRequest>()))
                .Returns(new DescribeLogStreamsResponse
                {
                    LogStreams = new List<LogStream>
                    {
                        new LogStream
                        {
                            LogStreamName = Logstream
                        }
                    }
                });

            var target = new CloudWatchLogsClientWrapper(amazonclientMock.Object, LogGroup, Logstream);

            //act
            target.InitialiseLogStream();

            //assert
            amazonclientMock.Verify(x => x.CreateLogStream(It.IsAny<CreateLogStreamRequest>())
                , Times.Never()
                );
        }

        [Test]
        public void InitialiseLogStream_Test()
        {
            //arrange
            var cloudwatchClient = InitialiseClient();

            var cloudwatvchLogClient = new CloudWatchLogsClientWrapper(cloudwatchClient, LogGroup, Logstream);

            cloudwatvchLogClient.InitialiseLogStream();

            //act
            var results = cloudwatchClient.DescribeLogStreams(new DescribeLogStreamsRequest(LogGroup));

            //assert
            results.LogStreams.Count().Should().Equal(1);
            results.LogStreams.Any(x => x.LogStreamName == Logstream).Should().Be.True();
        }

        /// <summary>
        /// Add LogEvents, PutLogEvents should be invoked
        /// </summary>
        [Test]
        public void AddLogRequest_Verify()
        {
            //arrange
            var container = new AutoMoqer();

            var amazonclientMock = container.GetMock<IAmazonCloudWatchLogs>();

            amazonclientMock.Setup(
                x => x.DescribeLogStreams(
                    It.IsAny<DescribeLogStreamsRequest>()
                    )
                ).Returns(new DescribeLogStreamsResponse
                {
                    LogStreams = new List<LogStream>
                    {
                        new LogStream
                        {
                            LogStreamName = Logstream
                        }
                    }
                });

            amazonclientMock.Setup(x => x.PutLogEvents(It.IsAny<PutLogEventsRequest>()))
                .Returns(new PutLogEventsResponse());

            var target = new CloudWatchLogsClientWrapper(amazonclientMock.Object, LogGroup, Logstream);

            var logEvents = new List<InputLogEvent>()
            {
                new InputLogEvent
                {
                    Message = "TestMessage",
                    Timestamp = DateTime.UtcNow
                }
            };

            //act
            target.AddLogRequest(logEvents);

            //assert
            amazonclientMock.Verify(x => x.PutLogEvents(
                It.Is<PutLogEventsRequest>(y =>
                    y.LogGroupName == LogGroup &&
                    y.LogStreamName == Logstream &&
                    y.LogEvents.Single().Message == logEvents.Single().Message &&
                    y.LogEvents.Single().Timestamp == logEvents.Single().Timestamp
                    ))
                , Times.Once()
                );
        }

        /// <summary>
        ///  Adding of 20 LogEvents, putting of logevents dont allow datetime for the future
        /// </summary>
        [Test]
        public void AddLogRequest_OnlyOneShouldbeCommited()
        {
            //arrange
            var cloudwatchClient = InitialiseClient();

            var logstream = Logstream + Guid.NewGuid();

            var cloudwatchLogClient = new CloudWatchLogsClientWrapper(
                cloudwatchClient, 
                LogGroup,
                logstream);

            cloudwatchLogClient.InitialiseLogStream();

            var logevents = Builder<InputLogEvent>
                .CreateListOfSize(20)
                .All()
                   .With(x => x.Timestamp = DateTime.Now.AddDays(1))
                .TheFirst(1)
                    .With(x => x.Timestamp = DateTime.Now.AddMinutes(-1))
                .Build()
                .ToList();
            
            //act
            cloudwatchLogClient.AddLogRequest(logevents);

            Thread.Sleep(2000);

            var results = cloudwatchClient.GetLogEvents(new GetLogEventsRequest(
                LogGroup,
                logstream));

            //assert
            results.Events.Count.Should().Equal(1);
            results.Events.All(x => string.IsNullOrEmpty(x.Message));
            results.Events.All(x => x.Timestamp != new DateTime());
        }

        /// <summary>
        ///  Adding of 20 LogEvents, all of them should be stored in CloudWatchLogs
        /// </summary>
        [Test]
        public void AddLogRequest_AllShouldbeCommited()
        {
            //arrange
            var cloudwatchClient = InitialiseClient();

            var logstream = Logstream + Guid.NewGuid();

            var cloudwatvhLogClient = new CloudWatchLogsClientWrapper(
                cloudwatchClient, 
                LogGroup,
                logstream);

            cloudwatvhLogClient.InitialiseLogStream();

            var logevents = Builder<InputLogEvent>
                .CreateListOfSize(20)
                .All()
                    .With(x => x.Timestamp = DateTime.Now.AddDays(-1))
                .Build()
                .ToList();

            //act
            cloudwatvhLogClient.AddLogRequest(logevents);

            Thread.Sleep(2000);

            var results = cloudwatchClient.GetLogEvents(new GetLogEventsRequest(
                LogGroup,
                logstream));

            //assert
            results.Events.Count.Should().Equal(20);
            results.Events.All(x => string.IsNullOrEmpty(x.Message));
            results.Events.All(x => x.Timestamp != new DateTime());
        }

        [Test]
        public void AddLogRequest_InitialiseLogStream_ShouldBeCalledINoLogStream()
        {
            //arrange
            var cloudwatchClient = InitialiseClient();

            var logstream = Logstream + Guid.NewGuid();

            var cloudwatvhLogClient = new CloudWatchLogsClientWrapper(
                cloudwatchClient,
                LogGroup,
                logstream);

            var logevents = Builder<InputLogEvent>
                .CreateListOfSize(20)
                .All()
                    .With(x => x.Timestamp = DateTime.Now.AddDays(-1))
                .Build()
                .ToList();

            //act
            try
            {
                cloudwatvhLogClient.AddLogRequest(logevents);
            }
            catch (ApplicationException ex)
            {
                if (ex.Message == "LogStream doesn't exist")
                    //assert
                    Assert.Pass();
            }
        }
    }
}
