// MIT License
// Copyright $ext_companyname$
using System;
using System.Linq;
using System.Linq.Expressions;

namespace $ext_rootnamespace$.Common.Extensions
{
    /// <summary>
    /// A collection of extension methods for enumerables.
    /// </summary>
    internal static class IQueryableExtensions
    {
        /// <summary>
        /// Orders the collection with an option to do it ascending or descending.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <typeparam name="TKey">The key to use for ordering.</typeparam>
        /// <param name="source">The source collection to order.</param>
        /// <param name="selector">A function to define the sorting expression.</param>
        /// <param name="descending">If ture, will sort in descending order instead of ascending.</param>
        /// <returns>A new enumerable sorted as specified.</returns>
        public static IOrderedQueryable<T> Order<T, TKey>(this IQueryable<T> source, Expression<Func<T, TKey>> selector, bool descending)
        {
            if (descending)
            {
                return source.OrderByDescending(selector);
            }

            return source.OrderBy(selector);
        }
    }
}