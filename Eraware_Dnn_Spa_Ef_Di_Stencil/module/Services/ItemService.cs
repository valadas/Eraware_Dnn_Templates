// MIT License
// Copyright $ext_companyname$

using $ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.Data.Repositories;
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
        public void CreateItem(Item item, int userId)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            this.itemRepository.Create(item, userId);
        }

        /// <inheritdoc/>
        public (IList<Item> items, int page, int resultCount, int pageCount) GetItemsPage(string query, int page = 1, int pageSize = 10, bool descending = false)
        {
            var items = this.itemRepository.Get();
            if (!string.IsNullOrWhiteSpace(query))
            {
                items = items.Where(i => i.Name.Contains(query) || i.Description.Contains(query));
            }

            items = descending ? items.OrderByDescending(i => i.Name) : items.OrderBy(i => i.Name);
            items = this.itemRepository.GetPage(page, pageSize, items, out int resultCount, out int pageCount);

            return (items.ToList(), page, resultCount, pageCount);
        }

        /// <inheritdoc/>
        public void DeleteItem(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            this.itemRepository.Delete(item.Id);
        }
    }
}