// MIT License
// Copyright $ext_companyname$

using System;

namespace $ext_rootnamespace$.Data.Entities
{
    /// <summary>
    /// Base entity to provide common properties to other entities and allow it's usage in generic repositories.
    /// </summary>
    public class BaseEntity : IEntity
    {
        /// <inheritdoc/>
        public int Id { get; set; }

        /// <inheritdoc/>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <inheritdoc/>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <inheritdoc/>
        public int CreatedByUserId { get; set; } = -1;

        /// <inheritdoc/>
        public int UpdatedByUserId { get; set; } = -1;
    }
}