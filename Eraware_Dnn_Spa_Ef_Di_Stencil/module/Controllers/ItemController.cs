// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$.Controllers
{
    using DotNetNuke.Security;
    using DotNetNuke.Web.Api;
    using NSwag.Annotations;
    using $ext_rootnamespace$.DTO;
    using $ext_rootnamespace$.Services;
    using $ext_rootnamespace$.ViewModels;
    using System;
    using System.Net;
    using System.Threading.Tasks;
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
        public async Task<IHttpActionResult> CreateItem(CreateItemDTO item)
        {
            var result = await this.itemService.CreateItemAsync(item, this.UserInfo.UserID);
            return this.Ok(result);
        }

        /// <summary>
        /// Gets a paged and sorted list of items matching a certain query.
        /// </summary>
        /// <param name="dto">The details of the query, <see cref="GetItemsPageDTO"/>.</param>
        /// <returns>List of pages + paging information.</returns>
        [HttpGet]
        [AllowAnonymous]
        [SwaggerResponse(
            HttpStatusCode.OK,
            typeof(ItemsPageViewModel),
            Description = "OK")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(Exception), Description = "Error")]
        public async Task<IHttpActionResult> GetItemsPage([FromUri] GetItemsPageDTO dto)
        {
            var page = await this.itemService.GetItemsPageAsync(dto.Query, dto.Page, dto.PageSize, dto.Descending);
            return this.Ok(page);
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
        public async Task<IHttpActionResult> DeleteItem(int itemId)
        {
            await this.itemService.DeleteItemAsync(itemId);
            return this.Ok();
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
            return this.Ok(this.CanEdit);
        }

        /// <summary>
        /// Updates an existing item.
        /// </summary>
        /// <param name="item">The new information about the item, <see cref="UpdateItemDTO"/>.</param>
        /// <returns>Only a status code and no data.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [SwaggerResponse(HttpStatusCode.OK, null, Description = "OK")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(ArgumentException), Description = "Malformed request")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(Exception), Description = "Error")]
        public async Task<IHttpActionResult> UpdateItem(UpdateItemDTO item)
        {
            await this.itemService.UpdateItem(item, this.UserInfo.UserID);
            return this.Ok();
        }
    }
}
