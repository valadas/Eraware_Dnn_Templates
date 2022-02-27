// MIT License
// Copyright $ext_companyname$

using $ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.DTO;
using $ext_rootnamespace$.ViewModels;
using System.Threading.Tasks;

namespace $ext_rootnamespace$.Services
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
        Task<ItemViewModel> CreateItemAsync(CreateItemDTO item, int userId);

        /// <summary>
        /// Deletes an item.
        /// </summary>
        /// <param name="itemId">The id of the item to delete.</param>
        /// <returns>An awaitable task.</returns>
        Task DeleteItemAsync(int itemId);

        /// <summary>
        /// Gets a list of items paged.
        /// </summary>
        /// <param name="query">An optional search query.</param>
        /// <param name="page">The page to get.</param>
        /// <param name="pageSize">How many items are including per page.</param>
        /// <param name="descending">If true, sorts the results in descending order, if false in ascending order.</param>
        /// <returns><see cref="ItemsPageViewModel"/>.</returns>
        Task<ItemsPageViewModel> GetItemsPageAsync(string query, int page = 1, int pageSize = 10, bool descending = false);

        /// <summary>
        /// Updates an existing item.
        /// </summary>
        /// <param name="item">The item to edit with its new details.</param>
        /// <param name="userId">The id of the acting DNN user.</param>
        /// <returns>An awaitable task.</returns>
        Task UpdateItemAsync(UpdateItemDTO item, int userId);
    }
}
