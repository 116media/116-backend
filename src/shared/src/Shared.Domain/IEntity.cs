namespace _116.Shared.Domain;

/// <summary>
/// A base generic interface for entities with a typed identifier.
/// </summary>
/// <typeparam name="T">The type of the entity's identifier.</typeparam>
public interface IEntity<T> : IEntity
{
    /// <summary>
    /// The identifier of the entity, assigned once when it is constructed.
    /// </summary>
    public T Id { get; }
}

/// <summary>
/// A non-generic base interface for entities.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets or sets the creation timestamp of the entity.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier the entity's creator.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the last updated timestamp of the entity.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier the entity's last updater.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
