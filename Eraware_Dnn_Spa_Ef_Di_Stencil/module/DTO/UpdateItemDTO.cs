// MIT License
// Copyright $ext_companyname$

using System.ComponentModel.DataAnnotations;

namespace $ext_rootnamespace$.DTO
{
    /// <summary>
    /// Data transfer object used to update an item.
    /// </summary>
    public class UpdateItemDTO : CreateItemDTO
    {
        /// <summary>
        /// Gets or sets the id of the item to edit.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "IdGreaterThanZero")]
        public int Id { get; set; }
    }
}