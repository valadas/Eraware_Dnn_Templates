using $ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.Data.Repositories;
using Newtonsoft.Json;
using System;
using System.Linq;
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
        public void GenericRepositoryCreatesAndGetsById()
        {
            var repository = new Repository<Item>(dataContext);
            var expectedItem = new Item() { Id = 1, Name = "Name", Description = "Description" };
            repository.Create(expectedItem);

            var returnnedItem = repository.GetById(1);

            Assert.Equal(JsonConvert.SerializeObject(expectedItem), JsonConvert.SerializeObject(returnnedItem));
        }

        [Fact]
        public void GenericRepositoryDeletes()
        {
            var repository = new Repository<Item>(dataContext);
            var item = new Item() { Id = 1, Name = "Name", Description = "Description" };
            repository.Create(item);

            repository.Delete(item.Id);

            Assert.Empty(repository.GetAll());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void GenericRepositoryGetsAll(int iterations)
        {
            var repository = new Repository<Item>(dataContext);
            for (int i = 1; i <= iterations; i++)
            {
                var item = new Item() { Id = i, Name = $"Name {i}", Description = $"Description {i}" };
                repository.Create(item);
            }

            var count = repository.GetAll().Count();

            Assert.Equal(iterations, count);
        }
        
        [Theory]
        [InlineData(100, 1, 10, 10)]
        [InlineData(101, 2, 20, 6)]
        [InlineData(5, 0, 10, 1)]
        public void GenericRepositoryPages(int items, int page, int pageSize, int pages)
        {
            var repository = new Repository<Item>(dataContext);
            for (int i = 1; i <= items; i++)
            {
                var item = new Item() { Id = i, Name = $"Name {i}", Description = $"Description {i}" };
                repository.Create(item);
            }

            var results = repository.GetPage(page, pageSize, repository.Get(), out int resultCount, out int pageCount).ToList();

            Assert.True(results.Count() <= pageSize);
            Assert.Equal(items, resultCount);
            Assert.Equal(pages, pageCount);
            var lastId = 0;
            foreach (var result in results)
            {
                // Checks the order was not changed.
                Assert.True(result.Id > lastId);
                lastId = result.Id;
            }
        }

        [Fact]
        public void GenericRepositoryUpdates()
        {
            var repository = new Repository<Item>(dataContext);
            repository.Create(new Item() { Id = 1, Name = "Original Name", Description = "Original Description" });
            var entity = repository.GetById(1);
            entity.Name = "New Name";
            entity.Description = "New Description";

            repository.Update(entity);

            Assert.Equal("New Name", entity.Name);
            Assert.Equal("New Description", entity.Description);
        }

        [Fact]
        public void GenericRepositoryCreate_ThrowsWithNullEntity()
        {
            var repository = new Repository<Item>(dataContext);

            Action create = () => repository.Create(null);

            var ex = Assert.Throws<ArgumentNullException>(create);
            Assert.Equal("entity", ex.ParamName);
        }

        [Fact]
        public void GenericRepositoryUpdate_ThrowsWithNullEntity()
        {
            var repository = new Repository<Item>(dataContext);

            Action update = () => repository.Update(null);

            var ex = Assert.Throws<ArgumentNullException>(update);
            Assert.Equal("entity", ex.ParamName);
        }

        [Fact]
        public void Repository_Create_DefaultAudit()
        {
            var repository = new Repository<Item>(dataContext);
            var item = new Item() { Name = "Name", Description = "Description" };

            repository.Create(item);

            Assert.Equal(-1, item.CreatedByUserId);
            Assert.Equal(-1, item.UpdatedByUserId);
            Assert.True(item.CreatedAt == item.UpdatedAt);
            var createdTimeSpan = DateTime.UtcNow - item.CreatedAt;
            var updatedTimeSpan = DateTime.UtcNow - item.UpdatedAt;
            Assert.True(createdTimeSpan < TimeSpan.FromMinutes(1));
            Assert.True(updatedTimeSpan < TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void Repository_Create_UsesUserId()
        {
            var repository = new Repository<Item>(dataContext);
            var item = new Item() { Name = "Name", Description = "Description" };

            repository.Create(item, 123);

            Assert.Equal(123, item.CreatedByUserId);
            Assert.Equal(123, item.UpdatedByUserId);
        }

        [Fact]
        public void Repository_Update_DefaultAudit()
        {
            var repository = new Repository<Item>(dataContext);
            var item = new Item() { Name = "Name", Description = "Description" };
            repository.Create(item);
            item.Name = "New Name";
            item.Description = "New Description";

            repository.Update(item);

            Assert.True(item.CreatedAt < item.UpdatedAt);
            Assert.Equal(-1, item.CreatedByUserId);
            Assert.Equal(-1, item.UpdatedByUserId);
        }

        [Fact]
        public void Repository_Update_UsesuserId()
        {
            var repository = new Repository<Item>(dataContext);
            var item = new Item() { Name = "Name", Description = "Description" };
            repository.Create(item);
            item.Name = "New Name";
            item.Description = "New Description";

            repository.Update(item, 123);

            Assert.True(item.CreatedAt < item.UpdatedAt);
            Assert.Equal(-1, item.CreatedByUserId);
            Assert.Equal(123, item.UpdatedByUserId);
        }
    }
}
