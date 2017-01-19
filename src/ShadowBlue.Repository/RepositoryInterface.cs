using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Amazon.DynamoDBv2.DocumentModel;

namespace ShadowBlue.Repository
{
    [ContractClass(typeof(RepositoryInterface<>))]
    public interface IRepository<T> : IDisposable where T : class
    {
        void Add(T entity);
        void Delete(string id);
        T Get(string id);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAllWithQuery(ScanOperator scanOperator, ConditionalOperatorValues? condition, params object[] values);
    }

    [ContractClassFor(typeof(IRepository<>))]
    public abstract class RepositoryInterface<T> : IRepository<T> where T : class, IDisposable
    {
        public void Add(T entity)
        {
            Contract.Requires(entity != null);
        }

        public void Delete(string id)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
        }

        public IEnumerable<T> GetAllWithQuery(ScanOperator scanOperator, ConditionalOperatorValues? condition, params object[] values)
        {
            Contract.Requires(values != null);
            return null;
        }

        public T Get(string id)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Ensures(Contract.Result<T>() != null);
            return default(T);
        }

        public IEnumerable<T> GetAll()
        {
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);
            return default(IEnumerable<T>);
        }

        public void Dispose()
        {
        }
    }
}