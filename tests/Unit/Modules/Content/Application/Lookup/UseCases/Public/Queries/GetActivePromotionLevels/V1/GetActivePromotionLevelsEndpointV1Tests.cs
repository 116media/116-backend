using _116.Content.Application.Lookup.UseCases.Public.Queries.GetActivePromotionLevels.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Public.Queries.GetActivePromotionLevels.V1;

/// <summary>
/// Unit tests for <see cref="GetActivePromotionLevelsResponse"/>.
/// </summary>
public class GetActivePromotionLevelsEndpointV1Tests
{
    [Fact]
    public void GetActivePromotionLevelsResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<PromotionLevelDto> promotionLevels = [CreatePromotionLevelDto()];

        // Act
        var response = new GetActivePromotionLevelsResponse(PromotionLevels: promotionLevels);

        // Assert
        response.PromotionLevels.Should().NotBeNull();
        response.PromotionLevels.Should().HaveCount(1);
    }

    private static PromotionLevelDto CreatePromotionLevelDto() => new(Guid.NewGuid(), "Bronze", 30, 9.99m, true);
}
