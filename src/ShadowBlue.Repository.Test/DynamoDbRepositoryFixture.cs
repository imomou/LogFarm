using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Moq;
using NUnit.Framework;
using ShadowBlue.LogFarm.Base.Properties;
using ShadowBlue.Repository.Models;

namespace ShadowBlue.Repository.Test
{
    [TestFixture]
    public class DynamoDbRepositoryFixture
    {
        private static readonly DynamoDBOperationConfig DefaultDbOperationConfig
            = new DynamoDBOperationConfig
            {
                OverrideTableName = Settings.Default.ElmahTableName,
                ConsistentRead = true,
                SkipVersionCheck = true
            };

        private readonly DynamoDBContext _ddb = new DynamoDBContext();

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
            var repositoryMock = container.GetMock<IRepository<ElmahError>>();
            var target = container.Resolve<DynamoDbRepository<ElmahError>>();

            var fakeEpochId = string.Format("{0}-{1}", "!23", Guid.NewGuid());

            var error = new ElmahError
            {
                ApplicationName = Settings.Default.ApplicationName,
                DateTimeId = fakeEpochId
            };

            target.Add(error);

            repositoryMock.Verify( x =>
                    x.Add(It.Is<ElmahError>( err =>
                        error.ApplicationName == Settings.Default.ApplicationName &&
                        error.DateTimeId == fakeEpochId 
                    )
                )
                , Times.Once()
            );
        }
    }
}
