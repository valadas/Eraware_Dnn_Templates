using Effort.Provider;
using $ext_rootnamespace$.Data;
using $ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.Data.Repositories;
using $ext_rootnamespace$Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Data.Entity;

namespace UnitTests
{
    /// <summary>
    /// Provides a fake data context for testing.
    /// </summary>
    public class FakeDataContext : IDisposable
    {
        public EffortConnection connection;
        public ModuleDbContext dataContext;

        private bool _disposed = false;

        public FakeDataContext()
        {
            this.connection = Effort.DbConnectionFactory.CreateTransient();
            this.dataContext = new ModuleDbContext(this.connection);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                this.dataContext.Dispose();
                this.connection.Dispose();
            }

            this.dataContext = null;
            this.connection = null;

            _disposed = true;
        }
    }

    public class TestDataContext : ModuleDbContext
    {
        public TestDataContext(DbConnection connection)
            : base(connection)
        {
        }

        public DbSet<Category> categories { get; set; }
        public DbSet<Product> products { get; set; }
    }

    public class Category : BaseEntity
    {
        public Category()
        {
            this.Products = new HashSet<Product>();
        }
        [Required]
        public string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }

    public class Product : BaseEntity
    {
        public string Name { get; set; }

        public virtual Category Category { get; set; }
    }

    public class ProductRepository : Repository<Product>
    {
        public ProductRepository(ModuleDbContext context, IDateTimeProvider dateTimeProvider)
            : base(context, dateTimeProvider)
        {
        }
    }
}
