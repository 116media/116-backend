using _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetAllPricingTiersResponse"/>.
/// </summary>
public class AdminGetAllPricingTiersEndpointV1Tests
{
    [Fact]
    public void AdminGetAllPricingTiersResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<PricingTierDto> pricingTiers = [CreatePricingTierDto()];

        // Act
        var response = new AdminGetAllPricingTiersResponse(PricingTiers: pricingTiers);

        // Assert
        response.PricingTiers.Should().NotBeNull();
        response.PricingTiers.Should().ContainSingle();
    }

    private static PricingTierDto CreatePricingTierDto() => new(Guid.NewGuid(), "Basic", "Basic tier", true);
}
