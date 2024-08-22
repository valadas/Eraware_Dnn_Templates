using $ext_rootnamespace$.Controllers;
using $ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.Data.Repositories;
using $ext_rootnamespace$.Providers;
using $ext_rootnamespace$.Services.Items;
using $ext_rootnamespace$.Services.Localization;
using DotNetNuke.Entities.Users;
using FluentValidation;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Xunit;

namespace IntegrationTests.Controllers
{
    public class ItemControllerTests : FakeDataContext
    {
        private readonly Mock<IDateTimeProvider> dateTimeProvider;
        private readonly IRepository<Item> itemRepository;
        private readonly IItemService itemService;
        private readonly IValidator<CreateItemDTO> createItemDtoValidator;
        private readonly IValidator<UpdateItemDTO> updateItemDtoValidator;
        private readonly Mock<ILocalizationService> localizationService;
        private readonly ItemController itemController;


        public ItemControllerTests()
        {
            this.dateTimeProvider = new Mock<IDateTimeProvider>();
            this.dateTimeProvider.Setup(p => p.GetUtcNow()).Returns(new DateTime(2022, 1, 1));
            this.itemRepository = new Repository<Item>(this.dataContext, this.dateTimeProvider.Object);
            this.localizationService = new Mock<ILocalizationService>();
            var resx = new LocalizationViewModel
            {
                ModelValidation = new LocalizationViewModel.ModelValidationInfo
                {
                    NameRequired = "Name is required.",
                    IdGreaterThanZero = "Id must be greater than zero.",
                },
                UI = new LocalizationViewModel.UIInfo
                {
                    AddItem = "Add Item",
                    Cancel = "Cancel",
                    Create = "Create",
                    Delete = "Delete",
                    DeleteItemConfirm = "Are you sure you want to delete this item?",
                    Description = "Description",
                    Edit = "Edit",
                    LoadMore = "Load more",
                    Name = "Name",
                    No = "No",
                    Save = "Save",
                    Yes = "Yes",
                    SearchPlaceholder = "Search...",
                    ShownItems = "Shown items",
                },
            };
            this.localizationService.SetupGet(s => s.ViewModel).Returns(resx);
            this.createItemDtoValidator = new CreateItemDtoValidator(localizationService.Object);
            this.updateItemDtoValidator = new UpdateItemDtoValidator(localizationService.Object);
            this.itemService = new ItemService(this.itemRepository, this.createItemDtoValidator, this.updateItemDtoValidator);
            this.itemController = new FakeItemController(this.itemService);
        }

        [Theory]
        [InlineData("name1", "description1")]
        [InlineData("name2", "description2")]
        public async Task CreateItem_Creates(string name, string description)
        {
            var dto = new CreateItemDTO()
            {
                Name = name,
                Description = description,
            };

            var result = await this.itemController.CreateItem(dto);

            var response = Assert.IsType<OkNegotiatedContentResult<ItemViewModel>>(result);
            Assert.Equal(name, response.Content.Name);
            Assert.Equal(description, response.Content.Description);
            Assert.True(response.Content.Id > 0);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(9, 1)]
        [InlineData(10, 1)]
        [InlineData(99, 10)]
        [InlineData(101, 11)]
        public async Task GetItemsPage_GetsProperPages(int amount, int expectedPages)
        {
            this.CreateItems(amount);
            var dto = new GetItemsPageDTO { Query = "", Page = 1, PageSize = 10, Descending = true };

            var result = await this.itemController.GetItemsPage(dto);

            var response = Assert.IsType<OkNegotiatedContentResult<ItemsPageViewModel>>(result);
            Assert.True(response.Content.Items.Count() <= 10);
            Assert.Equal(1, response.Content.Page);
            Assert.Equal(expectedPages, response.Content.PageCount);
        }

        [Fact]
        public async Task GetItemsPage_Page0_DefaultsToPage1()
        {
            this.CreateItems(1);

            var result = await this.itemController.GetItemsPage(
                new GetItemsPageDTO { Query = "", Page = 0, PageSize = 10, Descending = true });

            var response = Assert.IsType<OkNegotiatedContentResult<ItemsPageViewModel>>(result);
            Assert.Equal(1, response.Content.Page);
        }

        [Fact]
        public async Task GetItemsPage_PageSize0_DefaultsTo1()
        {
            this.CreateItems(1);

            var result = await this.itemController.GetItemsPage(
                new GetItemsPageDTO { Query = "", Page = 1, PageSize = 0 });

            var response = Assert.IsType<OkNegotiatedContentResult<ItemsPageViewModel>>(result);
            Assert.Equal(1, response.Content.Page);
        }

        [Fact]
        public async Task GetItemsPage_50Items_GetsProperly()
        {
            this.CreateItems(101);

            var results = await this.itemController.GetItemsPage(
                new GetItemsPageDTO { Query = "", Page = 1, PageSize = 50 });

            var response = Assert.IsType<OkNegotiatedContentResult<ItemsPageViewModel>>(results);
            Assert.Equal(50, response.Content.Items.Count);
            Assert.Equal(1, response.Content.Page);
            Assert.Equal(3, response.Content.PageCount);
            Assert.Equal(101, response.Content.ResultCount);
        }

        [Fact]
        public async Task GetItemsPage_SortsAscending()
        {
            this.CreateItems(3);

            var result = await this.itemController.GetItemsPage(
                new GetItemsPageDTO { Query = "", Page = 1, PageSize = 10 });

            var response = Assert.IsType<OkNegotiatedContentResult<ItemsPageViewModel>>(result);
            Assert.Equal(1, response.Content.Items[0].Id);
            Assert.Equal(2, response.Content.Items[1].Id);
            Assert.Equal(3, response.Content.Items[2].Id);
        }

        [Fact]
        public async Task GetItemsPage_SortsDescending()
        {
            this.CreateItems(3);

            var result = await this.itemController.GetItemsPage(
                new GetItemsPageDTO { Query = "", Page = 1, PageSize = 10, Descending = true });

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
        public async Task GetItemsPage_Query_Filters(string query, int expectedResults)
        {
            this.CreateItems(100);

            var result = await this.itemController.GetItemsPage(
                new GetItemsPageDTO { Query = query, Page = 1, PageSize = 10 });

            var response = Assert.IsType<OkNegotiatedContentResult<ItemsPageViewModel>>(result);
            Assert.True(response.Content.Items.Count <= 10);
            Assert.Equal(expectedResults, response.Content.ResultCount);
        }

        [Fact]
        public async Task DeleteItem_DeletesMissingItem()
        {
            this.CreateItems(10);

            var result = await this.itemController.DeleteItem(100);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteItem_Deletes()
        {
            this.CreateItems(10);

            var result = await this.itemController.DeleteItem(1);
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

        [Fact]
        public async Task EditItem_Saves()
        {
            this.CreateItems(1);
            var name = "New Name";
            var description = "New Description";
            var dto = new UpdateItemDTO()
            {
                Id = 1,
                Name = "New Name",
                Description = "New Description",
            };

            var result = await this.itemController.UpdateItem(dto);

            Assert.IsType<OkResult>(result);
            var newItem = this.dataContext.Items.FirstOrDefault();
            Assert.NotNull(newItem);
            Assert.Equal(1, newItem.Id);
            Assert.Equal(name, newItem.Name);
            Assert.Equal(description, newItem.Description);
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