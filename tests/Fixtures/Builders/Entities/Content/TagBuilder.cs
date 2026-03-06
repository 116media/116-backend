using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="TagEntity"/> instances in tests.
/// For test code, prefer using TagFactory instead of direct Builder usage.
/// </summary>
internal class TagBuilder
{
    private readonly Faker _faker = new();

    private Guid _id;
    private string _name;
    private string _slug;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagBuilder"/> class with random default values.
    /// </summary>
    public TagBuilder()
    {
        _id = Guid.NewGuid();
        string word = _faker.Lorem.Word().ToLower();
        _name = word[..Math.Min(TestConstants.Content.Tag.NameMaxLength, word.Length)];
        _slug = word[..Math.Min(TestConstants.Content.Tag.SlugMaxLength, word.Length)];
    }

    /// <summary>Sets the tag ID.</summary>
    public TagBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the tag name.</summary>
    public TagBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>Sets the tag slug.</summary>
    public TagBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>Builds the <see cref="TagEntity"/> instance.</summary>
    public TagEntity Build()
    {
        return TagEntity.Create(_id, _name, _slug);
    }
}
