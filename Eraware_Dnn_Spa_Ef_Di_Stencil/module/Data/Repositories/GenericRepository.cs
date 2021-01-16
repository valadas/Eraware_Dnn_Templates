// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$.Data.Repositories
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using $ext_rootnamespace$.Data.Entities;

    /// <summary>
    /// Provides common generic data access methods for entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities.</typeparam>
    public class GenericRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity
    {
        private readonly ModuleDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="context">The module database context.</param>
        public GenericRepository(ModuleDbContext context)
        {
            this.dbContext = context;
        }

        /// <inheritdoc/>
        public void Create(TEntity entity)
        {
            this.dbContext.Set<TEntity>().Add(entity);
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public void Delete(int id)
        {
            var entity = this.dbContext.Set<TEntity>().First(e => e.Id == id);
            this.dbContext.Set<TEntity>().Remove(entity);
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public IQueryable<TEntity> GetAll()
        {
            return this.dbContext.Set<TEntity>().AsNoTracking();
        }

        /// <inheritdoc/>
        public TEntity GetById(int id)
        {
            return this.dbContext.Set<TEntity>()
                .AsNoTracking()
                .FirstOrDefault(e => e.Id == id);
        }

        /// <inheritdoc/>
        public IQueryable<TEntity> GetPage(int page, int pageSize, IQueryable<TEntity> entities, out int resultCount, out int pageCount)
        {
            if (page < 1)
            {
                page = 1;
            }

            resultCount = entities.Count();
            pageCount = (resultCount + pageSize - 1) / pageSize;
            int skip = pageSize * (page - 1);
            return entities.OrderBy(i => 0).Skip(skip).Take(pageSize);
        }

        /// <inheritdoc/>
        public void Update(TEntity entity)
        {
            var localEntity = this.dbContext.Set<TEntity>()
                .Local
                .FirstOrDefault(l => l.Id == entity.Id);
            if (localEntity != null)
            {
                this.dbContext.Entry(localEntity).State = EntityState.Detached;
            }

            this.dbContext.Entry(entity).State = EntityState.Modified;
            this.dbContext.SaveChanges();
        }
    }
}