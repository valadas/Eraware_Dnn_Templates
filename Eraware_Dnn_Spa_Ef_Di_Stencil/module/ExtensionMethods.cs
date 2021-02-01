// MIT License
// Copyright $ext_companyname$

using System;
using System.Linq;
using System.Linq.Expressions;

namespace $ext_rootnamespace$
{
    /// <summary>
    /// Provides various entension methods.
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Gets a page of results from an IQueryable collection.
        /// </summary>
        /// <typeparam name="T">The type of the entities in the IQueryable.</typeparam>
        /// <param name="items">The original items to get a page from.</param>
        /// <param name="page">The page number to get.</param>
        /// <param name="pageSize">How many items are on each page.</param>
        /// <param name="resultCount">Returns the total amount of items available.</param>
        /// <param name="pageCount">Returns the total amount of available pages.</param>
        /// <returns>The items that belong on the give page.</returns>
        internal static IQueryable<T> GetPage<T>(
            this IQueryable<T> items,
            int page,
            int pageSize,
            out int resultCount,
            out int pageCount)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 1;
            }

            resultCount = items.Count();
            pageCount = (resultCount + pageSize - 1) / pageSize;
            int skip = pageSize * (page - 1);

            if (!items.IsOrdered())
            {
                items = items.OrderBy(i => true);
            }

            return items.Skip(skip).Take(10);
        }

        /// <summary>
        /// Checks if the query contains any sorting expressions
        /// such as OrderBy, OrderByDescending, ThenBy or ThenByDescending.
        /// </summary>
        /// <typeparam name="T">The type of entities in the query.</typeparam>
        /// <param name="queryable">The original queryable.</param>
        /// <returns>A value indicating whether the query is already ordered.</returns>
        internal static bool IsOrdered<T>(this IQueryable<T> queryable)
        {
            if (queryable == null)
            {
                throw new ArgumentNullException("queryable");
            }

            return OrderingMethodFinder.OrderMethodExists(queryable.Expression);
        }

        private class OrderingMethodFinder : ExpressionVisitor
        {
            private bool orderingMethodFound = false;

            internal static bool OrderMethodExists(Expression expression)
            {
                var visitor = new OrderingMethodFinder();
                visitor.Visit(expression);
                return visitor.orderingMethodFound;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var name = node.Method.Name;

                if (node.Method.DeclaringType == typeof(Queryable) && (
                    name.StartsWith("OrderBy", StringComparison.Ordinal) ||
                    name.StartsWith("ThenBy", StringComparison.Ordinal)))
                {
                    this.orderingMethodFound = true;
                }

                return base.VisitMethodCall(node);
            }
        }
    }
}