using $ext_rootnamespace$.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using $ext_rootnamespace$;
using Xunit;

namespace UnitTests
{

    public class EntensionMethodTests : FakeDataContext
    {
        [Fact]
        public void IQueryable_GetPage_EmptyDoesNotThrow()
        {
            var itemsList = new List<Item>();
            var items = itemsList.AsQueryable();

            var returnedItems = items.GetPage(1, 10, out int resultCount, out int pageCount);
            Assert.Equal(items.Count(), returnedItems.Count());
        }

        [Theory]
        [InlineData(0, -1, -1, 0, 0, null, false)]
        [InlineData(0, 0, 0, 0, 0, null, false)]
        [InlineData(10, 1, 10, 1, 10, 1, false)]
        [InlineData(20, 2, 10, 2, 10, 11, false)]
        [InlineData(21, 1, 10, 3, 10, 21, true)]
        public void IQueryable_GetPage_PagesProperly(
            int count,
            int page,
            int pageSize,
            int expedtedPages,
            int expectedItems,
            int? firstId,
            bool descending
            )
        {
            GenerateItems(count);

            var items = this.dataContext.Items.AsQueryable();
            if (descending)
            {
                items = items.OrderByDescending(i => i.Id);
            }
            var pagedItems = items.GetPage(page, pageSize, out int resultCount, out int pageCount);

            Assert.Equal(expectedItems, pagedItems.Count());
            Assert.Equal(count, resultCount);
            Assert.Equal(expedtedPages, pageCount);
            if (firstId.HasValue)
            {
                Assert.Equal(firstId.Value, pagedItems.FirstOrDefault().Id);
            }
        }

        [Fact]
        public void IQueryable_IsOrdered_ThrowsWithNull()
        {
            IQueryable<Item> items = null;

            Action sut = () =>
            {
                var isOrdered = items.IsOrdered();
            };

            Assert.Throws<ArgumentNullException>(sut);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public void IQueryable_IsOrdered_Works(bool ordered, bool descending, bool lastOrder)
        {
            GenerateItems(10);
            var items = this.dataContext.Items.AsQueryable();
            if (ordered)
            {
                if (descending)
                {
                    items = items.OrderByDescending(i => i.Name);
                }
                else
                {
                    items = items.OrderBy(i => i.Name);
                }
            }

            if (!lastOrder)
            {
                items = items.Where(i => i.Id > 4);
            }

            Assert.Equal(ordered, items.IsOrdered());
        }

        private void GenerateItems(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                this.dataContext.Items.Add(new Item()
                {
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                    CreatedByUserId = 123,
                    Description = $"Description {1}",
                    Id = i,
                    Name = $"Name {i}",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedByUserId = 234,
                });
            }
            this.dataContext.SaveChanges();
        }
    }
}
