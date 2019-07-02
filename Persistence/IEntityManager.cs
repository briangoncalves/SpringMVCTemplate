using System;
using System.Collections.Generic;
using System.Text;

namespace Persistence
{
    public interface IEntityManager
    {
        string GetConnectionString();

        T Query<T>(IQuery<T> query);

        void Execute(ICommand command, int? contextId = null, bool createNew = false);

        T Execute<T>(ICommand<T> command, int? contextId = null, bool createNew = false);

        ISession Session { get; }

        void InitializeSession();
    }
}
