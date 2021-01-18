using $ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.Data.Repositories;
using $ext_rootnamespace$.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.Services
{
    public class ItemServiceTests
    {
        private Mock<IRepository<Item>> itemRepository;
        private IItemService itemService;

        public ItemServiceTests()
        {
            this.itemRepository = new Mock<IRepository<Item>>();
            this.itemService = new ItemService(this.itemRepository.Object);
        }

        [Fact]
        public void CreateItem_NoItemThrows()
        {
            Action createItem = () => this.itemService.CreateItem(null, 123);

            Assert.Throws<ArgumentNullException>(createItem);
        }

        [Fact]
        public void CreateItem_Creates()
        {
            var item = new Item() { Name = "Name", Description = "Description" };

            this.itemService.CreateItem(item, 123);

            this.itemRepository.Verify(i => i.Create(item, 123));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetItemsPage_GetsPages(bool descending)
        {
            int resultCount = 30;
            int pageCount = 5;
            var result = new List<Item>().AsQueryable<Item>();
            this.itemRepository.Setup(r =>
            r.GetPage(2, 12, It.IsAny<IQueryable<Item>>(), out resultCount, out pageCount))
                .Returns(result);

            var finalReturn = this.itemService.GetItemsPage("test", 2, 12, descending);

            this.itemRepository.Verify(i => i.Get(), Times.Once);
            this.itemRepository.Verify(i => i.GetPage(2, 12, result, out resultCount, out pageCount), Times.Once);
            Assert.IsType<List<Item>>(finalReturn.items);
            Assert.Equal(2, finalReturn.page);
            Assert.Equal(30, finalReturn.resultCount);
            Assert.Equal(5, finalReturn.pageCount);
        }

        [Fact]
        public void DeleteItem_ThrowsIfNoItem()
        {
            Action deleteItem = () => this.itemService.DeleteItem(null);

            Assert.Throws<ArgumentNullException>(deleteItem);
        }

        [Fact]
        public void DeleteItem_Deletes()
        {
            var item = new Item() { Id = 123, Name = "Name", Description = "Description" };

            this.itemService.DeleteItem(item);

            this.itemRepository.Verify(i => i.Delete(123), Times.Once);
        }
    }
}