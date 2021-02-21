using $ext_rootnamespace$.Controllers;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Xunit;

namespace UnitTests.Controllers
{
    public class ValidateModelAttributeTests
    {
        private readonly HttpActionContext actionContext;
        private readonly HttpControllerContext controllerContext;
        private readonly HttpRequestMessage request;

        public ValidateModelAttributeTests()
        {
            this.actionContext = new HttpActionContext();
            this.controllerContext = new HttpControllerContext();
            this.request = new HttpRequestMessage();
            this.controllerContext.Request = this.request;
            this.actionContext.ControllerContext = this.controllerContext;
        }

        [Fact]
        public void OnActionExecuting_Invalid_Works()
        {
            var attribute = new ValidateModelAttribute();
            this.actionContext.ModelState.AddModelError("Name", "Name is required");
            attribute.OnActionExecuting(this.actionContext);

            Assert.Equal(HttpStatusCode.BadRequest, this.actionContext.Response.StatusCode);
            var error = Assert.IsType<ObjectContent<HttpError>>(this.actionContext.Response.Content);
            var modelState = JsonConvert.SerializeObject(error.Value);
            Assert.NotNull(modelState);
        }

        [Fact]
        public void OnActionExecuting_Valid_Works()
        {
            var attribute = new ValidateModelAttribute();

            attribute.OnActionExecuting(this.actionContext);

            Assert.Null(this.actionContext.Response);
        }
    }
}