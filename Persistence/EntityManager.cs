using System;
using System.Collections.Generic;
using System.Text;

namespace Persistence
{
    public class EntityManager : IEntityManager
    {
        public ISession Session { get; private set; }

        public string ConnectionString { get; set; }

        public void Initialize()
        {
            Session = new DapperSupport.Session(ConnectionString);
        }

        public void Execute(ICommand command, int? contextId = null, bool createNew = false)
        {
            command.Execute(Session, contextId);
        }

        public T Execute<T>(ICommand<T> command, int? contextId = null, bool createNew = false)
        {
            var result = command.Execute(Session, contextId);
            return result;
        }

        public string GetConnectionString()
        {
            return this.ConnectionString;
        }

        public void InitializeSession()
        {
            this.Initialize();
        }

        public T Query<T>(IQuery<T> query)
        {
            var result = query.Execute(Session);
            return result;
        }
    }
}
