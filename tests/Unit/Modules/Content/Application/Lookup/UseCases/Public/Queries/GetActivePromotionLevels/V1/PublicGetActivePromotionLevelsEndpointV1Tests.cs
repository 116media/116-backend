using _116.Content.Application.Lookup.UseCases.Public.Queries.GetActivePromotionLevels.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetActivePromotionLevels.V1;

/// <summary>
/// Unit tests for <see cref="PublicGetActivePromotionLevelsResponse"/>.
/// </summary>
public class PublicGetActivePromotionLevelsEndpointV1Tests
{
    [Fact]
    public void PublicGetActivePromotionLevelsResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<PromotionLevelDto> promotionLevels = [CreatePromotionLevelDto()];

        // Act
        var response = new PublicGetActivePromotionLevelsResponse(PromotionLevels: promotionLevels);

        // Assert
        response.PromotionLevels.Should().NotBeNull();
        response.PromotionLevels.Should().ContainSingle();
    }

    private static PromotionLevelDto CreatePromotionLevelDto() => new(Guid.NewGuid(), "Bronze", 30, 9.99m, true);
}
