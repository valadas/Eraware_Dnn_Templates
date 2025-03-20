// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$.Data.Repositories
{
    using System.Collections.Generic;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using $ext_rootnamespace$.Data.Entities;

    /// <summary>
    /// Provides generic data access features for entities.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IRepository<T>
        where T : BaseEntity
    {
        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <param name="token">A token that can be used to abort the request early.</param>
        /// <returns>All the entities.</returns>
        Task<IEnumerable<T>> GetAllAsync(CancellationToken token = default);

        /// <summary>
        /// Gets entitties as an IQueryable to allow furter filtering/sorting, etc.
        /// </summary>
        /// <returns>An IQueryable of the items.</returns>
        IQueryable<T> Get();

        /// <summary>
        /// Gets a single entity by id.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        /// <param name="token">A token that can be used to abort the request early.</param>
        /// <returns>A single entity.</returns>
        Task<T> GetByIdAsync(int id, CancellationToken token = default);

        /// <summary>
        /// Gets a page of entities.
        /// </summary>
        /// <param name="page">The page number to get.</param>
        /// <param name="pageSize">The size of each page.</param>
        /// <param name="filter">An optional filtering expression.</param>
        /// <param name="orderBy">An optional ordering expression.</param>
        /// <param name="orderByDescending">If true, will order the results in descending order.</param>
        /// <param name="token">A token that can be used to abort the request early.</param>
        /// <param name="include">If specified, will include the defined related entities.</param>
        /// <returns><see cref="PagedList{T}"/>.</returns>
        Task<PagedList<T>> GetPageAsync(
            int page,
            int pageSize,
            Expression<Func<T, bool>> filter = null,
            Expression<Func<T, object>> orderBy = null,
            bool orderByDescending = false,
            CancellationToken token = default,
            params Expression<Func<T, object>>[] include);

        /// <summary>
        /// Creates an entity and saves it to the database.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <param name="userId">The creating Dnn user ID. If not provided, will default to -1.</param>
        /// <param name="token">A token that can be used to abort the request early.</param>
        /// <returns>The id of the recently created item.</returns>
        Task<int> CreateAsync(T entity, int userId = -1, CancellationToken token = default);

        /// <summary>
        /// Updates an entity and saves the changes to the database.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="userId">The updating Dnn user ID. If not provided, will default to -1.</param>
        /// <param name="token">A token that can be used to abort the request early.</param>
        /// <returns>An awaitable Task.</returns>
        Task UpdateAsync(T entity, int userId = -1, CancellationToken token = default);

        /// <summary>
        /// Deletes an entity in the database.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        /// <param name="token">A token that can be used to abort the request early.</param>
        /// <returns>An awaitable Task.</returns>
        Task DeleteAsync(int id, CancellationToken token = default);
    }
}