using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spring.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    [TestClass]
    public class SpringTestBase : SpringLoadTestBase
    {
        [TestInitialize]
        public virtual void Initialize()
        {
            XmlConfigurator.Configure();

        }

        public IApplicationContext Context
        {
            get { return applicationContext; }
        }



        protected override string[] ConfigLocations
        {
            get { return new[] { "assembly://Test/Test/spring-config.xml" }; }
        }
    }
}
