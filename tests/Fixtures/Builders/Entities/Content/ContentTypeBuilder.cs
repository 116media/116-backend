using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ContentTypeEntity"/> instances in tests.
/// For test code, prefer using ContentTypeFactory instead of direct Builder usage.
/// </summary>
internal class ContentTypeBuilder
{
    private readonly Faker _faker = new();

    private Guid _id;
    private string _name;
    private bool _isActive = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeBuilder"/> class with random default values.
    /// </summary>
    public ContentTypeBuilder()
    {
        _id = Guid.NewGuid();
        string word = _faker.Lorem.Word();
        _name = word[..Math.Min(TestConstants.Content.ContentType.NameMaxLength, word.Length)];
    }

    /// <summary>Sets the content type ID.</summary>
    public ContentTypeBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the content type name.</summary>
    public ContentTypeBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>Marks the content type as inactive.</summary>
    public ContentTypeBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>Builds the <see cref="ContentTypeEntity"/> instance.</summary>
    public ContentTypeEntity Build()
    {
        var entity = ContentTypeEntity.Create(_id, _name);

        if (!_isActive)
        {
            entity.Deactivate();
        }

        return entity;
    }
}
