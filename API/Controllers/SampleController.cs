using Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace API.Controllers
{
    [RoutePrefix("api/sample")]    
    [Filters.JwtAuthentication]
    public class SampleController : ApiController
    {

        public ISampleManager SampleManager { get; set; }

        public SampleController()
        {
        }

        /// <summary>
        /// Returns a list of events
        /// </summary>
        /// <returns>List of events</returns>
        [HttpGet]
        [Route("list")]
        public HttpResponseMessage Get(int id)
        {
            //var data = SampleManager.GetSamples(id);            
            var data = "worked";
            return Request.CreateResponse(HttpStatusCode.OK, data);
        }
    }
}