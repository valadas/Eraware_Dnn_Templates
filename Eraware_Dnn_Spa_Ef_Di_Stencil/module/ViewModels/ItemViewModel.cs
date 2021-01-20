// MIT License
// Copyright $ext_companyname$

using $ext_rootnamespace$.Data.Entities;
using $ext_rootnamespace$.DTO;

namespace Acme.Modules.Contacts.ViewModels
{
    /// <summary>
    /// Represents the basic information about an item.
    /// </summary>
    public class ItemViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemViewModel"/> class.
        /// </summary>
        /// <param name="newItem">An <see cref="Item"/> entity.</param>
        public ItemViewModel(Item newItem)
        {
            this.Id = newItem.Id;
            this.Name = newItem.Name;
            this.Description = newItem.Description;
        }

        /// <summary>
        /// Gets or sets the id of the item.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the item description.
        /// </summary>
        public string Description { get; set; }
    }
}