using Spring.Context;
using Spring.Context.Support;
using Spring.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Optimization;
using System.Web.Routing;

namespace API
{
    public class WebApiApplication : SpringMvcApplication
    {
        protected void Application_Start()
        {
            IApplicationContext ctx = ContextRegistry.GetContext();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            var configuration = GlobalConfiguration.Configuration;
        }

        protected void Application_BeginRequest()
        {
            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                //
                //These headers are handling the "pre-flight" OPTIONS call sent by the browser

                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS, DELETE");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
                HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");

                HttpContext.Current.Response.End();
            }
        }
    }
}
