using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="TagBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class TagFactory
{
    /// <summary>
    /// Creates a tag with default random values.
    /// </summary>
    public static TagEntity Create() => new TagBuilder().Build();

    /// <summary>
    /// Creates a tag with specific name and slug.
    /// </summary>
    public static TagEntity Create(string name, string slug) => new TagBuilder().WithName(name).WithSlug(slug).Build();

    /// <summary>
    /// Creates a tag with known default values.
    /// </summary>
    public static TagEntity CreateDefault() =>
        new TagBuilder().WithName(TestConstants.Tag.ValidName).WithSlug(TestConstants.Tag.ValidSlug).Build();

    /// <summary>
    /// Creates a list of tags with the specified count.
    /// </summary>
    public static List<TagEntity> CreateMany(int count) => Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
