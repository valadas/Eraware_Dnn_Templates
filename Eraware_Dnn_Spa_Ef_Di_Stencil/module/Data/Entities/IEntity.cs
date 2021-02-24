// MIT License
// Copyright $ext_companyname$

using System;

namespace $ext_rootnamespace$.Data.Entities
{
    /// <summary>
    /// Ensures entities have some common properties.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time the entity was first created.
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time the entity was last updated.
        /// </summary>
        DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the Dnn user ID that created the entity.
        /// </summary>
        int CreatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the Dnn user ID that last updated the entity.
        /// </summary>
        int UpdatedByUserId { get; set; }
    }
}
