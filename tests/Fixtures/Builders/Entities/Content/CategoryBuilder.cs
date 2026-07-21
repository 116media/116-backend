using System.Reflection;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="CategoryEntity"/> instances in tests.
/// For test code, prefer using CategoryFactory instead of direct Builder usage.
/// </summary>
internal class CategoryBuilder
{
    private readonly Faker _faker = new();

    private Guid _id;
    private Guid _contentTypeId;
    private string _name;
    private string _slug;
    private string _description = "Default category description";
    private bool _isFree;
    private bool _isActive = true;
    private bool _isExclusive;
    private bool _isGossip;
    private bool _isDefaultForLyrics;
    private Guid? _posterFileId;
    private DateTimeOffset? _pinnedToFeedAt;
    private ContentTypeEntity? _contentType;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryBuilder"/> class with random default values.
    /// </summary>
    public CategoryBuilder(Guid contentTypeId)
    {
        _id = Guid.NewGuid();
        _contentTypeId = contentTypeId;
        string suffix = Guid.NewGuid().ToString("N")[..8];
        string word = _faker.Lorem.Word().ToLower();
        _name = $"{word}-{suffix}"[..Math.Min(TestConstants.Content.Category.NameMaxLength, $"{word}-{suffix}".Length)];
        _slug = $"{word}-{suffix}"[..Math.Min(TestConstants.Content.Category.SlugMaxLength, $"{word}-{suffix}".Length)];
    }

    /// <summary>
    /// Sets the category ID.
    /// </summary>
    public CategoryBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the content type ID.
    /// </summary>
    public CategoryBuilder WithContentTypeId(Guid contentTypeId)
    {
        _contentTypeId = contentTypeId;
        return this;
    }

    /// <summary>
    /// Sets the category name.
    /// </summary>
    public CategoryBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the category slug.
    /// </summary>
    public CategoryBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>
    /// Sets the category description.
    /// </summary>
    public CategoryBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Marks the category as free.
    /// </summary>
    public CategoryBuilder AsFree()
    {
        _isFree = true;
        return this;
    }

    /// <summary>
    /// Marks the category as paid.
    /// </summary>
    public CategoryBuilder AsPaid()
    {
        _isFree = false;
        return this;
    }

    /// <summary>
    /// Marks the category as inactive.
    /// </summary>
    public CategoryBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>
    /// Marks the category as exclusive.
    /// </summary>
    public CategoryBuilder WithIsExclusive(bool isExclusive = true)
    {
        _isExclusive = isExclusive;
        return this;
    }

    /// <summary>
    /// Marks the category as the gossip fallback source (IsGossip = true).
    /// </summary>
    public CategoryBuilder AsGossip()
    {
        _isGossip = true;
        return this;
    }

    /// <summary>
    /// Marks the category as the default category for lyrics pages (IsDefaultForLyrics = true).
    /// </summary>
    public CategoryBuilder AsDefaultForLyrics()
    {
        _isDefaultForLyrics = true;
        return this;
    }

    /// <summary>
    /// Marks the category as pinned to the feed at the given time (defaults to "now").
    /// Pass distinct timestamps across categories to exercise FIFO ordering/eviction.
    /// </summary>
    public CategoryBuilder PinnedToFeedAt(DateTimeOffset? pinnedAt = null)
    {
        _pinnedToFeedAt = pinnedAt ?? DateTimeOffset.UtcNow;
        return this;
    }

    /// <summary>
    /// Sets the content type navigation property via reflection.
    /// </summary>
    public CategoryBuilder WithContentType(ContentTypeEntity contentType)
    {
        _contentType = contentType;
        _contentTypeId = contentType.Id;
        return this;
    }

    /// <summary>
    /// Sets the poster file ID.
    /// </summary>
    public CategoryBuilder WithPosterFileId(Guid? posterFileId = null)
    {
        _posterFileId = posterFileId ?? Guid.NewGuid();
        return this;
    }

    /// <summary>
    /// Builds the <see cref="CategoryEntity"/> instance.
    /// </summary>
    public CategoryEntity Build()
    {
        var entity = CategoryEntity.Create(
            _id,
            _contentTypeId,
            _name,
            _slug,
            _description,
            _isFree,
            TestErrorsFactory.CreateCategoryErrors(),
            isGossip: _isGossip,
            isExclusive: _isExclusive,
            isDefaultForLyrics: _isDefaultForLyrics
        );

        if (!_isActive)
        {
            entity.Deactivate();
        }

        if (_posterFileId.HasValue)
        {
            entity.SetPosterFileId(_posterFileId);
        }

        if (_pinnedToFeedAt.HasValue)
        {
            PropertyInfo pinnedProp = typeof(CategoryEntity).GetProperty(
                nameof(CategoryEntity.PinnedToFeedAt),
                BindingFlags.Public | BindingFlags.Instance
            )!;

            pinnedProp.SetValue(entity, _pinnedToFeedAt);
        }

        if (_contentType is not null)
        {
            PropertyInfo prop = typeof(CategoryEntity).GetProperty(
                nameof(CategoryEntity.ContentType),
                BindingFlags.Public | BindingFlags.Instance
            )!;

            prop.SetValue(entity, _contentType);
        }

        return entity;
    }
}
