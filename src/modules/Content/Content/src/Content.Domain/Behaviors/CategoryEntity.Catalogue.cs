using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Catalogue behaviour of <see cref="CategoryEntity" />. Its state lives in <c>Entities/CategoryEntity.cs</c>.
/// </summary>
public sealed partial class CategoryEntity
{
    /// <summary>
    /// Renames the category and its slug.
    /// </summary>
    /// <param name="name">The new display name.</param>
    /// <param name="slug">The new URL-safe slug.</param>
    /// <returns><c>true</c> if either value changed; otherwise <c>false</c>.</returns>
    public bool Rename(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new ContentRuleException(ContentRuleCodes.CategoryNameRequired);
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw new ContentRuleException(ContentRuleCodes.CategorySlugRequired);
        }

        if (Name == name && Slug == slug)
        {
            return false;
        }

        Name = name;
        Slug = slug;
        MarkChanged();

        return true;
    }

    /// <summary>
    /// Rewrites the category's description.
    /// </summary>
    /// <param name="description">The new description.</param>
    /// <returns><c>true</c> if the description changed; otherwise <c>false</c>.</returns>
    public bool Redescribe(string description)
    {
        if (Description == description)
        {
            return false;
        }

        Description = description;
        MarkChanged();

        return true;
    }

    /// <summary>
    /// Sets the three feed-placement flags together, since a category may hold at most the role
    /// each one names and the handler decides them as one.
    /// </summary>
    /// <param name="isGossip">Whether this is the gossip category used for homepage feed fallbacks.</param>
    /// <param name="isExclusive">Whether this category is the exclusive show featured on the homepage.</param>
    /// <param name="isDefaultForLyrics">
    /// Whether this is the default category assigned to lyrics pages. Required — deliberately not
    /// optional, because a defaulted value here silently clears a persisted flag at every call site
    /// that omits it. Callers must pass the intended value explicitly.
    /// </param>
    /// <returns><c>true</c> if any flag changed; otherwise <c>false</c>.</returns>
    public bool Reclassify(bool isGossip, bool isExclusive, bool isDefaultForLyrics)
    {
        if (IsGossip == isGossip && IsExclusive == isExclusive && IsDefaultForLyrics == isDefaultForLyrics)
        {
            return false;
        }

        IsGossip = isGossip;
        IsExclusive = isExclusive;
        IsDefaultForLyrics = isDefaultForLyrics;
        MarkChanged();

        return true;
    }

    /// <summary>
    /// Records one change notice per unit of work, however many edit verbs the handler calls.
    /// </summary>
    private void MarkChanged()
    {
        if (DomainEvents.OfType<CategoryChangedEvent>().Any())
        {
            return;
        }

        AddDomainEvent(new CategoryChangedEvent(CategoryId: Id));
    }

    /// <summary>
    /// Marks this category as the default category for lyrics pages.
    /// The handler is responsible for calling <see cref="ClearDefaultForLyrics" /> on the
    /// previously default category before calling this method.
    /// </summary>
    public void SetDefaultForLyrics()
    {
        IsDefaultForLyrics = true;
        AddDomainEvent(new CategoryChangedEvent(CategoryId: Id));
    }

    /// <summary>
    /// Removes the default-for-lyrics flag from this category.
    /// Called by the handler on the previously default category before setting a new one.
    /// </summary>
    public void ClearDefaultForLyrics()
    {
        IsDefaultForLyrics = false;
        AddDomainEvent(new CategoryChangedEvent(CategoryId: Id));
    }

    /// <summary>
    /// Guards that this category is eligible for use on a commissioned order item.
    /// A category is commissionable when it is active and not free.
    /// </summary>
    /// <exception cref="ContentRuleException">
    /// Thrown when the category is inactive or free, surfaced as a not-found error to avoid leaking state.
    /// </exception>
    public void EnsureCommissionable()
    {
        if (!IsActive || IsFree)
        {
            throw new ContentRuleException(ContentRuleCodes.CategoryNotFound, Id.ToString());
        }
    }

    /// <summary>
    /// Activates the category, making it selectable for content creation and orders.
    /// </summary>
    /// <returns>True if the category was activated, false if already active.</returns>
    public bool Activate()
    {
        if (IsActive)
        {
            return false;
        }

        IsActive = true;
        AddDomainEvent(new CategoryChangedEvent(CategoryId: Id));

        return true;
    }

    /// <summary>
    /// Deactivates the category, preventing it from being used in new content or orders.
    /// </summary>
    /// <returns>True if the category was deactivated, false if already inactive.</returns>
    public bool Deactivate()
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        AddDomainEvent(new CategoryChangedEvent(CategoryId: Id));

        return true;
    }

    /// <summary>
    /// Sets or clears the poster image file reference.
    /// </summary>
    /// <param name="posterFileId">The FileEntity ID, or null to clear.</param>
    public void SetPosterFileId(Guid? posterFileId)
    {
        PosterFileId = posterFileId;
        AddDomainEvent(new CategoryChangedEvent(CategoryId: Id));
    }

    /// <summary>
    /// Marks this category as the exclusive show.
    /// The handler is responsible for calling ClearExclusive() on the previously
    /// exclusive category before calling this method.
    /// </summary>
    public void SetExclusive()
    {
        IsExclusive = true;
        AddDomainEvent(new CategoryChangedEvent(CategoryId: Id));
    }

    /// <summary>
    /// Removes the exclusive flag from this category.
    /// Called by the handler on the previously exclusive category before setting a new one.
    /// </summary>
    public void ClearExclusive()
    {
        IsExclusive = false;
        AddDomainEvent(new CategoryChangedEvent(CategoryId: Id));
    }

    /// <summary>
    /// Pins this category to the content feed, stamping the current time.
    /// The handler is responsible for enforcing the per-content-type cap and unpinning
    /// the oldest pinned category when the cap would be exceeded. Re-pinning an already
    /// pinned category refreshes its timestamp (moving it to the front of the FIFO queue).
    /// </summary>
    public void PinToFeed(DateTimeOffset now)
    {
        PinnedToFeedAt = now;
        AddDomainEvent(new CategoryChangedEvent(CategoryId: Id));
    }

    /// <summary>
    /// Removes this category from the content feed.
    /// Called by the handler directly (unpin endpoint) or as part of FIFO eviction
    /// when the per-content-type cap is exceeded.
    /// </summary>
    /// <returns>True if the category was pinned and is now removed, false if it was not pinned.</returns>
    public bool UnpinFromFeed()
    {
        if (PinnedToFeedAt is null)
        {
            return false;
        }

        PinnedToFeedAt = null;
        AddDomainEvent(new CategoryChangedEvent(CategoryId: Id));

        return true;
    }
}
