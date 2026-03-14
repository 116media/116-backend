using _116.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedArticles.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedArticles.V1;

/// <summary>
/// Unit tests for <see cref="PublicGetFeaturedArticlesResponse"/>.
/// </summary>
public class PublicGetFeaturedArticlesEndpointV1Tests
{
    [Fact]
    public void PublicGetFeaturedArticlesResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<ArticleSummaryDto> articles = [CreateArticleSummaryDto()];

        // Act
        var response = new PublicGetFeaturedArticlesResponse(Articles: articles);

        // Assert
        response.Should().NotBeNull();
        response.Articles.Should().BeSameAs(articles);
    }

    private static ArticleSummaryDto CreateArticleSummaryDto() =>
        new(
            Id: Guid.NewGuid(),
            CategoryId: Guid.NewGuid(),
            CategoryName: "Test",
            Title: "Test",
            Slug: "test",
            Headline: "Test",
            CoverImageUrl: null,
            AuthorId: "Test",
            Status: "Published",
            IsFeatured: false,
            PublishedAt: null,
            CreatedAt: null
        );
}
