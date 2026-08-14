using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ArtistEntity"/> instances in tests.
/// For test code, prefer using ArtistFactory instead of direct Builder usage.
/// </summary>
internal class ArtistBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = TestConstants.Content.Editorial.Artist.ValidName;
    private string _slug = $"{TestConstants.Content.Editorial.Artist.ValidSlug}-{Guid.NewGuid():N}";
    private string? _bio;
    private Guid? _avatarFileId;
    private Guid? _userId;
    private string? _realName;
    private IReadOnlyList<string>? _aliases;
    private DateOnly? _birthdate;
    private string? _hometown;

    /// <summary>
    /// Sets the artist ID.
    /// </summary>
    public ArtistBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the artist's display name.
    /// </summary>
    public ArtistBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the artist's URL-safe slug.
    /// </summary>
    public ArtistBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>
    /// Sets the artist's free-text biography.
    /// </summary>
    public ArtistBuilder WithBio(string? bio)
    {
        _bio = bio;
        return this;
    }

    /// <summary>
    /// Sets the artist's avatar file ID.
    /// </summary>
    public ArtistBuilder WithAvatarFileId(Guid avatarFileId)
    {
        _avatarFileId = avatarFileId;
        return this;
    }

    /// <summary>
    /// Sets the artist's legal or birth name.
    /// </summary>
    public ArtistBuilder WithRealName(string? realName)
    {
        _realName = realName;
        return this;
    }

    /// <summary>
    /// Sets the artist's alternate names.
    /// </summary>
    public ArtistBuilder WithAliases(IReadOnlyList<string>? aliases)
    {
        _aliases = aliases;
        return this;
    }

    /// <summary>
    /// Sets the artist's date of birth.
    /// </summary>
    public ArtistBuilder WithBirthdate(DateOnly? birthdate)
    {
        _birthdate = birthdate;
        return this;
    }

    /// <summary>
    /// Sets the artist's hometown.
    /// </summary>
    public ArtistBuilder WithHometown(string? hometown)
    {
        _hometown = hometown;
        return this;
    }

    /// <summary>
    /// Marks the artist profile as claimed by the given identity user.
    /// </summary>
    public ArtistBuilder AsClaimedBy(Guid userId)
    {
        _userId = userId;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="ArtistEntity"/> instance.
    /// </summary>
    public ArtistEntity Build()
    {
        ArtistEntity entity = ArtistEntity.Create(
            id: _id,
            name: _name,
            slug: _slug,
            bio: _bio,
            realName: _realName,
            aliases: _aliases,
            birthdate: _birthdate,
            hometown: _hometown,
            errors: TestErrorsFactory.CreateArtistErrors()
        );

        if (_avatarFileId.HasValue)
        {
            entity.SetAvatarFileId(_avatarFileId.Value);
        }

        if (_userId.HasValue)
        {
            entity.ClaimOwnership(_userId.Value, TestErrorsFactory.CreateArtistErrors());
        }

        entity.CreatedAt = DateTime.UtcNow;

        return entity;
    }
}
