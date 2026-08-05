using _116.Content.Application.Interactions.UseCases.Public.Commands.RateVideo.V1;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="PublicRateVideoRequest"/> instances in tests.
/// </summary>
public class PublicRateVideoRequestBuilder
{
    private short _stars;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicRateVideoRequestBuilder"/> class
    /// with a valid star rating that satisfies the validator (1–5 inclusive).
    /// </summary>
    public PublicRateVideoRequestBuilder()
    {
        _stars = TestConstants.Interactions.ValidStarRating;
    }

    /// <summary>
    /// Sets the star rating value.
    /// </summary>
    /// <param name="stars">The star rating (valid range is 1–5 inclusive).</param>
    /// <returns>The builder instance for chaining.</returns>
    public PublicRateVideoRequestBuilder WithStars(short stars)
    {
        _stars = stars;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PublicRateVideoRequest"/> instance.
    /// </summary>
    /// <returns>A configured PublicRateVideoRequest instance.</returns>
    public PublicRateVideoRequest Build()
    {
        return new PublicRateVideoRequest(Stars: _stars);
    }
}
