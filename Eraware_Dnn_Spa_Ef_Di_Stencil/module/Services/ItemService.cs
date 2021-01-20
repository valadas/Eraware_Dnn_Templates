// MIT License
// Copyright $ext_companyname$

using $ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.Data.Repositories;
using $ext_rootnamespace$.DTO;
using $ext_rootnamespace$.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace $ext_rootnamespace$.Modules.Contacts.Services
{
    /// <summary>
    /// Provides services to manage items.
    /// </summary>
    public class ItemService : IItemService
    {
        private IRepository<Item> itemRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemService"/> class.
        /// </summary>
        /// <param name="itemRepository">The items repository.</param>
        public ItemService(IRepository<Item> itemRepository)
        {
            this.itemRepository = itemRepository;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"> is thrown if the item or one of its required properties are missing.</exception>
        public ItemViewModel CreateItem(CreateItemDTO item, int userId)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (string.IsNullOrWhiteSpace(item.Name))
            {
                throw new ArgumentNullException("The item name is required.", nameof(item.Name))
            }

            this.itemRepository.Create(item, userId);

            var newItem = new Item() { Name = item.Name, Description = item.Description };
            this.itemRepository.Create(newItem, userId);

            return new ItemViewModel(newItem);
        }

        /// <inheritdoc/>
        public ItemsPageViewModel GetItemsPage(string query, int page = 1, int pageSize = 10, bool descending = false)
        {
            var items = this.itemRepository.Get();
            if (!string.IsNullOrWhiteSpace(query))
            {
                items = items.Where(i => i.Name.Contains(query) || i.Description.Contains(query));
            }

            items = descending ? items.OrderByDescending(i => i.Name) : items.OrderBy(i => i.Name);
            items = this.itemRepository.GetPage(page, pageSize, items, out int resultCount, out int pageCount);

            return new ItemsPageViewModel()
            {
                Items = items.Select(items => new ItemViewModel(i)).ToList(),
                Page = page,
                ResultCount = resultCount,
                PageCount = pageCount,
            };
        }

        /// <inheritdoc/>
        public void DeleteItem(int itemId)
        {
            this.itemRepository.Delete(itemId);
        }
    }
}