using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="ContentTypeBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ContentTypeFactory
{
    /// <summary>
    /// Creates a content type with default random values.
    /// </summary>
    public static ContentTypeEntity Create() => new ContentTypeBuilder().Build();

    /// <summary>
    /// Creates a content type with a specific name.
    /// </summary>
    public static ContentTypeEntity Create(string name) => new ContentTypeBuilder().WithName(name).Build();

    /// <summary>
    /// Creates an inactive content type.
    /// </summary>
    public static ContentTypeEntity CreateInactive() => new ContentTypeBuilder().AsInactive().Build();

    /// <summary>
    /// Creates a content type with a known default name (ValidName).
    /// </summary>
    public static ContentTypeEntity CreateDefault() =>
        new ContentTypeBuilder().WithName(TestConstants.ContentType.ValidName).Build();

    /// <summary>
    /// Creates a list of content types with the specified count.
    /// </summary>
    public static List<ContentTypeEntity> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
