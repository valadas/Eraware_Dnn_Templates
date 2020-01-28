namespace $ext_rootnamespace$.Data.Entities
{
    /// <summary>
    /// Ensures entities have an ID.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        int Id { get; set; }
    }
}