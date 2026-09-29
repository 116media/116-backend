using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Content.Domain.ValueObjects;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a content category (e.g., "Artist Profile", "116 Le Focus", "Chronique Sale").
/// Every article and video must belong to exactly one category. Categories determine whether
/// content is free or paid, and carry their own pricing configuration.
/// </summary>
public partial class CategoryEntity : Aggregate<Guid>
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
    public Slug Slug { get; private set; } = null!;

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
    /// The pricing tiers configured for this category.
    /// </summary>
    public ICollection<CategoryPricingEntity> Pricing { get; } = new List<CategoryPricingEntity>();

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
        bool isGossip = false,
        bool isExclusive = false,
        bool isDefaultForLyrics = false
    )
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw new ContentRuleException(ContentRuleCodes.CategoryNameRequired);
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw new ContentRuleException(ContentRuleCodes.CategorySlugRequired);
        }

        var category = new CategoryEntity
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
        category.AddDomainEvent(new CategoryChangedEvent(CategoryId: id));

        return category;
    }
}
