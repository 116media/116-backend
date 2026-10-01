using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Revision behaviour of <see cref="TagEntity" />. Its state lives in <c>Entities/TagEntity.cs</c>.
/// </summary>
public partial class TagEntity
{
    /// <summary>
    /// Updates the tag's name and slug.
    /// </summary>
    /// <param name="name">The new display name for the tag.</param>
    /// <param name="slug">The new URL-safe slug for the tag.</param>
    /// <exception cref="ContentRuleException">Thrown when name or slug are empty or whitespace.</exception>
    public void Update(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new ContentRuleException(ContentRuleCodes.TagNameRequired);
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw new ContentRuleException(ContentRuleCodes.TagSlugRequired);
        }

        Name = name;
        Slug = slug;

        AddDomainEvent(new TagGraphChangedEvent(TagId: Id));
    }

    /// <summary>
    /// Declares the tag's removal so post-commit consumers (the tags cache
    /// invalidation) can act on the change. Called by the delete flow
    /// immediately before the repository removal.
    /// </summary>
    public void MarkDeleted()
    {
        AddDomainEvent(new TagGraphChangedEvent(TagId: Id));
    }
}
