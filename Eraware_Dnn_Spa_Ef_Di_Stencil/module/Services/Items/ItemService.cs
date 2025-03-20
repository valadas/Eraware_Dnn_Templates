// MIT License
// Copyright $ext_companyname$

using $ext_rootnamespace$.Common.Extensions;
using $ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.Data.Repositories;
using FluentValidation;
using FluentValidation.Results;
using OneOf;
using OneOf.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace $ext_rootnamespace$.Services.Items
{
    /// <summary>
    /// Provides services to manage items.
    /// </summary>
    public class ItemService : IItemService
    {
        private readonly IItemRepository itemRepository;
        private readonly IValidator<CreateItemDTO> createItemDtoValidator;
        private readonly IValidator<UpdateItemDTO> updateItemDtoValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemService"/> class.
        /// </summary>
        /// <param name="itemRepository">The items repository.</param>
        /// <param name="createItemDtoValidator">The validator for the <see cref="CreateItemDTO"/> entity.</param>
        /// <param name="updateItemDtoValidator">The validator for the <see cref="UpdateItemDTO"/> entity.</param>
        public ItemService(
            IItemRepository itemRepository,
            IValidator<CreateItemDTO> createItemDtoValidator,
            IValidator<UpdateItemDTO> updateItemDtoValidator)
        {
            this.itemRepository = itemRepository;
            this.createItemDtoValidator = createItemDtoValidator;
            this.updateItemDtoValidator = updateItemDtoValidator;
        }

        /// <inheritdoc/>
        public async Task<OneOf<Success<ItemViewModel>, Error<List<ValidationFailure>>>> CreateItemAsync(CreateItemDTO item, int userId, CancellationToken token = default)
        {
            var validationResult = await this.createItemDtoValidator.ValidateAsync(item);

            if (!validationResult.IsValid)
            {
                return new Error<List<ValidationFailure>>(validationResult.Errors);
            }

            var newItem = new Item() { Name = item.Name, Description = item.Description };
            await this.itemRepository.CreateAsync(newItem, userId);

            var vm = new ItemViewModel(newItem);
            return new Success<ItemViewModel>(vm);
        }

        /// <inheritdoc/>
        public async Task<ItemsPageViewModel> GetItemsPageAsync(string query, int page = 1, int pageSize = 10, bool descending = false)
            {
            var items = await this.itemRepository.GetPageAsync(
                page,
                pageSize,
                filter: item => string.IsNullOrEmpty(query) || item.Name.ToUpper().Contains(query.ToUpper()),
                orderBy: item => item.Name,
                orderByDescending: descending);

        var itemsPageViewModel = new ItemsPageViewModel()
            {
                Items = items.Items.Select(item => new ItemViewModel
                {
                    Description = item.Description,
                    Id = item.Id,
                    Name = item.Name,
                }).ToList(),
                Page = items.Page,
                ResultCount = items.ResultCount,
                PageCount = items.PageCount,
            };

            return itemsPageViewModel;
        }

        /// <inheritdoc/>
        public async Task DeleteItemAsync(int itemId)
        {
            await this.itemRepository.DeleteAsync(itemId);
        }

        /// <inheritdoc/>
        public async Task<OneOf<Success, Error<List<ValidationFailure>>>> UpdateItemAsync(UpdateItemDTO dto, int userId, CancellationToken token = default)
        {
            var validationResult = await this.updateItemDtoValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                return new Error<List<ValidationFailure>>(validationResult.Errors);
            }

            var item = await this.itemRepository.GetByIdAsync(dto.Id);
            item.Name = dto.Name;
            item.Description = dto.Description;

            await this.itemRepository.UpdateAsync(item, userId);

            return default(Success);
        }
    }
}
