// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$
{
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Instrumentation;
    using $ext_rootnamespace$.Data;
    using $ext_rootnamespace$.Data.Entities;
    using $ext_rootnamespace$.Data.Repositories;
    using $ext_rootnamespace$.Providers;
    using $ext_rootnamespace$.Services.Items;
    using $ext_rootnamespace$.Services.Localization;
    using FluentValidation;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System.Diagnostics.CodeAnalysis;

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
            services.AddScoped<ModuleDbContext, ModuleDbContext>();
            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<IItemService, ItemService>();
            services.TryAddScoped(x => LoggerSource.Instance);
            services.AddScoped<ILocalizationService, LocalizationService>();
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<IValidator<CreateItemDTO>, CreateItemDtoValidator>();
            services.AddScoped<IValidator<UpdateItemDTO>, UpdateItemDtoValidator>();
        }
    }
}