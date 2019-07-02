using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;

namespace Persistence.DapperSupport
{
    public class Session : ISession
    {
        private readonly string connectionString;
        private readonly Dictionary<int, IDapperContext> contexts;
        int timeout = 300;

        public Session(string connectionString)
        {
            contexts = new Dictionary<int, IDapperContext>();
            this.connectionString = connectionString;
            var connection = new SqlConnectionStringBuilder(this.connectionString);
            timeout = connection.ConnectTimeout;
        }

        public Session(IDapperContext context)
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            contexts = new Dictionary<int, IDapperContext> { { id, context } };
        }

        private IDapperContext GetContext(int? contextId = null, bool createNew = false)
        {
            var id = contextId ?? Thread.CurrentThread.ManagedThreadId;
            if(contexts.ContainsKey(id) && !createNew)
            {
                return contexts[id];
            }
            var newContext = new DapperContext(this.connectionString);
            id = createNew && contexts.ContainsKey(id) ? newContext.GetHashCode() : id;
            contexts[id] = newContext;
            return contexts[id];
        }

        public int BeginTransaction(int? contextId = null, bool createNew = false)
        {
            var id = contextId ?? (createNew ? DateTime.Now.GetHashCode() : Thread.CurrentThread.ManagedThreadId);
            var context = GetContext(id, createNew);
            if(!context.InTransaction())
            {
                context.BeginTransaction();
            }
            return id;
        }

        private void RemoveContext(int? contextId = null)
        {
            var id = contextId ?? Thread.CurrentThread.ManagedThreadId;

            if (contexts.ContainsKey(id))
            {
                contexts[id].Dispose();
                contexts[id] = null;
                contexts.Remove(id);
            }
        }

        public void Commit(int? contextId = null)
        {
            GetContext(contextId).Commit();
            RemoveContext(contextId);
        }

        public void Dispose(int? contextId = null)
        {
            GetContext(contextId).Dispose();
            RemoveContext(contextId);
        }

        public void Execute(string query, object param = null, int? contextId = null, bool createNew = false)
        {
            GetContext(contextId, createNew).Transaction(t => GetContext(contextId, createNew).Connection.Execute(query, param, t));
        }

        public object ExecuteScalar(string query, object param = null, int? contextId = null, bool createNew = false)
        {
            return GetContext(contextId, createNew).Transaction(t => GetContext(contextId, createNew).Connection.ExecuteScalar(query, param, t));
        }

        public List<int> GetSessionKeys()
        {
            var keys = new List<int>();
            foreach(var key in contexts.Keys)
            {
                keys.Add(key);
            }
            return keys;
        }

        public IEnumerable<T> Query<T>(string query, object param = null, int? timeout = null, int? contextId = null, bool createNew = false)
        {
            var queryTimeout = this.timeout;

            if(timeout.HasValue)
            {
                queryTimeout = timeout.GetValueOrDefault();
            }

            if(GetContext(contextId, createNew).InTransaction())
            {
                return GetContext(contextId, createNew).Transaction(t =>
                {
                    var result = GetContext(contextId, createNew).Connection.Query<T>(query, param, t, true, queryTimeout);
                    return result;
                });
            }
            else
            {
                var result = GetContext(contextId, createNew).Connection.Query<T>(query, param, null, true, queryTimeout);
                return result;
            }
        }

        public void Rollback(int? contextId = null)
        {
            GetContext(contextId).Rollback();
            RemoveContext(contextId);
        }
    }
}
