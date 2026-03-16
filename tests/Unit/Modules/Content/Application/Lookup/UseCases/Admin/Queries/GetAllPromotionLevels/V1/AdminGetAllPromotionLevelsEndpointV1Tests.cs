using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetAllPromotionLevelsResponse"/>.
/// </summary>
public class AdminGetAllPromotionLevelsEndpointV1Tests
{
    [Fact]
    public void AdminGetAllPromotionLevelsResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<PromotionLevelDto> promotionLevels = [CreatePromotionLevelDto()];

        // Act
        var response = new AdminGetAllPromotionLevelsResponse(PromotionLevels: promotionLevels);

        // Assert
        response.PromotionLevels.Should().NotBeNull();
        response.PromotionLevels.Should().ContainSingle();
    }

    private static PromotionLevelDto CreatePromotionLevelDto() => new(Guid.NewGuid(), "Bronze", 30, 9.99m, true);
}
