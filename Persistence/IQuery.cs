using System;
using System.Collections.Generic;
using System.Text;

namespace Persistence
{
    public interface IQuery<out T>
    {
        T Execute(ISession session);        
    }
}
