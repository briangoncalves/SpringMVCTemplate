using Domain;
using Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business
{
    public class SampleManager : ISampleManager
    {
        public IEntityManager EntityManager;

        public IList<Sample> GetSamples(int id)
        {
            var session = EntityManager.Session;
            try
            {
                return EntityManager.Query(new Persistence.Queries.Calendar.GetSamples(id));
            }
            finally
            {
                session.Dispose();
            }
        }
    }
}
