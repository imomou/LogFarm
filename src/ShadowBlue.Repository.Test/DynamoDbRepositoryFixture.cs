using System;
using System.Linq;
using Amazon;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ShadowBlue.Repository.Models;
using ShadowBlue.LogFarm.Base;

namespace ShadowBlue.Repository.Test
{
    [TestFixture(Category = LogFarmApplication.TestCategories.Integration)]
    public class DynamoDbRepositoryFixture
    {
        private static readonly DynamoDBOperationConfig DefaultDbOperationConfig
            = new DynamoDBOperationConfig
            {
                OverrideTableName = Settings.Default.ElmahTableName,
                ConsistentRead = true,
                SkipVersionCheck = true
            };

        private readonly DynamoDBContext _ddb = new DynamoDBContext(RegionEndpoint.APSoutheast2, DefaultDbOperationConfig);

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
        public void RepositoryAdd_Verify()
        {
            var container = new AutoMoq.AutoMoqer();
            var repositoryMock = container.GetMock<IDynamoDBContext>();

            container.SetInstance(Settings.Default.ElmahTableName);
            var target = container.Resolve<DynamoDbRepository<ElmahError>>();

            var fakeEpochId = string.Format("{0}-{1}", "123", Guid.NewGuid());

            var error = Builder<ElmahError>.CreateNew()
                .With(x => x.ApplicationName = Settings.Default.ApplicationName)
                .With(x => x.DateTimeId = fakeEpochId)
                .Build();

            target.Add(error);

            repositoryMock.Verify(x =>
                    x.Save(It.Is<ElmahError>(err =>
                        error.ApplicationName == Settings.Default.ApplicationName &&
                        error.DateTimeId == fakeEpochId
                    ), 
                    It.IsAny<DynamoDBOperationConfig>()
                ),
                Times.Once()
            );
        }
    }
}
