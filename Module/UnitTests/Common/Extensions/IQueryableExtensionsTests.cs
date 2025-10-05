using $ext_rootnamespace$.Common.Extensions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.Common.Extensions
{
    public class IQueryableExtensionsTests
    {
        [Theory]
        [InlineData(false, 1, 2, 3)]
        [InlineData(true, 3, 2, 1)]
        public void Order_OrdersBothWays(bool descending, params int[] expected)
        {
            var list = new List<Item>
            {
                new Item {Id = 1},
                new Item {Id = 3},
                new Item {Id = 2},
            }.AsQueryable();

            var orderedList = list
                .Order(i => i.Id, descending)
                .Select(i => i.Id).ToList();

            Assert.Equal(expected, orderedList);
        }

        public class Item
        {
            public int Id { get; set; }
        }
    }
}