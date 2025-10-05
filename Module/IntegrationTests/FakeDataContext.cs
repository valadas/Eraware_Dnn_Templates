using $ext_rootnamespace$.Data;
using Effort.Provider;
using System;

namespace IntegrationTests
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
}