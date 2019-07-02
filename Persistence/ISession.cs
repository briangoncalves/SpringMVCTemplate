using System;
using System.Collections.Generic;
using System.Text;

namespace Persistence
{
    public interface ISession
    {
        IEnumerable<T> Query<T>(string query, object param = null, int? timeout = null, int? contextId = null
            , bool createNew = false);

        void Execute(string query, object param = null, int? contextId = null, bool createNew = false);

        object ExecuteScalar(string query, object param = null, int? contextId = null, bool createNew = false);

        int BeginTransaction(int? contextId = null, bool createNew = false);

        void Commit(int? contextId = null);

        void Rollback(int? contextId = null);

        void Dispose(int? contextId = null);

        List<int> GetSessionKeys();
    }
}
