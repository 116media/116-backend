using System.ComponentModel.DataAnnotations;
using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Constants;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a content discovery tag (e.g., "Fally Ipupa", "Kinshasa", "Afrobeats").
/// Tags are the primary discovery mechanism for public users browsing content.
/// </summary>
public class TagEntity : Aggregate<Guid>
{
    /// <summary>
    /// Display name of the tag (e.g., "Fally Ipupa", "Kinshasa").
    /// </summary>
    [MaxLength(length: ContentConstants.MaxTagNameLength)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// URL-safe slug for the tag (e.g., "fally-ipupa", "kinshasa").
    /// Used as a unique identifier in public-facing tag filter URLs.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxTagSlugLength)]
    public string Slug { get; private set; } = null!;

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private TagEntity() { }

    /// <summary>
    /// Creates a new tag entity.
    /// </summary>
    /// <param name="id">The unique identifier for the tag.</param>
    /// <param name="name">The display name of the tag.</param>
    /// <param name="slug">The URL-safe slug for the tag (lowercase, hyphens only).</param>
    /// <returns>A new <see cref="TagEntity" /> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when name or slug are empty or whitespace.</exception>
    public static TagEntity Create(Guid id, string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw TagErrors.NameRequired();
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw TagErrors.SlugRequired();
        }

        return new TagEntity
        {
            Id = id,
            Name = name,
            Slug = slug,
        };
    }
}
