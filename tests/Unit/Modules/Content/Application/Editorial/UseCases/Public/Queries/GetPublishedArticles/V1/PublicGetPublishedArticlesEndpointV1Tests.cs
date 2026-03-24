using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles.V1;

/// <summary>
/// Unit tests for <see cref="PublicGetPublishedArticlesResponse"/>.
/// </summary>
public class PublicGetPublishedArticlesEndpointV1Tests
{
    [Fact]
    public void PublicGetPublishedArticlesResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var paginated = new PaginatedResult<ArticleSummaryDto>(1, 10, 1, [CreateArticleSummaryDto()]);

        // Act
        var response = new PublicGetPublishedArticlesResponse(Articles: paginated);

        // Assert
        response.Should().NotBeNull();
        response.Articles.Should().Be(paginated);
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
            IsFeatured: false,
            PublishedAt: null
        );
}
