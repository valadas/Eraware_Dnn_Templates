using $ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.Data.Repositories;
using $ext_rootnamespace$.Services.Items;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Services.Items
{
    public class ItemServiceTests
    {
        private CancellationToken token;
        private Mock<IRepository<Item>> itemRepository;
        private IItemService itemService;
        private Mock<IValidator<CreateItemDTO>> createItemDtoValidator;
        private Mock<IValidator<UpdateItemDTO>> updateItemDtoValidator;

        public ItemServiceTests()
        {
            this.token = new CancellationToken();
            this.itemRepository = new Mock<IRepository<Item>>();
            this.createItemDtoValidator = new Mock<IValidator<CreateItemDTO>>();
            this.updateItemDtoValidator = new Mock<IValidator<UpdateItemDTO>>();
            this.itemService = new ItemService(
                this.itemRepository.Object,
                this.createItemDtoValidator.Object,
                this.updateItemDtoValidator.Object);
        }

        [Fact]
        public async Task CreateItem_ValidatesDto()
        {
            // Arrange
            var dto = new CreateItemDTO();
            var userId = 123;
            this.createItemDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateItemDTO>(), this.token))
                .Returns(Task.FromResult(new ValidationResult
                {
                    Errors = { new ValidationFailure("Name", "Name is required.") },
                }));

            // Act
            var result = await this.itemService.CreateItemAsync(dto, userId, token);

            // Assert
        }

        [Fact]
        public async Task CreateItem_Creates()
        {
            var item = new CreateItemDTO() { Name = "Name", Description = "Description" };
            this.createItemDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateItemDTO>(), this.token))
                .Returns(Task.FromResult(new ValidationResult()));
            this.itemRepository.Setup(r => r.CreateAsync(It.IsAny<Item>(), It.IsAny<int>()))
                .Callback<Item, int>((i, u) =>
                {
                    i.Id = 1;
                    i.Name = item.Name;
                    i.Description = item.Description;
                });
            var result = await this.itemService.CreateItemAsync(item, 123);

            this.itemRepository.Verify(r =>
                r.CreateAsync(It.Is<Item>(i =>
                i.Name == item.Name &&
                i.Description == item.Description), 123));
            result.Switch(
                success => {
                    Assert.Equal(1, success.Value.Id);
                    Assert.Equal(item.Name, success.Value.Name);
                    Assert.Equal(item.Description, success.Value.Description);
                },
                error => Assert.Fail());
        }

        [Theory]
        [InlineData(false)]
        public async Task GetItemsPage_GetsPages(bool descending)
        {
            var returnedItems = new List<Item>();
            for (int i = 0; i < 30; i++)
            {
                returnedItems.Add(new Item
                {
                    Id = i,
                    Name = $"Name {i}",
                    Description = $"Description {i}",
                });
            }
            var returnedPage = new PagedList<Item>(
                returnedItems, 1, 10, 30, 3);
            this.itemRepository.Setup(i => i.GetPageAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Func<IQueryable<Item>, IOrderedQueryable<Item>>>()))
                .Returns(Task.FromResult(returnedPage));
            
            var finalReturn = await this.itemService.GetItemsPageAsync("test", 1, 10, descending);

            Assert.IsType<ItemsPageViewModel>(finalReturn);
            Assert.Equal(30, finalReturn.ResultCount);
            Assert.Equal(3, finalReturn.PageCount);
            Assert.Equal(1, finalReturn.Page);
        }

        [Fact]
        public async Task DeleteItem_Deletes()
        {
            var itemId = 123;

            await this.itemService.DeleteItemAsync(itemId);

            this.itemRepository.Verify(i => i.DeleteAsync(123), Times.Once);
        }

        [Fact]
        public async Task UpdateItem_ValidatesDto()
        {
            // Arrange
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
            var userId = 123;
            this.itemRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(originalItem));
            this.updateItemDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateItemDTO>(), this.token))
                .Returns(Task.FromResult(new ValidationResult
                {
                    Errors = { new ValidationFailure("Id", "ID must be positive") },
                }));

            // Act
            var result = await this.itemService.UpdateItemAsync(item, userId, this.token);

            // Assert
            result.Switch(
                success => Assert.Fail(),
                error => Assert.Equal("Id", error.Value.First().PropertyName));
        }

        [Fact]
        public async Task UpdateItem_Updates()
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
            this.itemRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(originalItem));
            this.updateItemDtoValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateItemDTO>(), this.token))
                .Returns(Task.FromResult(new ValidationResult()));

            await this.itemService.UpdateItemAsync(item, 2);

            itemRepository.Verify(r => r.UpdateAsync(It.Is<Item>(i =>
                i.Id == item.Id &&
                i.Name == item.Name &&
                i.Description == item.Description), 2), Times.Once);
        }
    }
}
