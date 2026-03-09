using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers.V1;

/// <summary>
/// Unit tests for <see cref="GetAllPricingTiersResponse"/>.
/// </summary>
public class GetAllPricingTiersEndpointV1Tests
{
    [Fact]
    public void GetAllPricingTiersResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<PricingTierDto> pricingTiers = [CreatePricingTierDto()];

        // Act
        var response = new GetAllPricingTiersResponse(PricingTiers: pricingTiers);

        // Assert
        response.PricingTiers.Should().NotBeNull();
        response.PricingTiers.Should().HaveCount(1);
    }

    private static PricingTierDto CreatePricingTierDto() => new(Guid.NewGuid(), "Basic", "Basic tier", true);
}
