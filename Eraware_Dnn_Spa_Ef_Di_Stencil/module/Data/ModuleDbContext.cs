namespace $ext_rootnamespace$.Data
{
    using System.Data.Entity;
    using $ext_rootnamespace$.Data.Entities;

    /// <summary>
    /// The data context for this module.
    /// </summary>
    public class ModuleDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleDbContext"/> class.
        /// </summary>
        public ModuleDbContext()
            : base("name=SiteSqlServer")
        {
        }

        /// <summary>
        /// Gets or sets the module items.
        /// </summary>
        public DbSet<Item> Items { get; set; }
    }
}