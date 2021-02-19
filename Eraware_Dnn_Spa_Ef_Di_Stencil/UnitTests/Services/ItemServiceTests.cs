using $ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.Data.Repositories;
using $ext_rootnamespace$.DTO;
using $ext_rootnamespace$.Services;
using $ext_rootnamespace$.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        public void CreateItem_NoNameThrows()
        {
            Action createItem = () => this.itemService.CreateItem(new CreateItemDTO(), 123);

            Assert.Throws<ArgumentNullException>(createItem);
        }

        [Fact]
        public void CreateItem_Creates()
        {
            var item = new CreateItemDTO() { Name = "Name", Description = "Description" };
            itemRepository.Setup(r => r.Create(It.IsAny<Item>(), It.IsAny<int>()))
                .Callback<Item, int>((i, u) => {
                    i.Id = 1;
                    i.Name = item.Name;
                    i.Description = item.Description;
                });

            var createdItem = this.itemService.CreateItem(item, 123);

            this.itemRepository.Verify(r =>
                r.Create(It.Is<Item>(i =>
                i.Name == item.Name &&
                i.Description == item.Description), 123));
            Assert.Equal(1, createdItem.Id);
            Assert.Equal(item.Name, createdItem.Name);
            Assert.Equal(item.Description, createdItem.Description);
        }

        [Theory]
        [InlineData(0, 0, true, 30, 1)]
        [InlineData(1, 10, false, 3, 1)]
        [InlineData(1, 20, false, 2, 1)]
        [InlineData(4, 10, false, 3, 3)]
        public void GetItemsPage_GetsPages(
            int page,
            int pageSize,
            bool descending,
            int expectedPages
            int returnedPage)
        {
            this.itemRepository.Setup(r => r.Get())
                .Returns(() =>
                {
                    List<Item> items = new List<Item>();
                    for (int i = 0; i < 30; i++)
                    {
                        items.Add(new Item() { Id = i, Name = $"test {i}", Description = $"Description {i}" });
                    }

                    return items.AsQueryable();
                });

            var finalReturn = this.itemService.GetItemsPage("test", page, pageSize, descending);

            Assert.IsType<ItemsPageViewModel>(finalReturn);
            Assert.Equal(30, finalReturn.ResultCount);
            Assert.Equal(expectedPages, finalReturn.PageCount);
            Assert.Equal(retunedPage, finalReturn.Page);
        }

        [Fact]
        public void DeleteItem_Deletes()
        {
            var itemId = 123;

            this.itemService.DeleteItem(itemId);

            this.itemRepository.Verify(i => i.Delete(123), Times.Once);
        }

        [Fact]
        public void UpdateItem_Updates()
        {
            var originalItem = new Item
            {
                Id = 1,
                Name = "Original Name",
                Description = "Original Description",
            };

            var item = new UpdateItemDTO
            {
                Id = 1,
                Name = "New Item Name",
                Description = "New Item Description",
            };
            this.itemRepository.Setup(r => r.GetById(It.IsAny<int>()))
                .Returns(originalItem);

            this.itemService.UpdateItem(item, 2);

            itemRepository.Verify(r => r.Update(It.Is<Item>(i =>
                i.Id == item.Id &&
                i.Name == item.Name &&
                i.Description == item.Description), 2), Times.Once);
        }

        [Fact]
        public void UpdateItem_NullDto_Throws()
        {
            Action nullDto = () => this.itemService.UpdateItem(null, 2);

            var ex = Assert.Throws<ArgumentNullException>(nullDto);
            Assert.Equal("dto", ex.ParamName);
            itemRepository.Verify(r =>
                r.Update(
                    It.IsAny<Item>(),
                    It.IsAny<int>()
                ), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  ")]
        public void UpdateItem_NoName_Throws(string name)
        {
            var item = new UpdateItemDTO
            {
                Id = 123,
                Name = name,
                Description = null,
            };

            Action noName = () => this.itemService.UpdateItem(item, 2);

            var ex = Assert.Throws<ArgumentNullException>(noName);
            Assert.Equal("Name", ex.ParamName);
        }
    }
}
