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
        public DateTime CreatedAt { get; set; }

        /// <inheritdoc/>
        public DateTime UpdatedAt { get; set; }

        /// <inheritdoc/>
        public int CreatedByUserId { get; set; }

        /// <inheritdoc/>
        public int UpdatedByUserId { get; set; }
    }
}