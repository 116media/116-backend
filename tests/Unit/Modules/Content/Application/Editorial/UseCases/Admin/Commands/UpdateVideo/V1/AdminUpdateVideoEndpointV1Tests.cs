using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo.V1;

/// <summary>
/// Unit tests for <see cref="AdminUpdateVideoResponse"/>.
/// </summary>
public class AdminUpdateVideoEndpointV1Tests
{
    [Fact]
    public void AdminUpdateVideoResponse_ShouldConstructCorrectly()
    {
        // Arrange
        VideoDetailDto dto = CreateVideoDetailDto();

        // Act
        var response = new AdminUpdateVideoResponse(Video: dto);

        // Assert
        response.Should().NotBeNull();
        response.Video.Should().Be(dto);
    }

    [Fact]
    public void AdminUpdateVideoRequest_ShouldConstructCorrectly()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();
        DateTimeOffset featuredUntil = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        var request = new AdminUpdateVideoRequest(
            CategoryId: categoryId,
            Title: "Updated Video Title",
            Slug: "updated-video-slug",
            Description: "Updated description",
            CustomerId: customerId,
            OrderItemId: orderItemId,
            SocialBoost: true,
            IsFeatured: true,
            FeaturedUntil: featuredUntil,
            MetaTitle: "Updated Meta Title",
            MetaDescription: "Updated meta description"
        );

        // Assert
        request.Should().NotBeNull();
        request.CategoryId.Should().Be(categoryId);
        request.Title.Should().Be("Updated Video Title");
        request.Slug.Should().Be("updated-video-slug");
        request.Description.Should().Be("Updated description");
        request.CustomerId.Should().Be(customerId);
        request.OrderItemId.Should().Be(orderItemId);
        request.SocialBoost.Should().BeTrue();
        request.IsFeatured.Should().BeTrue();
        request.FeaturedUntil.Should().Be(featuredUntil);
        request.MetaTitle.Should().Be("Updated Meta Title");
        request.MetaDescription.Should().Be("Updated meta description");
    }

    [Fact]
    public void AdminUpdateVideoRequest_WithNullOptionalFields_ShouldConstructCorrectly()
    {
        // Act
        var request = new AdminUpdateVideoRequest(
            CategoryId: Guid.NewGuid(),
            Title: "Updated Video",
            Slug: "updated-video",
            Description: null,
            CustomerId: null,
            OrderItemId: null,
            SocialBoost: false,
            IsFeatured: false,
            FeaturedUntil: null,
            MetaTitle: null,
            MetaDescription: null
        );

        // Assert
        request.Description.Should().BeNull();
        request.CustomerId.Should().BeNull();
        request.OrderItemId.Should().BeNull();
        request.FeaturedUntil.Should().BeNull();
        request.MetaTitle.Should().BeNull();
        request.MetaDescription.Should().BeNull();
    }

    private static VideoDetailDto CreateVideoDetailDto() =>
        new(
            Id: Guid.NewGuid(),
            CategoryId: Guid.NewGuid(),
            CategoryName: "Test",
            Title: "Test",
            Slug: "test",
            Description: null,
            ThumbnailUrl: null,
            ThumbnailStorageKey: null,
            AuthorId: "Test",
            Status: EnumContentStatus.Draft,
            RejectionReason: null,
            YoutubeVideoId: null,
            IsFeatured: false,
            FeaturedUntil: null,
            HasLyrics: false,
            ShootingScheduledAt: null,
            PublishedAt: null,
            MetaTitle: null,
            MetaDescription: null,
            Tags: []
        );
}
