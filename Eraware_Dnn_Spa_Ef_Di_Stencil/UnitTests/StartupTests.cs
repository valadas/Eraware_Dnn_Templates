using $ext_rootnamespace$.Data;
using $ext_rootnamespace$.Data.Repositories;
using $ext_rootnamespace$.Providers;
using $ext_rootnamespace$.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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
                typeof(ModuleDbContext),
                typeof(IRepository<>),
                typeof(IItemService),
                typeof(ILoggingService),
                typeof(ILocalizationService),
                typeof(IDateTimeProvider),
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