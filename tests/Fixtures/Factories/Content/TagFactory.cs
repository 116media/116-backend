using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="TagEntity"/> instances in tests.
/// </summary>
public static class TagFactory
{
    /// <summary>Creates a tag with default random values.</summary>
    public static TagEntity Create() => new TagBuilder().Build();

    /// <summary>Creates a tag with specific name and slug.</summary>
    public static TagEntity Create(string name, string slug) => new TagBuilder().WithName(name).WithSlug(slug).Build();

    /// <summary>Creates a tag with a specific ID.</summary>
    public static TagEntity CreateWithId(Guid id) => new TagBuilder().WithId(id).Build();

    /// <summary>Creates a tag with known default values.</summary>
    public static TagEntity CreateDefault() =>
        new TagBuilder()
            .WithName(TestConstants.Content.Tag.ValidName)
            .WithSlug(TestConstants.Content.Tag.ValidSlug)
            .Build();

    /// <summary>Creates a list of tags with the specified count.</summary>
    public static List<TagEntity> CreateMany(int count) => Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
