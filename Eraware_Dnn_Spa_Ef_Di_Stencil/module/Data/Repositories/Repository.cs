// MIT License
// Copyright $ext_companyname$

using 
using System.Threading;$
ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.Providers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace $ext_rootnamespace$.Data.Repositories
{
    /// <summary>
    /// Provides common generic data access methods for entities.
    /// </summary>
    /// <typeparam name="T">The type of the entities.</typeparam>
    public abstract class Repository<T> : IRepository<T>
        where T : BaseEntity
    {
        private readonly ModuleDbContext context;
        private readonly DbSet<T> entities;
        private readonly IDateTimeProvider dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <param name="context">The module database context.</param>
    /// <param name="dateTimeProvider">Provides date and time information.</param>
    public Repository(
        ModuleDbContext context,
        IDateTimeProvider dateTimeProvider)
        {
            this.context = context;
            this.entities = context.Set<T>();
            this.dateTimeProvider = dateTimeProvider;
        }

    /// <inheritdoc/>
        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken token = default)
        {
            return await this.entities.ToListAsync(token);
        }

        /// <inheritdoc/>
        public IQueryable<T> Get()
        {
            return this.entities;
        }

        /// <inheritdoc/>
        public async Task<T> GetByIdAsync(int id, CancellationToken token = default)
        {
            return await this.entities.FindAsync(token, id);
        }

        /// <inheritdoc/>
        public virtual async Task<PagedList<T>> GetPageAsync(
            int page,
            int pageSize,
            Expression<Func<T, bool>> filter = null,
            Expression<Func<T, object>> orderBy = null,
            bool orderByDescending = false,
            CancellationToken token = default,
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

            IQueryable<T> query = this.entities;

            // Includes
            if (include?.Any() == true)
            {
                query = include.Aggregate(query, (current, inc) => current.Include(inc));
            }

            // Filter
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Sorting
            if (orderBy == null)
            {
                orderBy = i => i.Id;
            }

            query = orderByDescending
                ? query.OrderByDescending(orderBy)
                : query.OrderBy(orderBy);

            // Paging
            var resultCount = await query.CountAsync();
            int skip = pageSize * (page - 1);
            var items = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
            var pageCount = (resultCount + pageSize - 1) / pageSize;

            return new PagedList<T>(
                items,
                page,
                pageSize,
                resultCount,
                pageCount);
        }

        /// <inheritdoc/>
        public virtual async Task<int> CreateAsync(T entity, int userId = -1, CancellationToken token = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            entity.CreatedAt = this.dateTimeProvider.GetUtcNow();
            entity.CreatedByUserId = userId;
            entity.UpdatedAt = this.dateTimeProvider.GetUtcNow();
            entity.UpdatedByUserId = userId;
            this.entities.Add(entity);
            await this.context.SaveChangesAsync(token);
            return entity.Id;
        }

        /// <inheritdoc/>
        public virtual async Task UpdateAsync(T entity, int userId = -1, CancellationToken token = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            entity.UpdatedAt = this.dateTimeProvider.GetUtcNow();
            entity.UpdatedByUserId = userId;

            this.entities.Attach(entity);
            this.entities.Entry(entity).State = EntityState.Modified;
            await this.context.SaveChangesAsync(token);
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAsync(int id, CancellationToken token = default)
        {
            T entity = await this.entities.FindAsync(token, id);
            if (entity is null)
            {
                return;
            }

            this.entities.Remove(entity);
            await this.context.SaveChangesAsync(token);
        }
    }
}