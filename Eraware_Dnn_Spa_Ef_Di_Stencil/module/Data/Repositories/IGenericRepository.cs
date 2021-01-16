// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$.Data.Repositories
{
    using System.Linq;
    using System.Threading.Tasks;
    using $ext_rootnamespace$.Data.Entities;

    /// <summary>
    /// Provides generic data access features for entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IGenericRepository<TEntity>
        where TEntity : class, IEntity
    {
        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>IQueryable of all the entities.</returns>
        IQueryable<TEntity> GetAll();

        /// <summary>
        /// Gets a single entity by id.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        /// <returns>A single entity.</returns>
        TEntity GetById(int id);

        /// <summary>
        /// Creates an entity and saves it to the database.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        void Create(TEntity entity);

        /// <summary>
        /// Updates an entity and saves the changes to the database.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        void Update(TEntity entity);

        /// <summary>
        /// Deletes an entity in the database.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        void Delete(int id);

        /// <summary>
        /// Gets a page of entities with some data.
        /// </summary>
        /// <param name="page">The page number to fetch.</param>
        /// <param name="pageSize">The number of entities per page.</param>
        /// <param name="entities">The entities to page (for best performance, do your filters and sorting first).</param>
        /// <param name="resultCount">Returns the total number of items.</param>
        /// <param name="pageCount">Returns the total number of pages.</param>
        /// <returns>An IQueryable of the entities for that page.</returns>
        IQueryable<TEntity> GetPage(int page, int pageSize, IQueryable<TEntity> entities, out int resultCount, out int pageCount);
    }
}