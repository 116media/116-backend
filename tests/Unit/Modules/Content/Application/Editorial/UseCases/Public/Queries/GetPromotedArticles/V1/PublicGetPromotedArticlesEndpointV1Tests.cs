using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles.V1;

/// <summary>
/// Unit tests for <see cref="PublicGetPromotedArticlesResponse"/>.
/// </summary>
public class PublicGetPromotedArticlesEndpointV1Tests
{
    [Fact]
    public void PublicGetPromotedArticlesResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<ArticleSummaryDto> articles = [CreateArticleSummaryDto()];

        // Act
        var response = new PublicGetPromotedArticlesResponse(Articles: articles);

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
            Status: EnumContentStatus.Published,
            IsPromoted: false,
            PublishedAt: null
        );
}
