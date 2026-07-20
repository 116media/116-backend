using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Constants;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a content category (e.g., "Artist Profile", "116 Le Focus", "Chronique Sale").
/// Every article and video must belong to exactly one category. Categories determine whether
/// content is free or paid, and carry their own pricing configuration.
/// </summary>
public class CategoryEntity : Aggregate<Guid>
{
    /// <summary>
    /// The unique identifier of the content type this category belongs to (e.g., "Article", "Video").
    /// </summary>
    public Guid ContentTypeId { get; private set; }

    /// <summary>
    /// Display name of the category (e.g., "Artist Profile", "116 Le Focus").
    /// </summary>
    [MaxLength(length: ContentConstants.MaxCategoryNameLength)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// URL-safe slug for the category (e.g., "artist-profile", "116-le-focus").
    /// Must be unique across all categories.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxCategorySlugLength)]
    public string Slug { get; private set; } = null!;

    /// <summary>
    /// Human-readable description of the category.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxCategoryDescriptionLength)]
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Indicates whether content in this category is free (no payment required).
    /// When true, the payment flow is skipped entirely.
    /// </summary>
    public bool IsFree { get; private set; }

    /// <summary>
    /// Indicates whether this category is currently active and available for content creation.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// When true, this category is the gossip category. Published articles belonging to it
    /// are used as fallback content on the homepage promotion feed and populate the gossip strip.
    /// Exactly one article category should have this flag set at a time.
    /// Gossip is an article-only concept — no video category carries this flag.
    /// </summary>
    public bool IsGossip { get; private set; }

    /// <summary>
    /// Optional reference to a FileEntity storing the show's poster image.
    /// Nullable because article categories do not need a poster — only video
    /// categories (shows) use it for the exclusive homepage section.
    /// Resolved to a URL at mapping time via IFileRepository.
    /// </summary>
    public Guid? PosterFileId { get; private set; }

    /// <summary>
    /// When true, this show is the currently featured exclusive.
    /// Exactly one category should have this flag set at a time (mutex).
    /// The handler layer enforces the mutex by unsetting the previous exclusive
    /// before setting the new one.
    /// </summary>
    public bool IsExclusive { get; private set; }

    /// <summary>
    /// When true, this category is the default category assigned to lyrics pages created without
    /// an explicit category (e.g. via <c>GetDefaultLyricsCategoryAsync</c>). Mirrors
    /// <see cref="IsGossip" />'s shape. Exactly one category should have this flag set at a time.
    /// A lyrics-only concept — no article or video category carries this flag.
    /// </summary>
    public bool IsDefaultForLyrics { get; private set; }

    /// <summary>
    /// The moment this category was pinned to the content feed, or null if it is not pinned.
    /// A non-null value means the category appears as a section in its content-type feed
    /// (video feed, and later the article feed). The timestamp also serves as the FIFO
    /// eviction key: when the per-content-type cap is exceeded, the category with the
    /// oldest PinnedToFeedAt is unpinned first.
    /// </summary>
    public DateTimeOffset? PinnedToFeedAt { get; private set; }

    /// <summary>
    /// Whether this category is currently pinned to the content feed.
    /// Derived from <see cref="PinnedToFeedAt" /> — true when a pin timestamp is set.
    /// Not mapped: EF reads and writes <see cref="PinnedToFeedAt" /> only.
    /// </summary>
    [NotMapped]
    public bool IsPinnedToFeed => PinnedToFeedAt is not null;

    /// <summary>
    /// The content type this category is classified under.
    /// </summary>
    public ContentTypeEntity ContentType { get; private set; } = null!;

    /// <summary>
    /// The pricing tiers configured for this category.
    /// </summary>
    public ICollection<CategoryPricingEntity> Pricing { get; } = new List<CategoryPricingEntity>();

    /// <summary>
    /// Package slots that reference this category.
    /// </summary>
    public ICollection<PackageSlotEntity> PackageSlots { get; } = new List<PackageSlotEntity>();

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private CategoryEntity() { }

    /// <summary>
    /// Creates a new category entity.
    /// </summary>
    /// <param name="id">The unique identifier for the category.</param>
    /// <param name="contentTypeId">The identifier of the associated content type.</param>
    /// <param name="name">The display name of the category.</param>
    /// <param name="slug">The URL-safe slug for the category.</param>
    /// <param name="description">The description of the category.</param>
    /// <param name="isFree">Whether content in this category is free.</param>
    /// <param name="isGossip">Whether this is the gossip category used for homepage feed fallbacks.</param>
    /// <param name="isExclusive">Whether this category is the exclusive show featured on the homepage.</param>
    /// <param name="isDefaultForLyrics">Whether this is the default category assigned to lyrics pages.</param>
    /// <returns>A new <see cref="CategoryEntity" /> instance.</returns>
    public static CategoryEntity Create(
        Guid id,
        Guid contentTypeId,
        string name,
        string slug,
        string description,
        bool isFree,
        CategoryErrors errors,
        bool isGossip = false,
        bool isExclusive = false,
        bool isDefaultForLyrics = false
    )
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw errors.NameRequired();
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw errors.SlugRequired();
        }

        return new CategoryEntity
        {
            Id = id,
            ContentTypeId = contentTypeId,
            Name = name,
            Slug = slug,
            Description = description,
            IsFree = isFree,
            IsGossip = isGossip,
            IsExclusive = isExclusive,
            IsDefaultForLyrics = isDefaultForLyrics,
            IsActive = true,
        };
    }

    /// <summary>
    /// Updates the category's display name, slug, and description.
    /// </summary>
    /// <param name="name">The new display name.</param>
    /// <param name="slug">The new URL-safe slug.</param>
    /// <param name="description">The new description.</param>
    /// <param name="isGossip">Whether this is the gossip category used for homepage feed fallbacks.</param>
    /// <param name="isExclusive">Whether this category is the exclusive show featured on the homepage.</param>
    /// <param name="errors">The errors factory instance.</param>
    /// <param name="isDefaultForLyrics">
    /// Whether this is the default category assigned to lyrics pages. Defaults to <c>false</c> and
    /// is trailing/optional so existing call sites that do not yet thread this value through keep
    /// compiling; once an admin update use case is wired to it, callers must pass the current value
    /// explicitly or this call will silently clear the flag.
    /// </param>
    public void Update(
        string name,
        string slug,
        string description,
        bool isGossip,
        bool isExclusive,
        CategoryErrors errors,
        bool isDefaultForLyrics = false
    )
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw errors.NameRequired();
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw errors.SlugRequired();
        }

        Name = name;
        Slug = slug;
        Description = description;
        IsGossip = isGossip;
        IsExclusive = isExclusive;
        IsDefaultForLyrics = isDefaultForLyrics;
    }

    /// <summary>
    /// Marks this category as the default category for lyrics pages.
    /// The handler is responsible for calling <see cref="ClearDefaultForLyrics" /> on the
    /// previously default category before calling this method.
    /// </summary>
    public void SetDefaultForLyrics()
    {
        IsDefaultForLyrics = true;
    }

    /// <summary>
    /// Removes the default-for-lyrics flag from this category.
    /// Called by the handler on the previously default category before setting a new one.
    /// </summary>
    public void ClearDefaultForLyrics()
    {
        IsDefaultForLyrics = false;
    }

    /// <summary>
    /// Guards that this category is eligible for use on a commissioned order item.
    /// A category is commissionable when it is active and not free.
    /// </summary>
    /// <param name="errors">The errors factory instance.</param>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">
    /// Thrown when the category is inactive or free, surfaced as a not-found error to avoid leaking state.
    /// </exception>
    public void EnsureCommissionable(CategoryErrors errors)
    {
        if (!IsActive || IsFree)
        {
            throw errors.NotFound(id: Id);
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
        return true;
    }

    /// <summary>
    /// Sets or clears the poster image file reference.
    /// </summary>
    /// <param name="posterFileId">The FileEntity ID, or null to clear.</param>
    public void SetPosterFileId(Guid? posterFileId)
    {
        PosterFileId = posterFileId;
    }

    /// <summary>
    /// Marks this category as the exclusive show.
    /// The handler is responsible for calling ClearExclusive() on the previously
    /// exclusive category before calling this method.
    /// </summary>
    public void SetExclusive()
    {
        IsExclusive = true;
    }

    /// <summary>
    /// Removes the exclusive flag from this category.
    /// Called by the handler on the previously exclusive category before setting a new one.
    /// </summary>
    public void ClearExclusive()
    {
        IsExclusive = false;
    }

    /// <summary>
    /// Pins this category to the content feed, stamping the current time.
    /// The handler is responsible for enforcing the per-content-type cap and unpinning
    /// the oldest pinned category when the cap would be exceeded. Re-pinning an already
    /// pinned category refreshes its timestamp (moving it to the front of the FIFO queue).
    /// </summary>
    public void PinToFeed()
    {
        PinnedToFeedAt = DateTimeOffset.UtcNow;
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
        return true;
    }
}
