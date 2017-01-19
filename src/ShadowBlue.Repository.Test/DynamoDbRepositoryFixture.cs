using System;
using System.Collections.Generic;
using System.Linq;
using Amazon;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ShadowBlue.Repository.Models;
using ShadowBlue.LogFarm.Base;
using ShadowBlue.LogFarm.Base.Properties;
using Should.Fluent;

namespace ShadowBlue.Repository.Test
{
    [TestFixture(Category = LogFarmApplication.TestCategories.Integration)]
    public class DynamoDbRepositoryFixture
    {
        private static readonly DynamoDBOperationConfig DefaultDbOperationConfig
            = new DynamoDBOperationConfig
            {
                OverrideTableName = Settings.Default.ElmahTableName,
                SkipVersionCheck = true,
                IndexName = "ApplicationName-DateTimeId-Index"
            };

        private readonly DynamoDBContext _ddb = new DynamoDBContext(RegionEndpoint.USWest1, DefaultDbOperationConfig);

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
            var settingMock = container.GetMock<ISettings>();

            settingMock.Setup(x => x.ApplicationName)
                .Returns(Settings.Default.ApplicationName);
            settingMock.Setup(x => x.ElmahTableName)
                .Returns(Settings.Default.ElmahTableName);

            container.SetInstance("dummy");

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
                    It.Is<DynamoDBOperationConfig>(config => config.OverrideTableName == Settings.Default.ElmahTableName)
                ),
                Times.Once()
            );
        }

        [Test]
        public void RepositoryAdd_VerifyAdd()
        {
            var container = new AutoMoq.AutoMoqer();

            var settingMock = container.GetMock<ISettings>();

            container.SetInstance<IDynamoDBContext>(_ddb);

            settingMock.Setup(x => x.ApplicationName)
                .Returns(Settings.Default.ApplicationName);
            settingMock.Setup(x => x.ElmahTableName)
                .Returns(Settings.Default.ElmahTableName);

            container.SetInstance("dummy");

            var target = container.Resolve<DynamoDbRepository<ElmahError>>();

            var fakeEpochId = string.Format("{0}-{1}", "123", Guid.NewGuid());

            var error = Builder<ElmahError>.CreateNew()
                .With(x => x.ApplicationName = Settings.Default.ApplicationName)
                .With(x => x.DateTimeId = fakeEpochId)
                .Build();

            target.Add(error);

            var results = _ddb.FromScan<ElmahError>(
                new ScanOperationConfig(), DefaultDbOperationConfig).ToList();

            results.Single().DateTimeId.Should().Equal(fakeEpochId);
        }

        [Test]
        public void RepositoryDelete_Verify()
        {
            var container = new AutoMoq.AutoMoqer();
            var repositoryMock = container.GetMock<IDynamoDBContext>();
            var settingMock = container.GetMock<ISettings>();

            settingMock.Setup(x => x.ApplicationName)
                .Returns(Settings.Default.ApplicationName);
            settingMock.Setup(x => x.ElmahTableName)
                .Returns(Settings.Default.ElmahTableName);

            container.SetInstance("dummy");

            var target = container.Resolve<DynamoDbRepository<ElmahError>>();

            var fakeEpochId = string.Format("{0}-{1}", "123", Guid.NewGuid());

            target.Delete(fakeEpochId);

            repositoryMock.Verify(x =>
                    x.Delete<ElmahError>(
                        It.Is<string>(err =>
                            err == fakeEpochId
                        ),
                        It.Is<string>(asd =>
                            asd == Settings.Default.ApplicationName
                        ),
                        It.Is<DynamoDBOperationConfig>(config => 
                            config.OverrideTableName == Settings.Default.ElmahTableName)
                    ),
                Times.Once()
            );
        }

        [Test]
        public void RepositoryDelete_VerifyDelete()
        {
            var container = new AutoMoq.AutoMoqer();

            var settingMock = container.GetMock<ISettings>();

            settingMock.Setup(x => x.ApplicationName)
                .Returns(Settings.Default.ApplicationName);
            settingMock.Setup(x => x.ElmahTableName)
                .Returns(Settings.Default.ElmahTableName);

            container.SetInstance<IDynamoDBContext>(_ddb);
            container.SetInstance("dummy");

            var target = container.Resolve<DynamoDbRepository<ElmahError>>();

            var fakeEpochId = string.Format("{0}-{1}", "123", Guid.NewGuid());

            var error = Builder<ElmahError>.CreateNew()
                .With(x => x.ApplicationName = Settings.Default.ApplicationName)
                .With(x => x.DateTimeId = fakeEpochId)
                .Build();

            _ddb.Save(error, DefaultDbOperationConfig);

            target.Delete(fakeEpochId);

            var results = _ddb.FromScan<ElmahError>(
                new ScanOperationConfig(), DefaultDbOperationConfig).ToList();

            results.Count().Should().Be.LessThanOrEqualTo(0);
        }

        [Test]
        public void RepositoryGet_Verify()
        {
            var container = new AutoMoq.AutoMoqer();
            var repositoryMock = container.GetMock<IDynamoDBContext>();
            var settingMock = container.GetMock<ISettings>();

            settingMock.Setup(x => x.ApplicationName)
                .Returns(Settings.Default.ApplicationName);
            settingMock.Setup(x => x.ElmahTableName)
                .Returns(Settings.Default.ElmahTableName);

            container.SetInstance("dummy");

            var target = container.Resolve<DynamoDbRepository<ElmahError>>();

            var fakeEpochId = string.Format("{0}-{1}", "123", Guid.NewGuid());

            target.Get(fakeEpochId);

            repositoryMock.Verify(x =>
                    x.Load<ElmahError>(
                        It.Is<string>(err =>
                            err == fakeEpochId
                        ),
                         It.Is<string>(err =>
                            err == Settings.Default.ApplicationName
                        ),
                        It.Is<DynamoDBOperationConfig>(config => 
                            config.OverrideTableName == Settings.Default.ElmahTableName
                        )
                ),
                Times.Once()
            );
        }

        [Test]
        public void RepositoryGet_VerifyGet()
        {
            var container = new AutoMoq.AutoMoqer();

            var settingMock = container.GetMock<ISettings>();

            settingMock.Setup(x => x.ApplicationName)
                .Returns(Settings.Default.ApplicationName);
            settingMock.Setup(x => x.ElmahTableName)
                .Returns(Settings.Default.ElmahTableName);

            container.SetInstance("dummy");
            container.SetInstance<IDynamoDBContext>(_ddb);

            var target = container.Resolve<DynamoDbRepository<ElmahError>>();

            var fakeEpochId = string.Format("{0}-{1}", "123", Guid.NewGuid());

            var error = Builder<ElmahError>.CreateNew()
                .With(x => x.ApplicationName = Settings.Default.ApplicationName)
                .With(x => x.DateTimeId = fakeEpochId)
                .Build();

            _ddb.Save(error, DefaultDbOperationConfig);

            var result = target.Get(fakeEpochId);

            result.DateTimeId.Should().Equal(fakeEpochId);
        }

        [Test]
        public void RepositoryGetAll_Verify()
        {
            var container = new AutoMoq.AutoMoqer();
            var repositoryMock = container.GetMock<IDynamoDBContext>();
            var settingMock = container.GetMock<ISettings>();

            settingMock.Setup(x => x.ApplicationName)
                .Returns(Settings.Default.ApplicationName);
            settingMock.Setup(x => x.ElmahTableName)
                .Returns(Settings.Default.ElmahTableName);

            container.SetInstance("dummy");

            var target = container.Resolve<DynamoDbRepository<ElmahError>>();

            target.GetAll();

            repositoryMock.Verify(x =>
                    x.FromScan<ElmahError>(
                        It.IsAny<ScanOperationConfig>(),
                        It.Is<DynamoDBOperationConfig>(config =>
                            config.OverrideTableName == Settings.Default.ElmahTableName
                        )
                ),
                Times.Once()
            );
        }

        [Test]
        public void RepositoryGetAll_VerifyGetAll()
        {
            var container = new AutoMoq.AutoMoqer();

            var settingMock = container.GetMock<ISettings>();

            settingMock.Setup(x => x.ElmahTableName)
                .Returns(Settings.Default.ElmahTableName);

            container.SetInstance("dummy");
            container.SetInstance<IDynamoDBContext>(_ddb);

            var target = container.Resolve<DynamoDbRepository<ElmahError>>();

            var errors = Builder<ElmahError>.CreateListOfSize(3)
                .All()
                .With(x => x.ApplicationName = Settings.Default.ApplicationName)
                .Build();

            foreach (var error in errors)
            {
                _ddb.Save(error, DefaultDbOperationConfig);
            }

            target.GetAll().Count().Should().Equal(3);
        }

        [Test]
        public void RepositoryGetAllWithQuery_Verify()
        {
            var container = new AutoMoq.AutoMoqer();
            var repositoryMock = container.GetMock<IDynamoDBContext>();
            var settingMock = container.GetMock<ISettings>();

            settingMock.Setup(x => x.ApplicationName)
                .Returns(Settings.Default.ApplicationName);
            settingMock.Setup(x => x.ElmahTableName)
                .Returns(Settings.Default.ElmahTableName);

            container.SetInstance("dummy");

            var target = container.Resolve<DynamoDbRepository<ElmahError>>();

            target.GetAllWithQuery(ScanOperator.GreaterThan, It.IsAny<ConditionalOperatorValues>(), It.IsAny<object>());

            repositoryMock.Verify(x =>
                    x.Query<ElmahError>(
                        It.Is<string>(y => y == Settings.Default.ApplicationName) ,
                        It.Is<DynamoDBOperationConfig>(config =>
                            config.OverrideTableName == Settings.Default.ElmahTableName &&
                            config.QueryFilter.Single().Operator == ScanOperator.GreaterThan &&
                            config.QueryFilter.Single().PropertyName == "DateTimeId"
                        )
                ),
                Times.Once()
            );
        }

        [Test]
        public void RepositoryGetAllWithQuery_VerifyGetAllWithQuery()
        {
            var container = new AutoMoq.AutoMoqer();

            var settingMock = container.GetMock<ISettings>();

            settingMock.Setup(x => x.ApplicationName)
                .Returns(Settings.Default.ApplicationName);
            settingMock.Setup(x => x.ElmahTableName)
                .Returns(Settings.Default.ElmahTableName);

            container.SetInstance("dummy");
            container.SetInstance<IDynamoDBContext>(_ddb);

            var target = container.Resolve<DynamoDbRepository<ElmahError>>();

            var errors = Builder<ElmahError>.CreateListOfSize(3)
                .All()
                .With(x => x.ApplicationName = Settings.Default.ApplicationName)
                .Build();

            var i = 0;
            foreach (var error in errors)
            {
                error.DateTimeId = i + error.DateTimeId;
                _ddb.Save(error, DefaultDbOperationConfig);

                i++;
            }

            var results = target.GetAllWithQuery(ScanOperator.GreaterThan, It.IsAny<ConditionalOperatorValues>(), "1");

            results.Count().Should().Equal(2);
        }
    }
}
