using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
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
    private string? _description;
    private bool _isFree;
    private bool _isActive = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryBuilder"/> class with random default values.
    /// </summary>
    public CategoryBuilder(Guid contentTypeId)
    {
        _id = Guid.NewGuid();
        _contentTypeId = contentTypeId;
        string word = _faker.Lorem.Word().ToLower();
        _name = word[..Math.Min(TestConstants.Content.Category.NameMaxLength, word.Length)];
        _slug = word[..Math.Min(TestConstants.Content.Category.SlugMaxLength, word.Length)];
    }

    /// <summary>Sets the category ID.</summary>
    public CategoryBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the content type ID.</summary>
    public CategoryBuilder WithContentTypeId(Guid contentTypeId)
    {
        _contentTypeId = contentTypeId;
        return this;
    }

    /// <summary>Sets the category name.</summary>
    public CategoryBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>Sets the category slug.</summary>
    public CategoryBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>Sets the category description.</summary>
    public CategoryBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>Marks the category as free.</summary>
    public CategoryBuilder AsFree()
    {
        _isFree = true;
        return this;
    }

    /// <summary>Marks the category as paid.</summary>
    public CategoryBuilder AsPaid()
    {
        _isFree = false;
        return this;
    }

    /// <summary>Marks the category as inactive.</summary>
    public CategoryBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>Builds the <see cref="CategoryEntity"/> instance.</summary>
    public CategoryEntity Build()
    {
        var entity = CategoryEntity.Create(_id, _contentTypeId, _name, _slug, _description, _isFree);

        if (!_isActive)
        {
            entity.Deactivate();
        }

        return entity;
    }
}
