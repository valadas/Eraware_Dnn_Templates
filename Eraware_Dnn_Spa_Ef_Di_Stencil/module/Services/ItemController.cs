// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using DotNetNuke.Security;
    using DotNetNuke.Web.Api;
    using $ext_rootnamespace$.Data.Entities;
    using $ext_rootnamespace$.Data.Repositories;

    /// <summary>
    /// Provides Web API access for items.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "TODO: Implement localization.")]
    public class ItemController : ModuleApiController
    {
        private readonly IRepository<Item> itemRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemController"/> class.
        /// </summary>
        /// <param name="itemRepository">The items reposioty.</param>
        public ItemController(IRepository<Item> itemRepository)
        {
            this.itemRepository = itemRepository;
        }

        /// <summary>
        /// Creates a new item.
        /// </summary>
        /// <param name="item">The item to create.</param>
        /// <returns>Nothing.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage CreateItem(Item item)
        {
            try
            {
                this.itemRepository.Create(item, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                string message = "An unexpected error occured while trying to create the item";
                this.Logger.Error(message, ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message);
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
        public HttpResponseMessage GetItemsPage(string query, int page = 1, int pageSize = 10, bool descending = false)
        {
            try
            {
                var items = this.itemRepository.Get();
                if (!string.IsNullOrWhiteSpace(query))
                {
                    items = items.Where(i => i.Name.Contains(query) || i.Description.Contains(query));
                }

                items = descending ? items.OrderByDescending(i => i.Name) : items.OrderBy(i => i.Name);
                items = this.itemRepository.GetPage(page, pageSize, items, out int resultCount, out int pageCount);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { items, page, resultCount, pageCount, this.CanEdit });
            }
            catch (Exception ex)
            {
                string message = "An unexpected error occured while trying to fetch items.";
                this.Logger.Error(message, ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message);
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
        public HttpResponseMessage DeleteItem(Item item)
        {
            try
            {
                if (item == null)
                {
                    throw new ArgumentNullException(nameof(item));
                }

                this.itemRepository.Delete(item.Id);
                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                string message = "An unexpected error occured while trying to delete an item.";
                this.Logger.Error(message, ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message);
                throw;
            }
        }
    }
}