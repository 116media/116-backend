using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel.V1;

/// <summary>
/// Unit tests for <see cref="AdminCreatePromotionLevelResponse"/>.
/// </summary>
public class AdminCreatePromotionLevelEndpointV1Tests
{
    [Fact]
    public void AdminCreatePromotionLevelResponse_ShouldConstructCorrectly()
    {
        // Arrange
        PromotionLevelDto promotionLevel = CreatePromotionLevelDto();

        // Act
        var response = new AdminCreatePromotionLevelResponse(PromotionLevel: promotionLevel);

        // Assert
        response.PromotionLevel.Should().NotBeNull();
        response.PromotionLevel.Should().Be(promotionLevel);
    }

    private static PromotionLevelDto CreatePromotionLevelDto() => new(Guid.NewGuid(), "Bronze", 30, 9.99m, true);
}
