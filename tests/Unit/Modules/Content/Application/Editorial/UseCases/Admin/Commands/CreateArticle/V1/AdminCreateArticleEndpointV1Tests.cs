using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle.V1;

/// <summary>
/// Unit tests for <see cref="AdminCreateArticleResponse"/> and <see cref="AdminCreateArticleRequest"/>.
/// </summary>
public class AdminCreateArticleEndpointV1Tests
{
    [Fact]
    public void AdminCreateArticleRequest_ShouldConstructCorrectly()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();

        // Act
        var request = new AdminCreateArticleRequest(
            CategoryId: categoryId,
            Title: "Test Article",
            Slug: "test-article",
            CustomerId: customerId,
            OrderItemId: orderItemId
        );

        // Assert
        request.Should().NotBeNull();
        request.CategoryId.Should().Be(categoryId);
        request.Title.Should().Be("Test Article");
        request.Slug.Should().Be("test-article");
        request.CustomerId.Should().Be(customerId);
        request.OrderItemId.Should().Be(orderItemId);
    }

    [Fact]
    public void AdminCreateArticleRequest_WithNullOptionalFields_ShouldConstructCorrectly()
    {
        // Act
        var request = new AdminCreateArticleRequest(
            CategoryId: Guid.NewGuid(),
            Title: "Test Article",
            Slug: "test-article",
            CustomerId: null,
            OrderItemId: null
        );

        // Assert
        request.CustomerId.Should().BeNull();
        request.OrderItemId.Should().BeNull();
    }

    [Fact]
    public void AdminCreateArticleResponse_ShouldConstructCorrectly()
    {
        // Arrange
        ArticleDetailDto dto = CreateArticleDetailDto();

        // Act
        var response = new AdminCreateArticleResponse(Article: dto);

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
            Status: EnumContentStatus.Draft,
            RejectionReason: null,
            IsFeatured: false,
            FeaturedUntil: null,
            PublishedAt: null,
            MetaTitle: null,
            MetaDescription: null,
            Images: [],
            Tags: [],
            ReadTimeInMinutes: 0
        );
}
