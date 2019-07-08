using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Web.Http;
using System.Linq;

namespace Test
{
    [TestClass]
    public class TokenTest : SpringTestBase
    {
        [TestMethod]
        public void FirstTesT()
        {
            var result = 1 + 1;
            Assert.AreEqual(result, 2);
        }

        public API.Controllers.TokenController TokenController { get; set; }
        public API.Controllers.SampleController SampleController { get; set; }

        [TestMethod]
        public void GetValidToken()
        {
            // Arrange            
            TokenController.Request = new HttpRequestMessage();
            TokenController.Configuration = new HttpConfiguration();

            // Act
            var response = TokenController.GetToken("brian", "test");

            // Assert
            Assert.AreEqual(response.StatusCode, System.Net.HttpStatusCode.OK);
            Assert.IsTrue(response.TryGetContentValue<string>(out string token));
            Assert.AreNotEqual(token, "");
        }

        [TestMethod]
        public void GetInvalidTokenNotAuthorized()
        {
            // Arrange            
            TokenController.Request = new HttpRequestMessage();
            TokenController.Configuration = new HttpConfiguration();

            // Act
            var response = TokenController.GetToken("test", "test");

            // Assert
            Assert.AreEqual(response.StatusCode, System.Net.HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public void SampleCallJwtToken()
        {
            // Arrange            
            TokenController.Request = new HttpRequestMessage();
            TokenController.Configuration = new HttpConfiguration();

            // Act
            var response = TokenController.GetToken("test", "test");
            response.TryGetContentValue<string>(out string token);

            SampleController.Request = new HttpRequestMessage();
            if (token != null)
                SampleController.Request.Headers.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            SampleController.Configuration = new HttpConfiguration();

            var sampleResponse = SampleController.Get(1);
            sampleResponse.TryGetContentValue<string>(out string sample);

            Assert.AreEqual(sample, "worked");
        }

        [TestMethod]
        public void Controller_Has_JwtFilterAttribute()
        {
            var attribute = typeof(API.Controllers.SampleController)
                            .GetCustomAttributes(typeof(API.Filters.JwtAuthenticationAttribute), false)
                            .SingleOrDefault();
            Assert.AreNotEqual(attribute, null);
        }
    }
}
