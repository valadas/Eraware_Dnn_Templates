using $ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.Data.Repositories;
using $ext_rootnamespace$.Services.Items;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
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
        private IRepository<Item> itemRepository;
        private IItemService itemService;
        private IValidator<CreateItemDTO> createItemDtoValidator;
        private IValidator<UpdateItemDTO> updateItemDtoValidator;

        public ItemServiceTests()
        {
            this.token = new CancellationToken();
            this.itemRepository = Substitute.For<IRepository<Item>>();
            this.createItemDtoValidator = Substitute.For<IValidator<CreateItemDTO>>();
            this.updateItemDtoValidator = Substitute.For<IValidator<UpdateItemDTO>>();
            this.itemService = new ItemService(
                this.itemRepository,
                this.createItemDtoValidator,
                this.updateItemDtoValidator);
        }

        [Fact]
        public async Task CreateItem_ValidatesDto()
        {
            // Arrange
            var dto = new CreateItemDTO();
            var userId = 123;
            this.createItemDtoValidator.ValidateAsync(Arg.Any<CreateItemDTO>(), this.token)
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
            this.createItemDtoValidator.ValidateAsync(Arg.Any<CreateItemDTO>(), this.token)
                .Returns(Task.FromResult(new ValidationResult()));
            this.itemRepository.CreateAsync(Arg.Any<Item>(), Arg.Any<int>())
                .Returns(callInfo =>
                {
                    var i = callInfo.ArgAt<Item>(0);
                    i.Id = 1;
                    i.Name = item.Name;
                    i.Description = item.Description;
                    return 1;
                });

            var result = await this.itemService.CreateItemAsync(item, 123);

            await this.itemRepository.Received(1).CreateAsync(
               Arg.Is<Item>(i =>
               i.Name == item.Name &&
               i.Description == item.Description), 123);
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
            this.itemRepository.GetPageAsync(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<Func<IQueryable<Item>, IOrderedQueryable<Item>>>())
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

            await this.itemRepository.Received().DeleteAsync(123);
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
            this.itemRepository.GetByIdAsync(Arg.Any<int>())
                .Returns(Task.FromResult(originalItem));
            this.updateItemDtoValidator.ValidateAsync(Arg.Any<UpdateItemDTO>(), this.token)
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
            this.itemRepository.GetByIdAsync(Arg.Any<int>())
                .Returns(Task.FromResult(originalItem));
            this.updateItemDtoValidator.ValidateAsync(Arg.Any<UpdateItemDTO>(), this.token)
                .Returns(Task.FromResult(new ValidationResult()));

            await this.itemService.UpdateItemAsync(item, 2);

            await itemRepository.Received().UpdateAsync(Arg.Is<Item>(i =>
                i.Id == item.Id &&
                i.Name == item.Name &&
                i.Description == item.Description), 2);
        }
    }
}
