using DotNetNuke.Web.Api;
using Moq;
using $ext_rootnamespace$.Controllers;
using DotNetNuke.Instrumentation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Xunit;

namespace UnitTests.Controllers
{
    public class ModuleExceptionFilterAttributeTests
    {
        [Fact]
        public static void NoException_DoesNothing()
        {
            var attribute = new ModuleExceptionFilterAttribute();
            var loggerSource = new Mock<ILoggerSource>();
            var log = new Mock<ILog>();
            loggerSource.Setup(l => l.GetLogger(It.IsAny<string>())).Returns(log.Object);
            attribute.LoggerSource = loggerSource.Object;
            var actionContext = new HttpActionContext
            {
                Response = new HttpResponseMessage(HttpStatusCode.OK)
            };
            var actionExecutedContext = new HttpActionExecutedContext(actionContext, null);

            attribute.OnException(actionExecutedContext);

            Assert.Equal(HttpStatusCode.OK, actionExecutedContext.Response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(GetExceptionsTestData))]
        public static void ArgumentException_BadRequest(
            Exception exception, HttpStatusCode expectedStatusCode)
        {
            var attribute = new ModuleExceptionFilterAttribute();
            var loggerSource = new Mock<ILoggerSource>();
            var log = new Mock<ILog>();
            loggerSource.Setup(l => l.GetLogger(It.IsAny<string>())).Returns(log.Object);
            attribute.LoggerSource = loggerSource.Object;
            var actionContext = new HttpActionContext
            {
                Response = new HttpResponseMessage(HttpStatusCode.OK),
            };
            var actionExecutedContext = new HttpActionExecutedContext(actionContext, exception);
            actionExecutedContext.ActionContext.ControllerContext = new HttpControllerContext();

            attribute.OnException(actionExecutedContext);

            Assert.Equal(expectedStatusCode, actionExecutedContext.Response.StatusCode);
        }

        [Fact]
        public async Task SystemException_DoesNotExposeDetails()
        {
            var attribute = new ModuleExceptionFilterAttribute();
            var loggerSource = new Mock<ILoggerSource>();
            var log = new Mock<ILog>();
            loggerSource.Setup(l => l.GetLogger(It.IsAny<string>())).Returns(log.Object);
            attribute.LoggerSource = loggerSource.Object;
            var actionContext = new HttpActionContext
            {
                Response = new HttpResponseMessage(HttpStatusCode.OK),
            };
            var actionExecutedContext = new HttpActionExecutedContext(actionContext, new Exception("Danger"));

            attribute.OnException(actionExecutedContext);

            Assert.Equal(HttpStatusCode.InternalServerError, actionExecutedContext.Response.StatusCode);
            var response = await actionExecutedContext.Response.Content.ReadAsStringAsync();
            Assert.NotEqual("Danger", response);
        }

        public static IEnumerable<object[]> GetExceptionsTestData()
        {
            yield return new object[]
            {
                new ArgumentNullException("Null"),
                HttpStatusCode.BadRequest,
            };

            yield return new object[]
            {
                new ArgumentOutOfRangeException("Out of range"),
                HttpStatusCode.BadRequest,
            };

            yield return new object[]
            {
                new ArgumentException("Argument"),
                HttpStatusCode.BadRequest,
            };

            yield return new object[]
            {
                new NotImplementedException("Not Implemented"),
                HttpStatusCode.NotImplemented,
            };

            yield return new object[]
            {
                new TimeoutException("Timed out"),
                HttpStatusCode.RequestTimeout,
            };

            yield return new object[]
            {
                new UnauthorizedAccessException("Unauthorized"),
                HttpStatusCode.Unauthorized,
            };
        }
    }
}
