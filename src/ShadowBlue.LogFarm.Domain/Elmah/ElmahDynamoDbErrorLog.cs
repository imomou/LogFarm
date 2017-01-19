using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using Amazon.DynamoDBv2.DocumentModel;
using Elmah;
using NLog.Common;
using ShadowBlue.LogFarm.Base.Properties;
using ShadowBlue.Repository;
using ShadowBlue.Repository.Models;
using ApplicationException = Elmah.ApplicationException;

namespace ShadowBlue.LogFarm.Domain.Elmah
{
    /// <summary>
    /// DynamoDb error logging for Elmah
    /// </summary>
    public class DynamoDbErrorLog : ErrorLog
    {
        private static readonly Dictionary<string, string> Keys = new Dictionary<string, string>();
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private readonly IRepository<ElmahError> _repository;
        private readonly string _applicationName;
        private readonly string[] _unwantedChars = {".", "."};
        private const int MaxAppNameLength = 60;
        
        public DynamoDbErrorLog(IDictionary config)
        {
            if (config == null)
                throw new ArgumentException("config is null");

            ApplicationName = (string)config["ddbAppName"] ?? string.Empty;
            Environment = (string)config["environment"] ?? string.Empty;

            if (ApplicationName.Length > MaxAppNameLength)
            {
                throw new ApplicationException(string.Format(
                    "Application name is too long. Maximum length allowed is {0} characters.",
                    MaxAppNameLength.ToString("N0")));
            }

            if (Environment.Length > MaxAppNameLength)
            {
                throw new ApplicationException(string.Format(
                    "Application name is too long. Maximum length allowed is {0} characters.",
                    MaxAppNameLength.ToString("N0")));
            }

            var applcationName = string.Format("{0}-{1}", ApplicationName, Environment);
            _applicationName = applcationName;

            _repository = new DynamoDbRepository<ElmahError>(Settings.Default, _applicationName);
        }

        public DynamoDbErrorLog(IDictionary config, IRepository<ElmahError> repository)
        {
            if (config == null)
                throw new ArgumentException("config is null");

            ApplicationName = (string)config["ddbAppName"] ?? string.Empty;
            Environment = (string)config["environment"] ?? string.Empty;

            _repository = repository;
        }

        public override string Name
        {
            get { return "DynamoDb Error Log"; }
        }

        public string Environment { get; set; }

        /// <summary>
        /// Log an error to Elmah
        /// </summary>
        /// <param name="error"> Error can be obtained from filtercontext</param>
        /// <returns> DatetimeID will be returned if it's log sucessfully committed</returns>
        public override string Log(Error error)
        {
            var elmahError = new ElmahError
            {
                DateTimeId = ToDateTimeId(error.Time),
                ApplicationName = _applicationName,
                Host = error.HostName,
                Type = error.Type,
                Source = error.Source,
                Message = error.Message,
                Detail = error.Detail,
                User = error.User,
                StatusCode = error.StatusCode,
                TimeUtc = error.Time.ToUniversalTime(),
                Form = ToDictionary(error.Form),
                ServerVariables = ToDictionary(error.ServerVariables),
                Cookies = ToDictionary(error.Cookies),
                QueryString = ToDictionary(error.QueryString)
            };
            _repository.Add(elmahError);

            return elmahError.DateTimeId;
        }

        public override ErrorLogEntry GetError(string id)
        {
            if (id == null)
                throw new ArgumentException("id");

            if (id.Length < 1)
                throw new ArgumentException(null, "id");

            var result = _repository.Get(id);
            return ToError(result);
        }

        public override int GetErrors(int pageIndex, int pageSize, IList errorEntryList)
        {
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException("pageIndex", pageIndex, null);

            if (pageSize < 0)
                throw new ArgumentOutOfRangeException("pageSize", pageSize, null);

            IEnumerable<ElmahError> items = null;
            if (pageIndex > 0)
            {
                var prevIndexKey = string.Format("{0}-{1}", pageSize, pageIndex - 1);
                var prevQueryKey = Keys.Keys
                    .Where(a => a.StartsWith(pageSize.ToString()))
                    .Where(a => string.CompareOrdinal(a, prevIndexKey) <= 0)
                    .OrderByDescending(a => a)
                    .FirstOrDefault();
                if (prevQueryKey != null)
                    items = _repository.GetAllWithQuery(ScanOperator.GreaterThan, null, Keys[prevQueryKey]);
            }
            else
            {
                items = _repository
                    .GetAll()
                    .OrderByDescending(a => a);
            }

            var elmahErrors = items == null ? 
                items.ToList() : 
                new List<ElmahError>();
 
            var count = elmahErrors.Count();

            var elmahErrorLogs = elmahErrors
                .Take(pageSize)
                .Select(ToError)
                .OrderByDescending(x => x.Id)
                .ToList();

            foreach (var item in elmahErrorLogs)
            {
                errorEntryList.Add(item);
            }

            if (count < pageSize)
                return 1;

            var indexKey = string.Format("{0}-{1}", pageSize, pageIndex);
            if (Keys.ContainsKey(indexKey))
            {
                Keys[indexKey] = elmahErrorLogs.Last().Id;
            }
            else if (count > 0)
            {
                Keys.Add(indexKey, elmahErrorLogs.Last().Id);
            }

            return count;
        }

        public ErrorLogEntry ToError(ElmahError elmahError)
        {
            Contract.Requires(elmahError != null);

            var error = new Error
            {
                ApplicationName = elmahError.ApplicationName,
                HostName = elmahError.Host,
                Type = elmahError.Type,
                Source = elmahError.Source,
                Message = elmahError.Message,
                Detail = elmahError.Detail,
                User = elmahError.User,
                Time = elmahError.TimeUtc,
                StatusCode = elmahError.StatusCode,
            };
            return new ErrorLogEntry(this, elmahError.DateTimeId, error);
        }

        private Dictionary<string, string> ToDictionary(NameValueCollection collection)
        {
            Contract.Requires(collection != null);
            Contract.Ensures(Contract.Result<Dictionary<string, string>>() != null);

            InternalLogger.Debug("DynamoDb {0}", "ToDictionary");

            var queryStringDoc = new Dictionary<string, string>();

            foreach (var queryStringKey in collection.AllKeys)
            {
                InternalLogger.Debug("DynamoDb {0}", queryStringKey);

                var values = collection.GetValues(queryStringKey);

                if (values == null)
                    continue;

                var key = _unwantedChars.Aggregate(queryStringKey, 
                    (current, wantedChar) => current.Replace(wantedChar, string.Empty));

                queryStringDoc.Add(key, string.Join(",", values));
            }
            return queryStringDoc;
        }

        public string ToDateTimeId(DateTime dateTime)
        {
            var dateTimeUtc = dateTime;
            if (dateTime.Kind != DateTimeKind.Utc)
                dateTimeUtc = dateTime.ToUniversalTime();

            if (dateTimeUtc <= UnixEpoch)
                dateTimeUtc = DateTime.UtcNow;

            var diff = (long)(dateTimeUtc - UnixEpoch).TotalSeconds;
            return string.Format("{0}-{1}", diff, Guid.NewGuid());
        }
    }
}