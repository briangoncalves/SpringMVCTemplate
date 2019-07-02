using System;
using System.Collections.Generic;
using System.Text;

namespace Business
{
    public interface ISampleManager
    {
        IList<Domain.Sample> GetSamples(int id);
    }
}
