using $ext_rootnamespace$.Controllers;
using $ext_rootnamespace$.Services.EchoService;
using $ext_rootnamespace$.Services.Localization;
using NSubstitute;
using OneOf.Types;
using System.Web.Http.Results;
using Xunit;

namespace UnitTests.Controllers
{
    public class EchoControllerTests
    {
        private readonly IEchoService echoService;
        private readonly EchoController echoController;

        public EchoControllerTests()
        {
            this.echoService = Substitute.For<IEchoService>();
            this.echoController = new EchoController(this.echoService);
        }

        [Fact]
        public void CallsEchoService()
        {

            // Arrange
            var message = "Hello, World!";
            var expectedResponse = new EchoViewModel { Message = message };
            var dto = new EchoDto { Message = message };
            this.echoService.Echo(dto).Returns(expectedResponse);

            // Act
            var result = this.echoController.Echo(dto);

            // Assert
            var response = Assert.IsType<OkNegotiatedContentResult<EchoViewModel>>(result);
            Assert.Equal(expectedResponse.Message, response.Content.Message);
        }
    }
}
