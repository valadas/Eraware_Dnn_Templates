// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$.Data.Repositories
{
    using System.Collections.Generic;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
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
        /// <returns>All the entities.</returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Gets entitties as an IQueryable to allow furter filtering/sorting, etc.
        /// </summary>
        /// <returns>An IQueryable of the items.</returns>
        IQueryable<T> Get();

        /// <summary>
        /// Gets a single entity by id.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        /// <returns>A single entity.</returns>
        T GetById(int id);

        /// <summary>
        /// Creates an entity and saves it to the database.
        /// </summary>
        /// <param name="entity">The entity to save.</param>
        /// <param name="userId">The creating Dnn user ID. If not provided, will default to -1.</param>
        void Create(T entity, int userId = -1);

        /// <summary>
        /// Updates an entity and saves the changes to the database.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="userId">The updating Dnn user ID. If not provided, will default to -1.</param>
        void Update(T entity, int userId = -1);

        /// <summary>
        /// Deletes an entity in the database.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        void Delete(int id);
    }
}