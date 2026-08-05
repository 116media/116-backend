using _116.Content.Application.Interactions.UseCases.Public.Commands.AddVideoToPlaylist.V1;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="PublicAddVideoToPlaylistRequest"/> instances in tests.
/// </summary>
public class PublicAddVideoToPlaylistRequestBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _videoId;
    private int _sortOrder;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicAddVideoToPlaylistRequestBuilder"/> class
    /// with a valid random video identifier and sort order.
    /// </summary>
    public PublicAddVideoToPlaylistRequestBuilder()
    {
        _videoId = _faker.Random.Guid();
        _sortOrder = 1;
    }

    /// <summary>
    /// Sets the identifier of the video to add to the playlist.
    /// </summary>
    /// <param name="videoId">The unique identifier of the video.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicAddVideoToPlaylistRequestBuilder WithVideoId(Guid videoId)
    {
        _videoId = videoId;
        return this;
    }

    /// <summary>
    /// Sets the display order for the video within the playlist.
    /// </summary>
    /// <param name="sortOrder">The sort order for the video.</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicAddVideoToPlaylistRequestBuilder WithSortOrder(int sortOrder)
    {
        _sortOrder = sortOrder;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicAddVideoToPlaylistRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicAddVideoToPlaylistRequest instance.</returns>
    public PublicAddVideoToPlaylistRequest Build()
    {
        return new PublicAddVideoToPlaylistRequest(VideoId: _videoId, SortOrder: _sortOrder);
    }
}
