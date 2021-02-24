using DotNetNuke.Instrumentation;
using $ext_rootnamespace$.Services;
using Moq;
using System;
using Xunit;

namespace UnitTests.Services
{
    public class LoggingServiceTests
    {
        [Fact]
        public static void LoggingService_DefaultConstructorExists()
        {
            var service = new LoggingService();
            service.LogError("This is a test");
        }

        [Fact]
        public static void LogginService_TestableConstructorLogsError()
        {
            var logger = new Mock<ILog>();
            ILoggingService service = new LoggingService(logger.Object);

            service.LogError("Test");

            logger.Verify(l => l.Error("Test"), Times.Once);
        }

        [Fact]
        public static void LogginService_TestableConstructorLogsErrorWithException()
        {
            var logger = new Mock<ILog>();
            ILoggingService service = new LoggingService(logger.Object);
            var exception = new ArgumentNullException("test");

            service.LogError("Test", exception);

            logger.Verify(l => l.Error("Test", exception), Times.Once);
        }
    }
}
