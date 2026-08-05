using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle.V1;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminForceUnpromoteArticleRequest"/> instances in tests
/// with a valid default reason that satisfies the force-unpromote article validator.
/// </summary>
public class AdminForceUnpromoteArticleRequestBuilder
{
    private string _reason;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminForceUnpromoteArticleRequestBuilder"/> class
    /// with a valid default unpromote reason.
    /// </summary>
    public AdminForceUnpromoteArticleRequestBuilder()
    {
        _reason = TestConstants.Article.ValidRejectionReason;
    }

    /// <summary>
    /// Sets the unpromote reason.
    /// </summary>
    /// <param name="reason">The unpromote reason.</param>
    /// <returns>The builder instance for chaining.</returns>
    public AdminForceUnpromoteArticleRequestBuilder WithReason(string reason)
    {
        _reason = reason;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AdminForceUnpromoteArticleRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminForceUnpromoteArticleRequest instance.</returns>
    public AdminForceUnpromoteArticleRequest Build()
    {
        return new AdminForceUnpromoteArticleRequest(Reason: _reason);
    }
}
