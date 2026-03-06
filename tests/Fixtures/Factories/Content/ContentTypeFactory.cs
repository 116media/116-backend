using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="ContentTypeEntity"/> instances in tests.
/// </summary>
public static class ContentTypeFactory
{
    /// <summary>Creates a content type with default random values.</summary>
    public static ContentTypeEntity Create() => new ContentTypeBuilder().Build();

    /// <summary>Creates a content type with a specific name.</summary>
    public static ContentTypeEntity Create(string name) => new ContentTypeBuilder().WithName(name).Build();

    /// <summary>Creates a content type with a specific ID.</summary>
    public static ContentTypeEntity CreateWithId(Guid id) => new ContentTypeBuilder().WithId(id).Build();

    /// <summary>Creates an inactive content type.</summary>
    public static ContentTypeEntity CreateInactive() => new ContentTypeBuilder().AsInactive().Build();

    /// <summary>Creates a content type with a known default name (ValidName).</summary>
    public static ContentTypeEntity CreateDefault() =>
        new ContentTypeBuilder().WithName(TestConstants.Content.ContentType.ValidName).Build();

    /// <summary>Creates a list of content types with the specified count.</summary>
    public static List<ContentTypeEntity> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
