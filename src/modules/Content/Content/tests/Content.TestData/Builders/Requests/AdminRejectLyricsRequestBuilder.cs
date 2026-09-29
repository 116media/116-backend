using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyrics.V1;
using _116.Tests.TestData.Constants;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminRejectLyricsRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminRejectLyricsRequestBuilder
{
    private string _reason;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminRejectLyricsRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminRejectLyricsRequestBuilder()
    {
        _reason = TestConstants.Lyrics.ValidRejectionReason;
    }

    /// <summary>
    /// Sets the reason.
    /// </summary>
    /// <param name="reason">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminRejectLyricsRequestBuilder WithReason(string reason)
    {
        _reason = reason;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminRejectLyricsRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminRejectLyricsRequest instance.</returns>
    public AdminRejectLyricsRequest Build()
    {
        return new AdminRejectLyricsRequest(Reason: _reason);
    }
}
