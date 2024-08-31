using DotNetNuke.Instrumentation;
using $ext_rootnamespace$.Providers;
using $ext_rootnamespace$.Services.EchoService;
using $ext_rootnamespace$.Services.Localization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace UnitTests
{
    public class StartupTests
    {
        [Fact]
        public void Startup_RegistersAllRequiredServices()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            var startup = new $ext_rootnamespace$.Startup();
            var types = new List<Type>()
            {
                typeof(ILoggerSource),
                typeof(ILocalizationService),
                typeof(IDateTimeProvider),
                typeof(IEchoService),
            };

            // Act
            startup.ConfigureServices(services);

            // Assert
            types.ForEach(type =>
            {
                Assert.Contains(services, s => s.ServiceType == type);
            });

            Assert.Equal(types.Count, services.Count);
        }
    }
}