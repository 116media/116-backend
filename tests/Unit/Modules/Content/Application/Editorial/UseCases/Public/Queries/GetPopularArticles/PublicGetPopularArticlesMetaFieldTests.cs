using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;
using _116.Shared.Application.Metadata;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;

/// <summary>
/// Unit tests for <see cref="PublicGetPopularArticlesMetaField"/>. Referencing the
/// static <see cref="RouteMetadata"/> runs its initializer and asserts the OpenAPI
/// name, summary, and description exposed to the endpoint.
/// </summary>
public class PublicGetPopularArticlesMetaFieldTests
{
    private static RouteMetadata Meta => PublicGetPopularArticlesMetaField.PublicGetPopularArticles;

    [Fact]
    public void PublicGetPopularArticles_ShouldExposeExpectedName()
    {
        Meta.Name.Should().Be("PublicGetPopularArticles");
    }

    [Fact]
    public void PublicGetPopularArticles_ShouldExposeExpectedSummary()
    {
        Meta.Summary.Should().Be("Get popular articles");
    }

    [Fact]
    public void PublicGetPopularArticles_ShouldExposeNonEmptyDescription()
    {
        Meta.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetPopularArticles_Description_ShouldMentionRankingAndCaching()
    {
        Meta.Description.Should().Contain("weighted engagement score");
        Meta.Description.Should().Contain("cached");
        Meta.Description.Should().Contain("limit");
        Meta.Description.Should().Contain("categoryId");
        Meta.Description.Should().Contain("excludeId");
    }
}
