using System;
using System.Collections.Generic;
using System.Text;

namespace Persistence
{
    public interface ICommand
    {
        void Execute(ISession session, int? contextId = null);
    }

    public interface ICommand<T>
    {
        T Execute(ISession session, int? contextId = null);
    }
}
