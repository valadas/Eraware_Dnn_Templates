// MIT License
// Copyright $ext_companyname$

using $ext_rootnamespace$.Data.Entities;
using FluentValidation.Results;
using OneOf;
using OneOf.Types;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace $ext_rootnamespace$.Services.Items
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
        /// <param name="token">A token that can be used to abort the request early.</param>
        /// <returns>Either a success of <see cref="Item"/> of an error of string.</returns>
        Task<OneOf<Success<ItemViewModel>, Error<List<ValidationFailure>>>> CreateItemAsync(
            CreateItemDTO item,
            int userId,
            CancellationToken token = default);

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
        /// <param name="token">A token that can be used to abort the request early.</param>
        /// <returns>An awaitable task.</returns>
        Task<OneOf<Success, Error<List<ValidationFailure>>>> UpdateItemAsync(UpdateItemDTO item, int userId, CancellationToken token = default);
    }
}
