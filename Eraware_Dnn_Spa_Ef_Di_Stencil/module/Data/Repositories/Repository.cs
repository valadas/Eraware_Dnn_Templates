// MIT License
// Copyright $ext_companyname$

using $ext_rootnamespace$.Data.Repositories;
using $ext_rootnamespace$.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace $ext_rootnamespace$.Data.Repositories
{
    /// <summary>
    /// Provides common generic data access methods for entities.
    /// </summary>
    /// <typeparam name="T">The type of the entities.</typeparam>
    public class Repository<T> : IRepository<T>
        where T : BaseEntity
    {
        private readonly ModuleDbContext context;
        private DbSet<T> entities;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
        /// </summary>
        /// <param name="context">The module database context.</param>
        public Repository(ModuleDbContext context)
        {
            this.context = context;
            this.entities = context.Set<T>();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await this.entities.ToListAsync();
        }

        /// <inheritdoc/>
        public IQueryable<T> Get()
        {
            return this.entities;
        }

        /// <inheritdoc/>
        public async Task<T> GetByIdAsync(int id)
        {
            return await this.entities.SingleOrDefaultAsync(e => e.Id == id);
        }

        /// <inheritdoc/>
        public async Task<PagedList<T>> GetPageAsync(
            int page,
            int pageSize,
            Func<IQueryable<T>, IOrderedQueryable<T>> predicate,
            params Expression<Func<T, object>>[] include)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 1;
            }

            var items = this.entities.AsQueryable();

            if (include.Any())
            {
                items = include.Aggregate(items, (current, inc) => current.Include(inc));
            }

            items = items.OrderBy(i => i.Id);
            items = predicate(items);

            var resultCount = await items.CountAsync();
            var pageCount = (resultCount + pageSize - 1) / pageSize;
            int skip = pageSize * (page - 1);
            var pageItems = await items
                .Skip(skip)
                .Take(pageSize)
                .AsQueryable()
                .ToListAsync();

            return new PagedList<T>(
                pageItems,
                page,
                pageSize,
                resultCount,
                pageCount);
        }

        /// <inheritdoc/>
        public async Task<int> CreateAsync(T entity, int userId = -1)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            entity.CreatedByUserId = userId;
            entity.UpdatedByUserId = userId;
            this.entities.Add(entity);
            await this.context.SaveChangesAsync();
            return entity.Id;
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(T entity, int userId = -1)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedByUserId = userId;
            await this.context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id)
        {
            T entity = this.entities.SingleOrDefault(e => e.Id == id);
            if (entity is null)
            {
                return;
            }

            this.entities.Remove(entity);
            await this.context.SaveChangesAsync();
        }
    }
}