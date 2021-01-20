// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$.Controllers
{
    using DotNetNuke.Security;
    using DotNetNuke.Web.Api;
    using NSwag.Annotations;
    using $ext_rootnamespace$.Data.Entities;
    using $ext_rootnamespace$.DTO;
    using $ext_rootnamespace$.Services;
    using System;
    using System.Net;
    using System.Web.Http;

    /// <summary>
    /// Provides Web API access for items.
    /// </summary>
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
        [SwaggerResponse(HttpStatusCode.OK, typeof(ItemViewModel), Description = "OK")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(string), Description = "Bad Request")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(Exception), Description = "Error")]
        public IHttpActionResult CreateItem(CreateItemDTO item)
        {
            try
            {
                return this.Ok(this.itemService.CreateItem(item, this.UserInfo.UserID));
            }
            catch (ArgumentNullException ex)
            {
                this.Logger.Error(ex.Message, message);
                return this.BadRequest(ex.Message);
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
        [SwaggerResponse(
            HttpStatusCode.OK,
            typeof(ItemsPageViewModel),
            Description = "OK")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(Exception), Description = "Error")]
        public IHttpActionResult GetItemsPage(string query, int page = 1, int pageSize = 10, bool descending = false)
        {
            try
            {
                return this.Ok(this.itemService.GetItemsPage(query, page, pageSize, descending));
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
        /// <param name="itemId">The id of the item to delete.</param>
        /// <returns>Nothing.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [SwaggerResponse(HttpStatusCode.OK, typeof(void), Description = "OK")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(Exception), Description = "Error")]
        public IHttpActionResult DeleteItem(int itemId)
        {
            try
            {
                this.itemService.DeleteItem(itemId);
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

        /// <summary>
        /// Checks if a user can edit the current items.
        /// </summary>
        /// <returns>A boolean indicating whether the user can edit the current items.</returns>
        [HttpGet]
        [AllowAnonymous]
        [SwaggerResponse(HttpStatusCode.OK, typeof(bool), Description = "OK")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(Exception), Description = "Error")]
        public IHttpActionResult UserCanEdit()
        {
            try
            {
                return this.Ok(this.CanEdit);
            }
            catch (Exception ex)
            {
                var message = "There was an error checking if the current user can edit this module.";
                this.Logger.Error(message, ex);
                return this.InternalServerError(new Exception(message));
                throw;
            }
        }
    }
}