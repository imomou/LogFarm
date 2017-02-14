using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Configuration;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using ShadowBlue.LogFarm.Base.Properties;

namespace ShadowBlue.LogFarm.Repository
{
    public class DynamoDbRepository<T> : IRepository<T> where T : class
    {
        private readonly IDynamoDBContext _ddbcontext;
        private readonly string _applicationName;
        private readonly string _ddbTableName;
        private const string IndexName = "ApplicationName-DateTimeId-Index";

        public DynamoDbRepository(string ddbTableName, string applicationName)
        {
            Contract.Requires(
                    !string.IsNullOrEmpty(ddbTableName) &&
                    !string.IsNullOrEmpty(applicationName)
                );

            _ddbTableName = ddbTableName;
            _applicationName = applicationName;

            var awskey = WebConfigurationManager.AppSettings["AWSAccessKey"] ?? string.Empty;
            var awsSecret = WebConfigurationManager.AppSettings["AWSSecretKey"] ?? string.Empty;
            var regionStr = WebConfigurationManager.AppSettings["AWSRegion"] ?? string.Empty;

            var region = RegionEndpoint.GetBySystemName(regionStr);

            if (region.DisplayName.ToLower().Contains("unknown"))
                throw new ApplicationException(string.Format("Invalid region configured {0}", regionStr));
                
            if (string.IsNullOrEmpty(awskey) && string.IsNullOrEmpty(awsSecret))
                _ddbcontext = new DynamoDBContext(region);
            else
                _ddbcontext = new DynamoDBContext(new AmazonDynamoDBClient(awskey, awsSecret, region));
        }

        public DynamoDbRepository(ISettings settings, IDynamoDBContext ddbcontext, string dummy)
        {
            _ddbcontext = ddbcontext;
            _ddbTableName = settings.ElmahTableName;
            _applicationName = settings.ApplicationName;
        }

        public void Add(T entity)
        {
            _ddbcontext.Save(entity, new DynamoDBOperationConfig
            {
                OverrideTableName = _ddbTableName
            });
        }

        public void Delete(string id)
        {
            _ddbcontext.Delete<T>(
                id,
                _applicationName, 
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _ddbTableName
                }
            );
        }

        public T Get(string id)
        {
            return _ddbcontext.Load<T>(
                id, 
                _applicationName,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _ddbTableName
                }
            );
        }

        public IEnumerable<T> GetAll()
        {
            return _ddbcontext.FromScan<T>(new ScanOperationConfig(), new DynamoDBOperationConfig
            {
                OverrideTableName = _ddbTableName,
                IndexName = IndexName
            });
        }

        public IEnumerable<T> GetAllWithQuery(ScanOperator scanOperator, ConditionalOperatorValues? condition, params object[] values)
        {
            var config = new DynamoDBOperationConfig
            {
                OverrideTableName = _ddbTableName,
                IndexName = IndexName,          
            };
            config.QueryFilter.AddRange(new List<ScanCondition>
            {
                new ScanCondition("DateTimeId", scanOperator, values)
            });

            return _ddbcontext.Query<T>(_applicationName, config);
        }

        public void Dispose()
        {
            if (_ddbcontext != null) _ddbcontext.Dispose();
        }
    }
}