// MIT License
// Copyright $ext_companyname$

using System.Collections.Generic;

namespace $ext_rootnamespace$.Data.Repositories
{
    /// <summary>
    /// Presents a list of entities in pages.
    /// </summary>
    /// <typeparam name="T">The type of the entities in the paged list.</typeparam>
    public class PagedList<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        /// </summary>
        /// <param name="items">The items on this page.</param>
        /// <param name="page">The current page.</param>
        /// <param name="pageSize">The size of each page.</param>
        /// <param name="resultCount">How many items exist in the entire collection.</param>
        /// <param name="pageCount">The total number of pages available.</param>
        public PagedList(
            IEnumerable<T> items,
            int page,
            int pageSize,
            int resultCount,
            int pageCount)
        {
            this.Items = items;
            this.Page = page;
            this.PageSize = pageSize;
            this.ResultCount = resultCount;
            this.PageCount = pageCount;
        }

        /// <summary>
        /// Gets the items for this page.
        /// </summary>
        public IEnumerable<T> Items { get; }

        /// <summary>
        /// Gets the page number for the current page.
        /// </summary>
        public int Page { get; }

        /// <summary>
        /// Gets the amount of entities per page.
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Gets the total number of entities available in all pages.
        /// </summary>
        public int ResultCount { get; }

        /// <summary>
        /// Gets the total number of available pages.
        /// </summary>
        public int PageCount { get; }
    }
}