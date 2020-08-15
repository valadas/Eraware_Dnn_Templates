using $ext_rootnamespace$.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTests
{
    /// <summary>
    /// Provides a fake data context for testing.
    /// </summary>
    public class FakeDataContext : IDisposable
    {
        public ModuleDbContext dataContext;

        public FakeDataContext()
        {
            var connection = Effort.DbConnectionFactory.CreateTransient();
            this.dataContext = new ModuleDbContext(connection);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disponsing)
        {
            this.dataContext.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
