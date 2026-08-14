using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="ArtistEntity"/> instances in tests.
/// </summary>
public static class ArtistFactory
{
    /// <summary>
    /// Creates an unclaimed artist profile with default valid values.
    /// </summary>
    public static ArtistEntity Create() => new ArtistBuilder().Build();

    /// <summary>
    /// Creates an unclaimed artist profile with a specific ID.
    /// </summary>
    public static ArtistEntity CreateWithId(Guid id) => new ArtistBuilder().WithId(id).Build();

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
    /// Creates an artist profile with a biography set.
    /// </summary>
    public static ArtistEntity CreateWithBio(string bio) => new ArtistBuilder().WithBio(bio).Build();

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
    /// Creates an artist profile with an avatar file id set.
    /// </summary>
    public static ArtistEntity CreateWithAvatarFileId(Guid avatarFileId) =>
        new ArtistBuilder().WithAvatarFileId(avatarFileId).Build();

    /// <summary>
    /// Creates an artist profile already claimed by the given identity user.
    /// </summary>
    public static ArtistEntity CreateClaimed(Guid userId) => new ArtistBuilder().AsClaimedBy(userId).Build();

    /// <summary>
    /// Creates a list of unclaimed artist profiles.
    /// </summary>
    public static List<ArtistEntity> CreateMany(int count) => Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
