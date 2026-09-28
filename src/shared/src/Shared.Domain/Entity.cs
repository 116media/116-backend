namespace _116.Shared.Domain;

/// <summary>
/// Abstract base class for entities with auditing information and a typed identifier.
/// </summary>
/// <typeparam name="T">The type of the entity's identifier.</typeparam>
public abstract class Entity<T> : IEntity<T>
{
    /// <inheritdoc />
    public T Id { get; protected init; } = default!;

    /// <inheritdoc />
    public DateTime? CreatedAt { get; set; }

    /// <inheritdoc />
    public string? CreatedBy { get; set; }

    /// <inheritdoc />
    public DateTime? UpdatedAt { get; set; }

    /// <inheritdoc />
    public string? UpdatedBy { get; set; }
}
