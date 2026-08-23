using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="ArtistBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ArtistFactory
{
    /// <summary>
    /// Creates an unclaimed artist profile with default valid values.
    /// </summary>
    public static ArtistEntity Create() => new ArtistBuilder().Build();

    /// <summary>
    /// Creates an unclaimed artist profile with a known slug (for slug-conflict tests).
    /// </summary>
    public static ArtistEntity CreateWithSlug(string slug) => new ArtistBuilder().WithSlug(slug).Build();

    /// <summary>
    /// Creates an artist profile with a specific name and slug.
    /// </summary>
    public static ArtistEntity Create(string name, string slug) =>
        new ArtistBuilder().WithName(name).WithSlug(slug).Build();

    /// <summary>
    /// Creates an artist profile with a specific name, slug and biography.
    /// </summary>
    public static ArtistEntity Create(string name, string slug, string? bio) =>
        new ArtistBuilder().WithName(name).WithSlug(slug).WithBio(bio).Build();

    /// <summary>
    /// Creates an artist profile carrying every identity field.
    /// </summary>
    public static ArtistEntity CreateWithIdentity(
        string realName,
        IReadOnlyList<string> aliases,
        DateOnly birthdate,
        string hometown
    ) =>
        new ArtistBuilder()
            .WithRealName(realName)
            .WithAliases(aliases)
            .WithBirthdate(birthdate)
            .WithHometown(hometown)
            .Build();

    /// <summary>
    /// Creates an artist profile already claimed by the given identity user.
    /// </summary>
    public static ArtistEntity CreateClaimed(Guid userId) => new ArtistBuilder().AsClaimedBy(userId).Build();

    /// <summary>
    /// Creates a list of unclaimed artist profiles.
    /// </summary>
    public static List<ArtistEntity> CreateMany(int count) => Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
