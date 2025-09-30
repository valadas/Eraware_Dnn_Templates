// MIT License
// Copyright $ext_companyname$

using DotNetNuke.DependencyInjection;
using DotNetNuke.Instrumentation;
using $ext_rootnamespace$.Providers;
using $ext_rootnamespace$.Services.EchoService;
using $ext_rootnamespace$.Services.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace $ext_rootnamespace$
{
    /// <summary>
    /// Implements the IDnnStartup interface to run at application start.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup : IDnnStartup
    {
        /// <summary>
        /// Registers the dependencies for injection.
        /// </summary>
        /// <param name="services">The services collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddScoped(x => LoggerSource.Instance);
            services.AddScoped<ILocalizationService, LocalizationService>();
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<IEchoService, EchoService>();
        }
    }
}