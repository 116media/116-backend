using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle.V1;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Requests.Content;

/// <summary>
/// Fluent builder for creating <see cref="AdminRejectArticleRequest"/> instances in tests
/// with a valid default rejection reason that satisfies the reject article validator.
/// </summary>
public class AdminRejectArticleRequestBuilder
{
    private string _reason;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminRejectArticleRequestBuilder"/> class
    /// with a valid default rejection reason.
    /// </summary>
    public AdminRejectArticleRequestBuilder()
    {
        _reason = TestConstants.Article.ValidRejectionReason;
    }

    /// <summary>
    /// Builds the <see cref="AdminRejectArticleRequest"/> instance.
    /// </summary>
    /// <returns>A configured AdminRejectArticleRequest instance.</returns>
    public AdminRejectArticleRequest Build()
    {
        return new AdminRejectArticleRequest(Reason: _reason);
    }
}
