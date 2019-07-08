using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API.Controllers
{
    [Route("api/token")]
    [AllowAnonymous]
    public class TokenController : ApiController
    {
        [HttpGet]
        [Route("getToken")]
        public HttpResponseMessage GetToken(string username, string password)
        {
            if (CheckUser(username, password))
            {
                return Request.CreateResponse(HttpStatusCode.OK, JwtManager.GenerateToken(username));
            }

            return Request.CreateResponse(HttpStatusCode.Unauthorized);
        }

        private bool CheckUser(string username, string password)
        {
            if ((username == "brian") && (password == "test"))
            // should check in the database
                return true;
            return false;
        }
    }
}
