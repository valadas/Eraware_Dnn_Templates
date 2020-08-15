using $ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.Data.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace UnitTests.Data.Repositories
{
    public class GenericRepositoryTests : FakeDataContext
    {
        [Fact]
        public void GenericRepositoryConstructs()
        {
            var repository = new GenericRepository<Item>(dataContext);
            
            Assert.NotNull(dataContext);
            Assert.NotNull(repository);
        }

        [Fact]
        public void GenericRepositoryCreatesAndGetsById()
        {
            var repository = new GenericRepository<Item>(dataContext);
            var expectedItem = new Item() { Id = 1, Name = "Name", Description = "Description" };
            repository.Create(expectedItem);

            var returnnedItem = repository.GetById(1);

            Assert.Equal(JsonConvert.SerializeObject(expectedItem), JsonConvert.SerializeObject(returnnedItem));
        }

        [Fact]
        public void GenericRepositoryDeletes()
        {
            var repository = new GenericRepository<Item>(dataContext);
            var item = new Item() { Id = 1, Name = "Name", Description = "Description" };
            repository.Create(item);

            repository.Delete(item.Id);

            Assert.Empty(repository.GetAll());
        }

        [Fact]
        public void GenericRepositoryDeleteInexistingItemThrows()
        {
            var repository = new GenericRepository<Item>(dataContext);
            var item = new Item() { Id = 1, Name = "Name", Description = "Description" };
            repository.Create(item);

            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                repository.Delete(2);
            });
            
            Assert.Equal("Sequence contains no elements", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void GenericRepositoryGetsAll(int iterations)
        {
            var repository = new GenericRepository<Item>(dataContext);
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
            var repository = new GenericRepository<Item>(dataContext);
            for (int i = 1; i <= items; i++)
            {
                var item = new Item() { Id = i, Name = $"Name {i}", Description = $"Description {i}" };
                repository.Create(item);
            }

            var results = repository.GetPage(page, pageSize, repository.GetAll(), out int resultCount, out int pageCount).ToList();

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
            var repository = new GenericRepository<Item>(dataContext);
            repository.Create(new Item() { Id = 1, Name = "Original Name", Description = "Original Description" });
            var entity = repository.GetById(1);
            entity.Name = "New Name";
            entity.Description = "New Description";

            repository.Update(entity);

            Assert.Equal("New Name", entity.Name);
            Assert.Equal("New Description", entity.Description);
        }
    }
}
