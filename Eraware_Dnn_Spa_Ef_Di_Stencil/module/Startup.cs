// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$
{
    using DotNetNuke.DependencyInjection;
    using $ext_rootnamespace$.Data;
    using $ext_rootnamespace$.Data.Repositories;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Implements the IDnnStartup interface to run at application start.
    /// </summary>
    public class Startup : IDnnStartup
    {
        /// <summary>
        /// Registers the dependencies for injection.
        /// </summary>
        /// <param name="services">The services collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ModuleDbContext, ModuleDbContext>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        }
    }
}