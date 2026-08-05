using _116.Content.Application.Interactions.UseCases.Public.Commands.CreatePlaylist.V1;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="PublicCreatePlaylistRequest"/> instances in tests.
/// </summary>
public class PublicCreatePlaylistRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private string _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicCreatePlaylistRequestBuilder"/> class
    /// with a valid random playlist name that satisfies the validator.
    /// </summary>
    public PublicCreatePlaylistRequestBuilder()
    {
        string words = _faker.Lorem.Sentence(wordCount: 3);
        _name = words[..Math.Min(TestConstants.Interactions.MaxPlaylistNameLength, words.Length)];
    }

    /// <summary>
    /// Sets the playlist name.
    /// </summary>
    /// <param name="name">The playlist display name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicCreatePlaylistRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicCreatePlaylistRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicCreatePlaylistRequest instance.</returns>
    public PublicCreatePlaylistRequest Build()
    {
        return new PublicCreatePlaylistRequest(Name: _name);
    }
}
