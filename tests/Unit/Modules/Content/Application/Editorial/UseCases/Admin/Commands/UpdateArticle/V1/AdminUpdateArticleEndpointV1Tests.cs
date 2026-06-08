using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle.V1;

/// <summary>
/// Unit tests for <see cref="AdminUpdateArticleResponse"/>.
/// </summary>
public class AdminUpdateArticleEndpointV1Tests
{
    [Fact]
    public void AdminUpdateArticleResponse_ShouldConstructCorrectly()
    {
        // Arrange
        ArticleDetailDto dto = CreateArticleDetailDto();

        // Act
        var response = new AdminUpdateArticleResponse(Article: dto);

        // Assert
        response.Should().NotBeNull();
        response.Article.Should().Be(dto);
    }

    [Fact]
    public void AdminUpdateArticleRequest_ShouldConstructCorrectly()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();

        // Act
        var request = new AdminUpdateArticleRequest(
            CategoryId: categoryId,
            Title: "Updated Title",
            Slug: "updated-slug",
            Headline: "Updated headline text for the article",
            Body: "<p>Updated body</p>",
            CustomerId: customerId,
            OrderItemId: orderItemId,
            SocialBoost: true,
            MetaTitle: "Updated Meta Title",
            MetaDescription: "Updated meta description"
        );

        // Assert
        request.Should().NotBeNull();
        request.CategoryId.Should().Be(categoryId);
        request.Title.Should().Be("Updated Title");
        request.Slug.Should().Be("updated-slug");
        request.Headline.Should().Be("Updated headline text for the article");
        request.Body.Should().Be("<p>Updated body</p>");
        request.CustomerId.Should().Be(customerId);
        request.OrderItemId.Should().Be(orderItemId);
        request.SocialBoost.Should().BeTrue();
        request.MetaTitle.Should().Be("Updated Meta Title");
        request.MetaDescription.Should().Be("Updated meta description");
    }

    [Fact]
    public void AdminUpdateArticleRequest_WithNullOptionalFields_ShouldConstructCorrectly()
    {
        // Act
        var request = new AdminUpdateArticleRequest(
            CategoryId: Guid.NewGuid(),
            Title: "Updated Title",
            Slug: "updated-slug",
            Headline: "Updated headline",
            Body: "<p>Body</p>",
            CustomerId: null,
            OrderItemId: null,
            SocialBoost: false,
            MetaTitle: null,
            MetaDescription: null
        );

        // Assert
        request.CustomerId.Should().BeNull();
        request.OrderItemId.Should().BeNull();
        request.MetaTitle.Should().BeNull();
        request.MetaDescription.Should().BeNull();
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
            SocialBoost: false,
            IsPromoted: false,
            PromotedUntil: null,
            PromotionLevelId: null,
            PromotionLevelName: null,
            PublishedAt: null,
            MetaTitle: null,
            MetaDescription: null,
            Images: [],
            Tags: [],
            ReadTimeInMinutes: 0,
            LikeCount: 0,
            CommentCount: 0,
            ShareCount: 0,
            BookmarkCount: 0
        );
}
