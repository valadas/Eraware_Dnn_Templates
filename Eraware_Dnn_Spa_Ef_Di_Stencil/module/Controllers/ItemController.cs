// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$.Controllers
{
    using DotNetNuke.Security;
    using DotNetNuke.Web.Api;
    using $ext_rootnamespace$.Data.Entities;
    using $ext_rootnamespace$.Services;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Http;

    /// <summary>
    /// Provides Web API access for items.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ItemController : ModuleApiController
    {
        private readonly IItemService itemService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemController"/> class.
        /// </summary>
        /// <param name="itemService">The items reposioty.</param>
        public ItemController(IItemService itemService)
        {
        this.itemService = itemService;
        }

        /// <summary>
        /// Creates a new item.
        /// </summary>
        /// <param name="item">The item to create.</param>
        /// <returns>Nothing.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public IHttpActionResult CreateItem(Item item)
        {
            try
            {
                this.itemService.CreateItem(item, this.UserInfo.UserID);
                return this.Ok();
            }
            catch (Exception ex)
            {
                string message = "An unexpected error occured while trying to create the item";
                this.Logger.Error(message, ex);
                return this.InternalServerError(new Exception(message));
                throw;
            }
        }

        /// <summary>
        /// Gets a paged and sorted list of items matching a certain query.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <param name="page">The page number to fetch.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="descending">Sorts descending if true, or ascending if false.</param>
        /// <returns>List of pages + paging information.</returns>
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetItemsPage(string query, int page = 1, int pageSize = 10, bool descending = false)
        {
            try
            {
                var result = this.itemService.GetItemsPage(query, page, pageSize, descending);
                return this.Ok(new { result.items, page, result.resultCount, result.pageCount, this.CanEdit });
            }
            catch (Exception ex)
            {
                string message = "An unexpected error occured while trying to fetch items.";
                this.Logger.Error(message, ex);
                return this.InternalServerError(new Exception(message));
                throw;
            }
        }

        /// <summary>
        /// Deletes an existing item.
        /// </summary>
        /// <param name="item">The item to delete.</param>
        /// <returns>Nothing.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public IHttpActionResult DeleteItem(Item item)
        {
            try
            {
                if (item == null)
                {
                    return this.NotFound();
                }

                this.itemService.DeleteItem(item);
                return this.Ok();
            }
            catch (Exception ex)
            {
                string message = "An unexpected error occured while trying to delete an item.";
                this.Logger.Error(message, ex);
                return this.InternalServerError(new Exception(message));
                throw;
            }
        }
    }
}