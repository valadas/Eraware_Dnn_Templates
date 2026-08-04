// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$.Data.Entities
{
    using $ext_rootnamespace$.Common;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents an item entity.
    /// </summary>
    [Table(Globals.ModulePrefix + "Items")]
    public class Item : BaseEntity
    {
        /// <summary>
        /// Gets or sets the item name.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the item description.
        /// </summary>
        [StringLength(250)]
        public string Description { get; set; }
    }
}