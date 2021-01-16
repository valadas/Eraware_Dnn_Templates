// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$.Data.Repositories
{
    using $ext_rootnamespace$.Data.Entities;

    /// <summary>
    /// Provides access to Items data.
    /// </summary>
    public class ItemRepository : GenericRepository<Item>, IItemRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The module database context.</param>
        public ItemRepository(ModuleDbContext dbContext)
            : base(dbContext)
        {
        }
    }
}