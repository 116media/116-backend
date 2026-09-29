using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteLyrics.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminForceUnpromoteLyricsRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminForceUnpromoteLyricsRequestBuilder
{
    private string _reason;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminForceUnpromoteLyricsRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminForceUnpromoteLyricsRequestBuilder()
    {
        _reason = "Government takedown request";
    }

    /// <summary>
    /// Sets the reason.
    /// </summary>
    /// <param name="reason">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminForceUnpromoteLyricsRequestBuilder WithReason(string reason)
    {
        _reason = reason;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminForceUnpromoteLyricsRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminForceUnpromoteLyricsRequest instance.</returns>
    public AdminForceUnpromoteLyricsRequest Build()
    {
        return new AdminForceUnpromoteLyricsRequest(Reason: _reason);
    }
}
