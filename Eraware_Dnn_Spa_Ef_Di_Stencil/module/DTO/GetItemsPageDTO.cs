// MIT License
// Copyright $ext_companyname$

namespace Eraware.Modules.MyModule.DTO
{
    /// <summary>
    /// The data object to request a paged list of items.
    /// </summary>
    public class GetItemsPageDTO
    {
        /// <summary>
        /// Gets or sets the optional search query.
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the page number to get.
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Gets or sets the size of pages.
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Gets or sets a value indicating whether the items should be ordered descending.
        /// </summary>
        public bool Descending { get; set; } = false;
    }
}
