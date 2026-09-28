using _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideLyricsRevision.V1;

namespace _116.Content.TestData.Builders.Requests;

/// <summary>
/// Fluent builder for creating <see cref="AdminDecideLyricsRevisionRequest"/> instances in tests
/// with valid default values that satisfy its validator.
/// </summary>
public class AdminDecideLyricsRevisionRequestBuilder
{
    private bool _accept;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminDecideLyricsRevisionRequestBuilder"/> class with valid default values.
    /// </summary>
    public AdminDecideLyricsRevisionRequestBuilder()
    {
        _accept = true;
    }

    /// <summary>
    /// Sets the accept.
    /// </summary>
    /// <param name="accept">The value to build with.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminDecideLyricsRevisionRequestBuilder WithAccept(bool accept)
    {
        _accept = accept;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminDecideLyricsRevisionRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminDecideLyricsRevisionRequest instance.</returns>
    public AdminDecideLyricsRevisionRequest Build()
    {
        return new AdminDecideLyricsRevisionRequest(Accept: _accept);
    }
}
