using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Persistence.Queries.Calendar
{
    public class GetSamples : IQuery<IList<Domain.Sample>>
    {
        private int Id { get; set; }
        public GetSamples(int id)
        {
            this.Id = id;
        }

        public IList<Domain.Sample> Execute(ISession session)
        {
            var query = $@"
                    SELECT  ID,
                            NAME
                    FROM TB_SAMPLES
                    WHERE ID = {this.Id}
";

            var result = session.Query<Domain.Sample>(query).ToList();
            return result;
        }
    }
}
