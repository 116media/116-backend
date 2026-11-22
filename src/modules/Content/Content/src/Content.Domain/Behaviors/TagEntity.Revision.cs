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
    /// Updates the tag's name. The slug is assigned once at creation and never follows the name,
    /// so a published URL keeps resolving.
    /// </summary>
    /// <param name="name">The new display name for the tag.</param>
    /// <exception cref="ContentRuleException">Thrown when the name is empty or whitespace.</exception>
    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new ContentRuleException(ContentRuleCodes.TagNameRequired);
        }

        Name = name;

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
