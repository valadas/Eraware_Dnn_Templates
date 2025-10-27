// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$.Data
{
    using $ext_rootnamespace$.Data.Entities;
    using System;
    using System.Data.Common;
    using System.Data.Entity;
    using System.IO;
    using System.Configuration;
    using System.Web;

/// <summary>
/// The data context for this module.
/// </summary>
public class ModuleDbContext : DbContext
    {
        /// <summary>
        /// Cached connection string, computed lazily and thread-safe.
        /// </summary>
        private static readonly Lazy<string> CachedConnectionString = new Lazy<string>(ComputeConnectionString);

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleDbContext"/> class.
        /// </summary>
        public ModuleDbContext()
            : base(GetConnectionString())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleDbContext"/> class.
        /// </summary>
        /// <param name="connection">An existing <see cref="DbConnection"/>.</param>
        public ModuleDbContext(DbConnection connection)
            : base(connection, true)
        {
        }

        /// <summary>
        /// Gets or sets the module items.
        /// </summary>
        public DbSet<Item> Items { get; set; }

        /// <summary>
        /// Gets the connection string, using cached value after first call.
        /// </summary>
        /// <returns>The connection string.</returns>
        private static string GetConnectionString()
        {
            return CachedConnectionString.Value;
        }

        /// <summary>
        /// Computes the connection string by checking runtime context and configuration files.
        /// This method is called only once per application lifetime.
        /// </summary>
        /// <returns>The connection string.</returns>
        private static string ComputeConnectionString()
        {
            // For runtime
            if (HttpContext.Current != null)
            {
                return "name=SiteSqlServer";
            }

            // For CLI tools, start from the current base directory and move up until we find web.config
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string configFilePath = Path.Combine(baseDirectory, "web.config");
            while (!File.Exists(configFilePath) && baseDirectory != null)
            {
                baseDirectory = Directory.GetParent(baseDirectory)?.FullName;
                if (baseDirectory == null)
                {
                    throw new FileNotFoundException("web.config not found in any parent directory.");
                }

                configFilePath = Path.Combine(baseDirectory, "web.config");
            }

            // Load the configuration manually from the web.config file
            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = configFilePath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            var connectionString = config.ConnectionStrings.ConnectionStrings["SiteSqlServer"];
            return connectionString == null
                ? throw new NullReferenceException("Connection string 'SiteSqlServer' not found.")
                : connectionString.ConnectionString;
        }
    }
}