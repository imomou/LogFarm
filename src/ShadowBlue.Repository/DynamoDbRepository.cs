using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Configuration;
using Amazon;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace ShadowBlue.Repository
{
    public class DynamoDbRepository<T> : IRepository<T> where T : class, IDisposable
    {
        private readonly DynamoDBContext _ddbcontext;
        private readonly string _applicationName;
        private readonly string _ddbTableName;
        private const string IndexName = "ApplicationName-DateTimeId-Index";

        public DynamoDbRepository(string applicationName, string ddbTableName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(applicationName));

            _applicationName = applicationName;
            _ddbTableName = ddbTableName;

            var awskey = WebConfigurationManager.AppSettings["AWSAccessKey"] ?? string.Empty;
            var awsSecret = WebConfigurationManager.AppSettings["AWSSecretKey"] ??
                       string.Empty;

            if (string.IsNullOrEmpty(awskey) || string.IsNullOrEmpty(awsSecret))
                _ddbcontext = new DynamoDBContext(AWSClientFactory.CreateAmazonDynamoDBClient());
            else
                _ddbcontext = new DynamoDBContext(AWSClientFactory.CreateAmazonDynamoDBClient(awskey, awsSecret));
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
            _ddbcontext.Delete(id, new DynamoDBOperationConfig
            {
                OverrideTableName = _ddbTableName
            });
        }

        public T Get(string id)
        {
            return _ddbcontext.Load<T>(id, new DynamoDBOperationConfig
            {
                OverrideTableName = _ddbTableName
            });
        }

        public IEnumerable<T> GetAll(string applicationName)
        {
            return _ddbcontext.Query<T>(applicationName, new DynamoDBOperationConfig
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
                IndexName = IndexName
            };
            config.QueryFilter.AddRange(new List<ScanCondition>
            {
                new ScanCondition("DateTimeId", scanOperator, values)
            });

            return _ddbcontext.Query<T>(_applicationName, config);
        }
    }
}