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
        [InlineData(true)]
        [InlineData(false)]
        public void GetItemsPage_GetsPages(bool descending)
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

            var finalReturn = this.itemService.GetItemsPage("test", 2, 12, descending);

            Assert.IsType<ItemsPageViewModel>(finalReturn);
            Assert.Equal(2, finalReturn.Page); Assert.Equal(2, finalReturn.Page);
            Assert.Equal(30, finalReturn.ResultCount); Assert.Equal(30, finalReturn.ResultCount);
            Assert.Equal(3, finalReturn.PageCount);
        }

        [Fact]
        public void DeleteItem_Deletes()
        {
            var itemId = 123;

            this.itemService.DeleteItem(itemId);

            this.itemRepository.Verify(i => i.Delete(123), Times.Once);
        }
    }
}
