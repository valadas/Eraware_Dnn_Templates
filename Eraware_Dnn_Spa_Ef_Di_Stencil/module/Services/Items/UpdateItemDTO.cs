// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$.Services.Items
{
    /// <summary>
    /// Data transfer object used to update an item.
    /// </summary>
    public class UpdateItemDTO : CreateItemDTO
    {
        /// <summary>
        /// Gets or sets the id of the item to edit.
        /// </summary>
        public int Id { get; set; }
    }
}