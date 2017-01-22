using System;
using System.Collections.Generic;
using System.Linq;
using Amazon;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Elmah;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ShadowBlue.LogFarm.Base;
using ShadowBlue.LogFarm.Domain.Elmah;
using ShadowBlue.LogFarm.Repository;
using ShadowBlue.LogFarm.Repository.Models;
using Should.Fluent;

namespace ShadowBlue.LogFarm.Domain.Test
{
    [TestFixture(Category = LogFarmApplication.TestCategories.Integration)]
    public class ElmahDynamoDbErrorLogFixture
    {
        private static readonly DynamoDBOperationConfig DefaultDbOperationConfig
            = new DynamoDBOperationConfig
            {
                OverrideTableName = Settings.Default.ElmahTableName,
                SkipVersionCheck = true,
                IndexName = "ApplicationName-DateTimeId-Index"
            };

        private readonly DynamoDBContext _ddb = new DynamoDBContext(RegionEndpoint.USWest1, DefaultDbOperationConfig);
        private readonly Dictionary<string, string> _config = new Dictionary<string, string> {
                {
                    "ddbAppName", Settings.Default.ApplicationName
                },
                {
                    "environment", "Dev"
                }
            };

        [SetUp]
        public void SetupPerTest()
        {
            var removeTargets = _ddb.FromScan<ElmahError>(
                new ScanOperationConfig(), DefaultDbOperationConfig).ToList();
            var removeAllWrite = _ddb.CreateBatchWrite<ElmahError>(DefaultDbOperationConfig);
            removeAllWrite.AddDeleteItems(removeTargets);
            _ddb.ExecuteBatchWrite(removeAllWrite);
        }

        [Test]
        public void Log_DateTimeIdTest()
        {
            //arrange
            var repository = new DynamoDbRepository<ElmahError>(
                    Settings.Default,_ddb, string.Empty
                );

            var dynamodb = new ElmahDynamoDbErrorLog(_config, repository);

            var error = Builder<Error>.CreateNew()
                .Build();

            //act
            var result = dynamodb.Log(error);

            var comparer = dynamodb.ToDateTimeId(error.Time);

            //assert
            result.Substring(0, result.IndexOf("-", StringComparison.Ordinal))
                .Should().Equal(comparer.Substring(0, result.IndexOf("-", StringComparison.Ordinal)));
        }

        [Test]
        public void Log_Test()
        {
            //arrange
            var repository = new DynamoDbRepository<ElmahError>(
                    Settings.Default, _ddb, string.Empty
                );

            var dynamodb = new ElmahDynamoDbErrorLog(_config, repository);

            var error = Builder<Error>.CreateNew()
                .With(x => x.ApplicationName = Settings.Default.ApplicationName)
                .Build();

            //act
            dynamodb.Log(error);

            var elmaherrors = _ddb.FromScan<ElmahError>(new ScanOperationConfig(), DefaultDbOperationConfig).ToList();

            //assert
            elmaherrors.Count.Should().Equal(1);
            elmaherrors.Single().ApplicationName.Should().Equal(error.ApplicationName);
        }

        [Test]
        public void ToError_Test()
        {
            //arrange
            var repository = new DynamoDbRepository<ElmahError>(
                    Settings.Default, _ddb, string.Empty
                );

            var dynamodb = new ElmahDynamoDbErrorLog(_config, repository);

            var datetimeId = dynamodb.ToDateTimeId(DateTime.UtcNow);

            var error = Builder<ElmahError>.CreateNew()
                .With(x => x.DateTimeId = datetimeId)
                .With(x => x.ApplicationName = Settings.Default.ApplicationName)
                .Build();
            
            _ddb.Save(error, DefaultDbOperationConfig);
            
            //act
            var result = dynamodb.GetError(datetimeId);

            //assert
            result.Error.ApplicationName.Should().Equal(error.ApplicationName);
            result.Id.Should().Equal(datetimeId);
        }

        /// <summary>
        /// Get Erros with pageIndex equals 0
        /// </summary>
        [Test]
        public void GetErrors_Test_ZeroPageIndex_Verify()
        {
            //arrange
            var container = new AutoMoq.AutoMoqer();

            var reposistoryMock = container.GetMock<IRepository<ElmahError>>();

            var target = container.Resolve<ElmahDynamoDbErrorLog>();

            //act
            target.GetErrors(0, It.IsAny<int>(), It.IsAny<List<ErrorLogEntry>>());

            //assert
            reposistoryMock.Verify(x => x.GetAll());
        }

        /// <summary>
        /// Get Erros with pageIndex equals 0
        /// </summary>
        [Test]
        public void GetErrors_Test_ZeroPageIndex()
        {
            //arrange
            var repository = new DynamoDbRepository<ElmahError>(
                    Settings.Default, _ddb, string.Empty
                );

            var dynamodb = new ElmahDynamoDbErrorLog(_config, repository);

            var errors = Builder<ElmahError>.CreateListOfSize(20)
                .All()
                .With(x => x.ApplicationName = Settings.Default.ApplicationName)
                .Build();

            var random = new Random();

            foreach (var error in errors)
            {
                var date = new DateTime(2017, 01, random.Next(1,31));
                var datetimeId = dynamodb.ToDateTimeId(date);

                error.TimeUtc = date;
                error.DateTimeId = datetimeId;
                _ddb.Save(error, DefaultDbOperationConfig);
            }

            var results = new List<ErrorLogEntry>();

            var resultCount = dynamodb.GetErrors(0, 4, results);

            resultCount.Should().Equal(20);
            results.Count().Should().Equal(4);
            results.OrderByDescending(x => x.Error.Time).SequenceEqual(results).Should().Be.True();
        }

        //[Test]
        public void GetErrors_Test_GreaterThanZeroPageIndex_Verify()
        {
            //arrange
            var container = new AutoMoq.AutoMoqer();

            var reposistoryMock = container.GetMock<IRepository<ElmahError>>();

            var target = container.Resolve<ElmahDynamoDbErrorLog>();

            var errors = Builder<ElmahError>.CreateListOfSize(5)
                .All()
                .With(x => x.ApplicationName = Settings.Default.ApplicationName)
                .Build();

            reposistoryMock.Setup(x => x.GetAll())
                .Returns(errors);

            //act
            target.GetErrors(1, 4, new List<ErrorLogEntry>());
            target.GetErrors(1, 4, new List<ErrorLogEntry>());

            ////assert
            //reposistoryMock.Verify(x => x.GetAllWithQuery(
            //    It.Is<ScanOperator>(scanop => scanop == ScanOperator.GreaterThan),
            //    It.IsAny<ConditionalOperatorValues>(),
            //    It.IsAny<object>();
            //    //));

            reposistoryMock.Verify(x => x.GetAllWithQuery(
                It.IsAny<ScanOperator>(),
                It.IsAny<ConditionalOperatorValues>(),
                It.IsAny<object>()));
            //));

        }
    }
}
