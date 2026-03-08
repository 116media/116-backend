using System.ComponentModel.DataAnnotations;
using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Constants;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a top-level content format supported by the platform (e.g., "Article", "Video").
/// Content types are the first lookup table that must be seeded — categories cannot exist without one.
/// </summary>
public class ContentTypeEntity : Aggregate<Guid>
{
    /// <summary>
    /// Name of the content type (e.g., "Article", "Video").
    /// </summary>
    [MaxLength(length: ContentConstants.MaxContentTypeNameLength)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Indicates whether this content type is active and available for category assignment.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private ContentTypeEntity() { }

    /// <summary>
    /// Creates a new content type entity.
    /// </summary>
    /// <param name="id">The unique identifier for the content type.</param>
    /// <param name="name">The display name of the content type.</param>
    /// <returns>A new <see cref="ContentTypeEntity" /> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when name is empty or whitespace.</exception>
    public static ContentTypeEntity Create(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw ContentTypeErrors.NameRequired();
        }

        return new ContentTypeEntity { Id = id, Name = name };
    }

    /// <summary>
    /// Activates the content type, making it selectable when creating new categories.
    /// </summary>
    /// <returns>True if the content type was activated, false if already active.</returns>
    public bool Activate()
    {
        if (IsActive)
        {
            return false;
        }

        IsActive = true;
        return true;
    }

    /// <summary>
    /// Deactivates the content type, preventing it from being assigned to new categories.
    /// </summary>
    /// <returns>True if the content type was deactivated, false if already inactive.</returns>
    public bool Deactivate()
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        return true;
    }
}
