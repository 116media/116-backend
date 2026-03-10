using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels.V1;

/// <summary>
/// Unit tests for <see cref="GetAllPromotionLevelsResponse"/>.
/// </summary>
public class GetAllPromotionLevelsEndpointV1Tests
{
    [Fact]
    public void GetAllPromotionLevelsResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<PromotionLevelDto> promotionLevels = [CreatePromotionLevelDto()];

        // Act
        var response = new GetAllPromotionLevelsResponse(PromotionLevels: promotionLevels);

        // Assert
        response.PromotionLevels.Should().NotBeNull();
        response.PromotionLevels.Should().HaveCount(1);
    }

    private static PromotionLevelDto CreatePromotionLevelDto() => new(Guid.NewGuid(), "Bronze", 30, 9.99m, true);
}
