using $ext_rootnamespace$.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace UnitTests.Data.Repositories
{
    public class ItemRepositoryTests : FakeDataContext
    {
        [Fact]
        public void ItemRepositoryConstructs()
        {
            var repository = new ItemRepository(this.dataContext);

            Assert.True(repository != null);
        }
    }
}
