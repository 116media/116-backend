using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetArticleByIdResponse"/>.
/// </summary>
public class AdminGetArticleByIdEndpointV1Tests
{
    [Fact]
    public void AdminGetArticleByIdResponse_ShouldConstructCorrectly()
    {
        // Arrange
        ArticleDetailDto dto = CreateArticleDetailDto();

        // Act
        var response = new AdminGetArticleByIdResponse(Article: dto);

        // Assert
        response.Should().NotBeNull();
        response.Article.Should().Be(dto);
    }

    private static ArticleDetailDto CreateArticleDetailDto() =>
        new(
            Id: Guid.NewGuid(),
            CategoryId: Guid.NewGuid(),
            CategoryName: "Test",
            Title: "Test",
            Slug: "test",
            Headline: "Test",
            Body: "Test",
            CoverImageUrl: null,
            AuthorId: "Test",
            Status: "Draft",
            RejectionReason: null,
            IsFeatured: false,
            FeaturedUntil: null,
            PublishedAt: null,
            MetaTitle: null,
            MetaDescription: null,
            Images: [],
            Tags: [],
            ReadTimeInMinutes: 0,
            CreatedAt: null,
            UpdatedAt: null
        );
}
