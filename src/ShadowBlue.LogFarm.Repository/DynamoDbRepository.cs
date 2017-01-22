using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Configuration;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using ShadowBlue.LogFarm.Base.Properties;

namespace ShadowBlue.LogFarm.Repository
{
    public class DynamoDbRepository<T> : IRepository<T> where T : class
    {
        private readonly IDynamoDBContext _ddbcontext;
        private readonly ISettings _settings;
        private readonly string _applicationName;
        private const string IndexName = "ApplicationName-DateTimeId-Index";

        public DynamoDbRepository(ISettings settings, string applicationName)
        {
            Contract.Requires(
                    !string.IsNullOrEmpty(settings.ApplicationName) &&
                    !string.IsNullOrEmpty(settings.ElmahTableName)
                );

            _settings = settings;
            _applicationName = string.IsNullOrEmpty(applicationName) ? 
                _settings.ApplicationName : 
                applicationName;

            var awskey = WebConfigurationManager.AppSettings["AWSAccessKey"] ?? string.Empty;
            var awsSecret = WebConfigurationManager.AppSettings["AWSSecretKey"] ?? string.Empty;

            if (string.IsNullOrEmpty(awskey) && string.IsNullOrEmpty(awsSecret))
                _ddbcontext = new DynamoDBContext();
            else
                _ddbcontext = new DynamoDBContext(new AmazonDynamoDBClient(awskey, awsSecret));
        }

        public DynamoDbRepository(ISettings settings, IDynamoDBContext ddbcontext, string dummy)
        {
            _settings = settings;
            _ddbcontext = ddbcontext;
            _applicationName = settings.ApplicationName;
        }

        public void Add(T entity)
        {
            _ddbcontext.Save(entity, new DynamoDBOperationConfig
            {
                OverrideTableName = _settings.ElmahTableName
            });
        }

        public void Delete(string id)
        {
            _ddbcontext.Delete<T>(
                id,
                _applicationName, 
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _settings.ElmahTableName
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
                    OverrideTableName = _settings.ElmahTableName
                }
            );
        }

        public IEnumerable<T> GetAll()
        {
            return _ddbcontext.FromScan<T>(new ScanOperationConfig(), new DynamoDBOperationConfig
            {
                OverrideTableName = _settings.ElmahTableName,
                IndexName = IndexName
            });
        }

        public IEnumerable<T> GetAllWithQuery(ScanOperator scanOperator, ConditionalOperatorValues? condition, params object[] values)
        {
            var config = new DynamoDBOperationConfig
            {
                OverrideTableName = _settings.ElmahTableName,
                IndexName = IndexName,          
            };
            config.QueryFilter.AddRange(new List<ScanCondition>
            {
                new ScanCondition("DateTimeId", scanOperator, values)
            });

            return _ddbcontext.Query<T>(_settings.ApplicationName, config);
        }

        public void Dispose()
        {
            if (_ddbcontext != null) _ddbcontext.Dispose();
        }
    }
}