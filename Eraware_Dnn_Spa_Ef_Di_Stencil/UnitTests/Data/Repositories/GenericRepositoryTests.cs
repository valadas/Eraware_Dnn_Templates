using $ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.Data.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Data.Repositories
{
    public class GenericRepositoryTests : FakeDataContext
    {
        [Fact]
        public void GenericRepositoryConstructs()
        {
            var repository = new Repository<Item>(dataContext);
            
            Assert.NotNull(dataContext);
            Assert.NotNull(repository);
        }

        [Fact]
        public async Task GenericRepositoryCreatesAndGetsById()
        {
            var repository = new Repository<Item>(dataContext);
            var expectedItem = new Item() { Id = 1, Name = "Name", Description = "Description" };
            await repository.CreateAsync(expectedItem);

            var returnnedItem = await repository.GetByIdAsync(1);

            Assert.Equal(JsonConvert.SerializeObject(expectedItem), JsonConvert.SerializeObject(returnnedItem));
        }

        [Fact]
        public async Task GenericRepositoryDeletes()
        {
            var repository = new Repository<Item>(dataContext);
            var item = new Item() { Id = 1, Name = "Name", Description = "Description" };
            await repository.CreateAsync(item);

            await repository.DeleteAsync(item.Id);

            Assert.Empty(repository.GetAll());
        }

        [Fact]
        public void GenericRepositoryGet()
        {
            this.dataContext.Items.Add(new Item() { Id = 1, Name = "Name", Description = "Description" });
            this.dataContext.SaveChanges();
            var repository = new Repository<Item>(this.dataContext);

            var items = repository.Get();

            Assert.Equal(1, items.Count());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GenericRepositoryGetsAll(int iterations)
        {
            var repository = new Repository<Item>(dataContext);
            for (int i = 1; i <= iterations; i++)
            {
                var item = new Item() { Id = i, Name = $"Name {i}", Description = $"Description {i}" };
                await repository.CreateAsync(item);
            }

            var items = await repository.GetAllAsync();
            var count = items.Count();

            Assert.Equal(iterations, count);
        }

        [Fact]
        public async Task GenericRepositoryUpdates()
        {
            var repository = new Repository<Item>(dataContext);
            await repository.CreateAsync(new Item() { Id = 1, Name = "Original Name", Description = "Original Description" });
            var entity = await repository.GetByIdAsync(1);
            entity.Name = "New Name";
            entity.Description = "New Description";

            await repository.UpdateAsync(entity);

            Assert.Equal("New Name", entity.Name);
            Assert.Equal("New Description", entity.Description);
        }

        [Fact]
        public async Task GenericRepositoryCreate_ThrowsWithNullEntity()
        {
            var repository = new Repository<Item>(dataContext);

            Task create() => repository.CreateAsync(null);

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(create);
            Assert.Equal("entity", ex.ParamName);
        }

        [Fact]
        public async Task GenericRepositoryUpdate_ThrowsWithNullEntity()
        {
            var repository = new Repository<Item>(dataContext);

            Task update() => repository.UpdateAsync(null);

            var ex = Assert.Throws<ArgumentNullException>(update);
            Assert.Equal("entity", ex.ParamName);
        }

        [Fact]
        public async Task Repository_Create_DefaultAudit()
        {
            var repository = new Repository<Item>(dataContext);
            var item = new Item() { Name = "Name", Description = "Description" };

            await repository.CreateAsync(item);

            Assert.Equal(-1, item.CreatedByUserId);
            Assert.Equal(-1, item.UpdatedByUserId);
            Assert.True(item.CreatedAt == item.UpdatedAt);
            var createdTimeSpan = DateTime.UtcNow - item.CreatedAt;
            var updatedTimeSpan = DateTime.UtcNow - item.UpdatedAt;
            Assert.True(createdTimeSpan < TimeSpan.FromMinutes(1));
            Assert.True(updatedTimeSpan < TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task Repository_Create_UsesUserId()
        {
            var repository = new Repository<Item>(dataContext);
            var item = new Item() { Name = "Name", Description = "Description" };

            await repository.CreateAsync(item, 123);

            Assert.Equal(123, item.CreatedByUserId);
            Assert.Equal(123, item.UpdatedByUserId);
        }

        [Fact]
        public async Task Repository_Update_DefaultAudit()
        {
            var repository = new Repository<Item>(dataContext);
            var item = new Item() { Name = "Name", Description = "Description" };
            await repository.CreateAsync(item);
            item.Name = "New Name";
            item.Description = "New Description";

            await repository.UpdateAsync(item);

            Assert.True(item.CreatedAt < item.UpdatedAt);
            Assert.Equal(-1, item.CreatedByUserId);
            Assert.Equal(-1, item.UpdatedByUserId);
        }

        [Fact]
        public async Task Repository_Update_UsesuserId()
        {
            var repository = new Repository<Item>(dataContext);
            var item = new Item() { Name = "Name", Description = "Description" };
            await repository.CreateAsync(item);
            item.Name = "New Name";
            item.Description = "New Description";

            await repository.UpdateAsync(item, 123);

            Assert.True(item.CreatedAt < item.UpdatedAt);
            Assert.Equal(-1, item.CreatedByUserId);
            Assert.Equal(123, item.UpdatedByUserId);
        }

        /// <summary>
        /// Ensures that deleting an item id that does not exists simply does nothing.
        /// </summary>
        [Fact]
        public async Task Repository_DeleteMissing_DoesNotThrow()
        {
            var repository = new Repository<Item>(this.dataContext);

            await repository.Delete(1);
        }

        [Theory]
        [InlineData(1, 10, 10, 5)]
        [InlineData(3, 20, 10, 3)]
        [InlineData(0, 0, 1, 50)]
        public async Task GetPage_Pages(
            int page,
            int pageSize,
            int expectedItems,
            int expectedPages)
        {
            this.CreateItems(100);
            var repository = new Repository<Item>(this.dataContext);

            var result = await repository.GetPageAsync(
                page,
                pageSize,
                items => items
                .Where(item => item.Name.ToUpper().Contains("test".ToUpper()))
                .OrderBy(i => i.Name));

            Assert.Equal(expectedItems, result.Items.Count());
            Assert.Equal(page == 0 ? 1 : page, result.Page);
            Assert.Equal(pageSize == 0 ? 1 : pageSize, result.PageSize);
            Assert.Equal(50, result.ResultCount);
            Assert.Equal(expectedPages, result.PageCount);
        }

        [Fact]
        public async Task Pages_With_Related_Entities()
        {
            var testConnection = Effort.DbConnectionFactory.CreateTransient();
            using (var context = new TestDataContext(testConnection))
            {
                for (int i = 0; i < 10; i++)
                {
                    var newCategory = new Category { Name = $"Category {i}" };
                    context.categories.Add(newCategory);
                    await context.SaveChangesAsync();
                    for (int j = 0; j < 10; j++)
                    {
                        var newProduct = new Product
                        {
                            Name = $"Product {j}",
                            Category = newCategory,
                        };
                        context.products.Add(newProduct);
                    }
                    await context.SaveChangesAsync();
                }
                var repository = new Repository<Product>(context);

                var result = await repository.GetPageAsync(
                    1,
                    10,
                    products => products.OrderBy(product => product.Name),
                    product => product.Category);
            }
        }

        private void CreateItems(int count)
        {
            for (int i = 0; i < count; i++)
            {
                this.dataContext.Items.Add(new Item()
                {
                    Name = i % 2 == 0 ? $"Test Name {i}" : $"Name {i}",
                    Description = $"Test description {i}",
                });
            }
            this.dataContext.SaveChanges();
        }
    }
}
