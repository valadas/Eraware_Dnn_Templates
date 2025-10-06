using $ext_rootnamespace$.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace UnitTests.Data.Entities
{
    public class ItemTests
    {
        [Fact]
        public void ItemConstructsProperly()
        {
            var item = new Item();
            Assert.NotNull(item);
            Assert.Equal(0, item.Id);
            Assert.True(string.IsNullOrWhiteSpace(item.Name));
            Assert.True(string.IsNullOrWhiteSpace(item.Description));
        }

        [Theory]
        [InlineData(1, "Name1", "Description1")]
        [InlineData(2, "Name2", "Description2")]
        public void ItemPropertiesPersist(int id, string name, string description)
        {
            var item = new Item()
            {
                Id = id,
                Name = name,
                Description = description,
            };
            Assert.Equal(id, item.Id);
            Assert.Equal(name, item.Name);
            Assert.Equal(description, item.Description);
        }
    }
}
