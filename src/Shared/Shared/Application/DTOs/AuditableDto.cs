using _116.Shared.Domain;

namespace _116.Shared.Application.DTOs;

/// <summary>
/// Base record for admin-facing DTOs that must expose all entity audit fields.
/// </summary>
/// <remarks>
/// Inherit from this record on any DTO returned by an admin detail or list query.
/// Mapster maps the four audit properties automatically by name convention from any entity
/// that extends <c>Entity&lt;T&gt;</c> — no additional mapper configuration is required.
/// Public-facing DTOs must NOT inherit from this type.
/// </remarks>
public abstract record AuditableDto : IEntity
{
    /// <summary>
    /// When the entity was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Identity of whom created the entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// When the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Identity of whom last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
