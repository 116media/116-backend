using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem.V1;

/// <summary>
/// Unit tests for <see cref="AdminEditOrderItemRequest"/> and <see cref="AdminEditOrderItemResponse"/>.
/// </summary>
public class AdminEditOrderItemEndpointV1Tests
{
    [Fact]
    public void AdminEditOrderItemRequest_ShouldConstructCorrectly()
    {
        // Arrange & Act
        var request = new AdminEditOrderItemRequest(
            ContentKind: EnumCoreContentType.Video,
            CategoryId: Guid.NewGuid().ToString(),
            PromotionLevelId: Guid.NewGuid(),
            SocialBoost: true,
            IsBonus: false
        );

        // Assert
        request.ContentKind.Should().Be(EnumCoreContentType.Video);
        request.SocialBoost.Should().BeTrue();
        request.IsBonus.Should().BeFalse();
    }

    [Fact]
    public void AdminEditOrderItemRequest_WithAllNulls_ShouldConstructCorrectly()
    {
        // Arrange & Act
        var request = new AdminEditOrderItemRequest(
            ContentKind: null,
            CategoryId: null,
            PromotionLevelId: null,
            SocialBoost: null,
            IsBonus: null
        );

        // Assert
        request.ContentKind.Should().BeNull();
        request.CategoryId.Should().BeNull();
        request.PromotionLevelId.Should().BeNull();
        request.SocialBoost.Should().BeNull();
        request.IsBonus.Should().BeNull();
    }

    [Fact]
    public void AdminEditOrderItemResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var item = new OrderItemDto(
            Id: Guid.NewGuid(),
            ContentKind: EnumCoreContentType.Article,
            CategoryName: "Test",
            PromotionLevelName: null,
            PromoPriceUsd: null,
            SocialBoost: false,
            IsBonus: false,
            Tiers: []
        );

        // Act
        var response = new AdminEditOrderItemResponse(Item: item);

        // Assert
        response.Item.Should().NotBeNull();
        response.Item.Should().Be(item);
    }
}
