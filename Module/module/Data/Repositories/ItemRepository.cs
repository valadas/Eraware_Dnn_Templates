// MIT License
// Copyright $ext_companyname$

using $ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.Providers;

namespace $ext_rootnamespace$.Data.Repositories
{
    /// <inheritdoc cref="IItemRepository"/>
    internal class ItemRepository : Repository<Item>, IItemRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemRepository"/> class.
        /// </summary>
        /// <param name="context">The underlying data-context to use.</param>
        /// <param name="dateTimeProvider">Provides information about dates and times.</param>
        public ItemRepository(ModuleDbContext context, IDateTimeProvider dateTimeProvider)
            : base(context, dateTimeProvider)
        {
        }
    }
}