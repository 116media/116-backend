using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminCreateArtistRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminCreateArtistRequestBuilder
{
    private string _name;
    private string _slug;
    private string? _bio;
    private string? _realName;
    private IReadOnlyList<string>? _aliases;
    private DateOnly? _birthdate;
    private string? _hometown;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminCreateArtistRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminCreateArtistRequestBuilder()
    {
        _name = "Name";
        _slug = $"slug-{Guid.NewGuid():N}";
        _bio = null;
        _realName = null;
        _aliases = null;
        _birthdate = null;
        _hometown = null;
    }

    /// <summary>
    /// Sets the name.
    /// </summary>
    /// <param name="name">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateArtistRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the slug.
    /// </summary>
    /// <param name="slug">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateArtistRequestBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>
    /// Sets the bio.
    /// </summary>
    /// <param name="bio">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateArtistRequestBuilder WithBio(string? bio)
    {
        _bio = bio;
        return this;
    }

    /// <summary>
    /// Sets the real name.
    /// </summary>
    /// <param name="realName">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateArtistRequestBuilder WithRealName(string? realName)
    {
        _realName = realName;
        return this;
    }

    /// <summary>
    /// Sets the aliases.
    /// </summary>
    /// <param name="aliases">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateArtistRequestBuilder WithAliases(IReadOnlyList<string>? aliases)
    {
        _aliases = aliases;
        return this;
    }

    /// <summary>
    /// Sets the birthdate.
    /// </summary>
    /// <param name="birthdate">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateArtistRequestBuilder WithBirthdate(DateOnly? birthdate)
    {
        _birthdate = birthdate;
        return this;
    }

    /// <summary>
    /// Sets the hometown.
    /// </summary>
    /// <param name="hometown">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminCreateArtistRequestBuilder WithHometown(string? hometown)
    {
        _hometown = hometown;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminCreateArtistRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminCreateArtistRequest instance.</returns>
    public AdminCreateArtistRequest Build()
    {
        return new AdminCreateArtistRequest(
            Name: _name,
            Slug: _slug,
            Bio: _bio,
            RealName: _realName,
            Aliases: _aliases,
            Birthdate: _birthdate,
            Hometown: _hometown
        );
    }
}
