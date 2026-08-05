using _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl.V1;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminAttachYoutubeVideoUrlRequest"/> instances in tests
/// with a valid default YouTube URL that satisfies the attach YouTube URL validator.
/// </summary>
public class AdminAttachYoutubeVideoUrlRequestBuilder
{
    private string _youtubeVideoUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminAttachYoutubeVideoUrlRequestBuilder"/> class
    /// with a valid default YouTube watch URL.
    /// </summary>
    public AdminAttachYoutubeVideoUrlRequestBuilder()
    {
        _youtubeVideoUrl = TestConstants.Video.ValidYoutubeVideoUrl;
    }

    /// <summary>
    /// Sets the YouTube video URL.
    /// </summary>
    /// <param name="youtubeVideoUrl">The YouTube video URL.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminAttachYoutubeVideoUrlRequestBuilder WithYoutubeVideoUrl(string youtubeVideoUrl)
    {
        _youtubeVideoUrl = youtubeVideoUrl;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminAttachYoutubeVideoUrlRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminAttachYoutubeVideoUrlRequest instance.</returns>
    public AdminAttachYoutubeVideoUrlRequest Build()
    {
        return new AdminAttachYoutubeVideoUrlRequest(YoutubeVideoUrl: _youtubeVideoUrl);
    }
}
