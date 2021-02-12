using $ext_rootnamespace$.Controllers;
using $ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.Data.Repositories;
using $ext_rootnamespace$.DTO;
using $ext_rootnamespace$.Services;
using $ext_rootnamespace$.ViewModels;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using Xunit;

namespace IntegrationTests.Controllers
{
    public class ItemControllerTests : FakeDataContext
    {
        private readonly IRepository<Item> itemRepository;
        private readonly IItemService itemService;
        private readonly ItemController itemController;


        public ItemControllerTests()
        {
            this.itemRepository = new Repository<Item>(this.dataContext);
            this.itemService = new ItemService(this.itemRepository);
            this.itemController = new FakeItemController(this.itemService);
        }

        [Fact]
        public void CreateItem_NoUserInfo_InternalServerError()
        {
            var itemController = new ItemController(null);
            var dto = new CreateItemDTO()
            {
                Name = "name",
                Description = "description",
            };

            var result = itemController.CreateItem(dto);

            var content = Assert.IsType<ExceptionResult>(result);
            Assert.False(string.IsNullOrWhiteSpace(content.Exception.Message));
        }

        [Theory]
        [InlineData(true, null, null)]
        [InlineData(false, null, null)]
        [InlineData(false, null, "Description")]
        public void CreateItem_ArgumentNullThowsBadRequest(bool dtoNull, string name, string description)
        {
            CreateItemDTO dto = dtoNull ? null :
                new CreateItemDTO()
                {
                    Name = name,
                    Description = description,
                };

            var actionResult = this.itemController.CreateItem(dto);

            var response = Assert.IsType<BadRequestErrorMessageResult>(actionResult);
            Assert.False(string.IsNullOrWhiteSpace(response.Message));
            Assert.Equal(0, dataContext.Items.Count());
        }

        [Theory]
        [InlineData("name1", "description1")]
        [InlineData("name2", "description2")]
        public void CreateItem_Creates(string name, string description)
        {
            var dto = new CreateItemDTO()
            {
                Name = name,
                Description = description,
            };

            var result = this.itemController.CreateItem(dto);

            var response = Assert.IsType<OkNegotiatedContentResult<ItemViewModel>>(result);
            Assert.Equal(name, response.Content.Name);
            Assert.Equal(description, response.Content.Description);
            Assert.True(response.Content.Id > 0);
        }

        [Fact]
        public void GetItemsPage_NoService_InternalServerError()
        {
            var itemController = new ItemController(null);

            var result = itemController.GetItemsPage("");

            var response = Assert.IsType<ExceptionResult>(result);
            Assert.False(string.IsNullOrWhiteSpace(response.Exception.Message));
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(9, 1)]
        [InlineData(10, 1)]
        [InlineData(99, 10)]
        [InlineData(101, 11)]
        public void GetItemsPage_GetsProperPages(int amount, int expectedPages)
        {
            this.CreateItems(amount);

            var result = this.itemController.GetItemsPage("", 1, 10, true);

            var response = Assert.IsType<OkNegotiatedContentResult<ItemsPageViewModel>>(result);
            Assert.True(response.Content.Items.Count() <= 10);
            Assert.Equal(1, response.Content.Page);
            Assert.Equal(expectedPages, response.Content.PageCount);
        }

        [Fact]
        public void GetItemsPage_Page0_DefaultsToPage1()
        {
            this.CreateItems(1);

            var result = this.itemController.GetItemsPage("", 0, 10, true);

            var response = Assert.IsType<OkNegotiatedContentResult<ItemsPageViewModel>>(result);
            Assert.Equal(1, response.Content.Page);
        }

        [Fact]
        public void GetItemsPage_PageSize0_DefaultsTo1()
        {
            this.CreateItems(1);

            var result = this.itemController.GetItemsPage("", 1, 0);

            var response = Assert.IsType<OkNegotiatedContentResult<ItemsPageViewModel>>(result);
            Assert.Equal(1, response.Content.Page);
        }

        [Fact]
        public void GetItemsPage_50Items_GetsProperly()
        {
            this.CreateItems(101);

            var results = this.itemController.GetItemsPage("", 1, 50);

            var response = Assert.IsType<OkNegotiatedContentResult<ItemsPageViewModel>>(results);
            Assert.Equal(50, response.Content.Items.Count);
            Assert.Equal(1, response.Content.Page);
            Assert.Equal(3, response.Content.PageCount);
            Assert.Equal(101, response.Content.ResultCount);
        }

        [Fact]
        public void GetItemsPage_SortsAscending()
        {
            this.CreateItems(3);

            var result = this.itemController.GetItemsPage("", 1, 10);

            var response = Assert.IsType<OkNegotiatedContentResult<ItemsPageViewModel>>(result);
            Assert.Equal(1, response.Content.Items[0].Id);
            Assert.Equal(2, response.Content.Items[1].Id);
            Assert.Equal(3, response.Content.Items[2].Id);
        }

        [Fact]
        public void GetItemsPage_SortsDescending()
        {
            this.CreateItems(3);

            var result = this.itemController.GetItemsPage("", 1, 10, true);

            var response = Assert.IsType<OkNegotiatedContentResult<ItemsPageViewModel>>(result);
            Assert.Equal(3, response.Content.Items[0].Id);
            Assert.Equal(2, response.Content.Items[1].Id);
            Assert.Equal(1, response.Content.Items[2].Id);
        }

        [Theory]
        [InlineData("1", 20)]
        [InlineData("Name 1", 12)]
        [InlineData("Name 2", 11)]
        [InlineData("Name 10", 2)]
        [InlineData("Name 100", 1)]
        [InlineData("Missing", 0)]
        [InlineData("", 100)]
        public void GetItemsPage_Query_Filters(string query, int expectedResults)
        {
            this.CreateItems(100);

            var result = this.itemController.GetItemsPage(query, 1, 10);

            var response = Assert.IsType<OkNegotiatedContentResult<ItemsPageViewModel>>(result);
            Assert.True(response.Content.Items.Count <= 10);
            Assert.Equal(expectedResults, response.Content.ResultCount);
        }

        [Fact]
        public void DeleteItem_ThrowsException()
        {
            var itemController = new ItemController(null);
            var result = itemController.DeleteItem(1);

            var response = Assert.IsType<ExceptionResult>(result);
            Assert.False(string.IsNullOrWhiteSpace(response.Exception.Message));
        }

        [Fact]
        public void DeleteItem_DeletesMissingItem()
        {
            this.CreateItems(10);

            var result = this.itemController.DeleteItem(100);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void DeleteItem_Deletes()
        {
            this.CreateItems(10);

            var result = this.itemController.DeleteItem(1);
            var count = this.dataContext.Items.Count();
            var deletedItem = this.dataContext.Items.Find(1);

            Assert.IsType<OkResult>(result);
            Assert.Equal(9, count);
            Assert.Null(deletedItem);
        }

        [Fact]
        public void CanEdit_Defaults_To_False()
        {
            var result = this.itemController.UserCanEdit();

            var response = Assert.IsType<OkNegotiatedContentResult<bool>>(result);
            Assert.False(response.Content);
        }

        private void CreateItems(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var item = new Item()
                {
                    CreatedAt = DateTime.Now.AddMinutes(i),
                    CreatedByUserId = 123,
                    Description = $"Description {i + 1}",
                    Id = i + 1,
                    Name = $"Name {i + 1}",
                    UpdatedAt = DateTime.Now.AddSeconds(i),
                    UpdatedByUserId = 234,
                };
                this.dataContext.Items.Add(item);
            }
            this.dataContext.SaveChanges();
        }

        public class FakeItemController : ItemController
        {
            public readonly IItemService itemService;
            public FakeItemController(IItemService itemService)
                : base(itemService)
            {
                this.itemService = itemService;
            }
            public override UserInfo UserInfo => new UserInfo() { UserID = 123 };
        }
    }
}