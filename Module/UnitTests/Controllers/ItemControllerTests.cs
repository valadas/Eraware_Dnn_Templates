using $ext_rootnamespace$.Controllers;
using $ext_rootnamespace$.Services.Items;
using DotNetNuke.Entities.Users;
using NSubstitute;
using OneOf;
using OneOf.Types;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Xunit;

namespace UnitTests.Controllers
{
    public class ItemControllerTests
    {
        private CancellationToken token;
        private readonly IItemService itemService;
        private readonly ItemController itemController;

        public ItemControllerTests()
        {
            this.token = new CancellationToken();
            this.itemService = Substitute.For<IItemService>();
            this.itemController = new FakeItemController(this.itemService);
        }

        [Fact]
        public async Task CreateItem_Creates()
        {
            var name = "Name";
            var description = "Description";
            var userId = 123;
            var dto = new CreateItemDTO()
            {
                Name = name,
                Description = description,
            };
            var viewModel = new ItemViewModel() { Id = 1, Name = name, Description = description };
            this.itemService.CreateItemAsync(Arg.Any<CreateItemDTO>(), userId, this.token)
                .Returns(Task.FromResult<OneOf<Success<ItemViewModel>, Error<List<FluentValidation.Results.ValidationFailure>>>>(
                    new Success<ItemViewModel>(new ItemViewModel() { Id = 1, Name = name, Description = description })));

            var result = await this.itemController.CreateItem(dto);

            var response = Assert.IsType<OkNegotiatedContentResult<ItemViewModel>>(result);
            Assert.Equal(1, response.Content.Id);
            Assert.Equal(name, response.Content.Name);
            Assert.Equal(description, response.Content.Description);
        }

        [Fact]
        public async Task GetItemsPage_GetsProperPages()
        {
            var items = new List<ItemViewModel>();
            for (int i = 0; i < 100; i++)
            {
                var item = new ItemViewModel() { Id = i, Name = $"Name {i}", Description = $"Description {1}" };
                items.Add(item);
            }
            var itemsPageViewModel = new ItemsPageViewModel() { Items = items, Page = 1, PageCount = 10, ResultCount = 100 };
            this.itemService.GetItemsPageAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>())
                .Returns(Task.FromResult(itemsPageViewModel));
            var dto = new GetItemsPageDTO
            {
                Query = "Name",
                Page = 1,
                PageSize = 10,
                Descending = true,
            };

            var result = await this.itemController.GetItemsPage(dto);

            var response = Assert.IsType<OkNegotiatedContentResult<ItemsPageViewModel>>(result);
            Assert.Equal(100, response.Content.Items.Count);
            Assert.Equal(1, response.Content.Page);
            Assert.Equal(10, response.Content.PageCount);
            Assert.Equal(100, response.Content.ResultCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task DeleteItem_Deletes(int itemId)
        {
            var result = await this.itemController.DeleteItem(itemId);

            Assert.IsType<OkResult>(result);
            await this.itemService.Received().DeleteItemAsync(itemId);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UserCanEdit_ReturnsBool(bool canEdit)
        {
            this.itemController.CanEdit = canEdit;

            var result = this.itemController.UserCanEdit();

            var response = Assert.IsType<OkNegotiatedContentResult<bool>>(result);
            Assert.Equal(canEdit, response.Content);
        }

        [Fact]
        public async Task UpdateItem_Updates()
        {
            var item = new UpdateItemDTO
            {
                Id = 123,
                Name = "Edited Item",
                Description = "This item was edited",
            };

            await this.itemController.UpdateItem(item);

            await this.itemService.UpdateItemAsync(item, Arg.Any<int>(), this.token);
        }

        public class FakeItemController : ItemController
        {
            private bool canEdit = false;

            public readonly IItemService itemService;
            public FakeItemController(IItemService itemService)
                : base(itemService)
            {
                this.itemService = itemService;
            }
            public override UserInfo UserInfo => new UserInfo() { UserID = 123 };

            public override bool CanEdit
            {
                get { return this.canEdit; }
                set { this.canEdit = value; }
            }
        }

    }
}
