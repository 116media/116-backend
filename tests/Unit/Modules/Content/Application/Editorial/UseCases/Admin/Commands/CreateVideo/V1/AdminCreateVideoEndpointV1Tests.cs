using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo.V1;

/// <summary>
/// Unit tests for <see cref="AdminCreateVideoResponse"/>.
/// </summary>
public class AdminCreateVideoEndpointV1Tests
{
    [Fact]
    public void AdminCreateVideoResponse_ShouldConstructCorrectly()
    {
        // Arrange
        VideoDetailDto dto = CreateVideoDetailDto();

        // Act
        var response = new AdminCreateVideoResponse(Video: dto);

        // Assert
        response.Should().NotBeNull();
        response.Video.Should().Be(dto);
    }

    [Fact]
    public void AdminCreateVideoRequest_ShouldConstructCorrectly()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();
        DateTimeOffset scheduledAt = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        var request = new AdminCreateVideoRequest(
            CategoryId: categoryId,
            Title: "Test Video",
            Slug: "test-video",
            CustomerId: customerId,
            OrderItemId: orderItemId,
            Description: "Test description",
            ShootingScheduledAt: scheduledAt
        );

        // Assert
        request.Should().NotBeNull();
        request.CategoryId.Should().Be(categoryId);
        request.Title.Should().Be("Test Video");
        request.Slug.Should().Be("test-video");
        request.CustomerId.Should().Be(customerId);
        request.OrderItemId.Should().Be(orderItemId);
        request.Description.Should().Be("Test description");
        request.ShootingScheduledAt.Should().Be(scheduledAt);
    }

    [Fact]
    public void AdminCreateVideoRequest_WithNullOptionalFields_ShouldConstructCorrectly()
    {
        // Act
        var request = new AdminCreateVideoRequest(
            CategoryId: Guid.NewGuid(),
            Title: "Test Video",
            Slug: "test-video",
            CustomerId: null,
            OrderItemId: null,
            Description: null,
            ShootingScheduledAt: null
        );

        // Assert
        request.CustomerId.Should().BeNull();
        request.OrderItemId.Should().BeNull();
        request.Description.Should().BeNull();
        request.ShootingScheduledAt.Should().BeNull();
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
