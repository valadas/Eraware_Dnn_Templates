using $ext_rootnamespace$.Services.EchoService;
using Xunit;

namespace UnitTests.Services
{
    public class EchoServiceTests
    {
        private IEchoService echoService;

        public EchoServiceTests()
        {
            this.echoService = new EchoService();
        }

        [Fact]
        public void EchoesBackMessage()
        {
            // Arrange
            var dto = new EchoDto { Message = "Hello, World!" };

            // Act
            var echoedMessage = this.echoService.Echo(dto);

            // Assert
            Assert.Equal(dto.Message, echoedMessage.Message);
        }
    }
}
