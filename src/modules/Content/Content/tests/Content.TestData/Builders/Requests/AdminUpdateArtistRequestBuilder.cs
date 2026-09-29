using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArtist.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminUpdateArtistRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminUpdateArtistRequestBuilder
{
    private string _name;
    private string? _bio;
    private string? _realName;
    private IReadOnlyList<string>? _aliases;
    private DateOnly? _birthdate;
    private string? _hometown;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminUpdateArtistRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminUpdateArtistRequestBuilder()
    {
        _name = "Name";
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
    public AdminUpdateArtistRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the bio.
    /// </summary>
    /// <param name="bio">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateArtistRequestBuilder WithBio(string? bio)
    {
        _bio = bio;
        return this;
    }

    /// <summary>
    /// Sets the real name.
    /// </summary>
    /// <param name="realName">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateArtistRequestBuilder WithRealName(string? realName)
    {
        _realName = realName;
        return this;
    }

    /// <summary>
    /// Sets the aliases.
    /// </summary>
    /// <param name="aliases">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateArtistRequestBuilder WithAliases(IReadOnlyList<string>? aliases)
    {
        _aliases = aliases;
        return this;
    }

    /// <summary>
    /// Sets the birthdate.
    /// </summary>
    /// <param name="birthdate">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateArtistRequestBuilder WithBirthdate(DateOnly? birthdate)
    {
        _birthdate = birthdate;
        return this;
    }

    /// <summary>
    /// Sets the hometown.
    /// </summary>
    /// <param name="hometown">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminUpdateArtistRequestBuilder WithHometown(string? hometown)
    {
        _hometown = hometown;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminUpdateArtistRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminUpdateArtistRequest instance.</returns>
    public AdminUpdateArtistRequest Build()
    {
        return new AdminUpdateArtistRequest(
            Name: _name,
            Bio: _bio,
            RealName: _realName,
            Aliases: _aliases,
            Birthdate: _birthdate,
            Hometown: _hometown
        );
    }
}
