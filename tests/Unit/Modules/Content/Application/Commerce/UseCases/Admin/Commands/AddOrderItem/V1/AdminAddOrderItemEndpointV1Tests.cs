using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.V1;

/// <summary>
/// Unit tests for <see cref="AdminAddOrderItemRequest"/> and <see cref="AdminAddOrderItemResponse"/>.
/// </summary>
public class AdminAddOrderItemEndpointV1Tests
{
    [Fact]
    public void AdminAddOrderItemResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var item = new OrderItemDto(Guid.NewGuid(), "Article", "Technology", null, null, false, false, []);

        // Act
        var response = new AdminAddOrderItemResponse(Item: item);

        // Assert
        response.Item.Should().NotBeNull();
        response.Item.Should().Be(item);
    }

    [Fact]
    public void AdminAddOrderItemRequest_ShouldConstructCorrectly()
    {
        // Act
        var request = new AdminAddOrderItemRequest(
            ContentKind: EnumCoreContentType.Article,
            CategoryId: Guid.NewGuid().ToString(),
            PromotionLevelId: null,
            SocialBoost: false,
            IsBonus: false
        );

        // Assert
        request.ContentKind.Should().Be(EnumCoreContentType.Article);
        request.PromotionLevelId.Should().BeNull();
    }
}
