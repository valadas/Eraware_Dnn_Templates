// MIT License
// Copyright $ext_companyname$

using $ext_rootnamespace$.Data.Entities;
using System.Collections.Generic;

namespace $ext_rootnamespace$.Modules.Contacts.Services
{
    /// <summary>
    /// Provides services to manage items.
    /// </summary>
    public interface IItemService
    {
        /// <summary>
        /// Creates a new item.
        /// </summary>
        /// <param name="item">The item to create.</param>
        /// <param name="userId">The acting user id.</param>
        /// <returns><see cref="Item"/>.</returns>
        Item CreateItem(Item item, int userId);

        /// <summary>
        /// Deletes an item.
        /// </summary>
        /// <param name="item">The item to delete.</param>
        void DeleteItem(Item item);

        /// <summary>
        /// Gets a list of items paged.
        /// </summary>
        /// <param name="query">An optional search query.</param>
        /// <param name="page">The page to get.</param>
        /// <param name="pageSize">How many items are including per page.</param>
        /// <param name="descending">If true, sorts the results in descending order, if false in ascending order.</param>
        /// <returns>A Tuple containing a list of items, the current page, the total results count and the total page count.</returns>
        (IList<Item> items, int page, int resultCount, int pageCount) GetItemsPage(string query, int page = 1, int pageSize = 10, bool descending = false);
    }
}