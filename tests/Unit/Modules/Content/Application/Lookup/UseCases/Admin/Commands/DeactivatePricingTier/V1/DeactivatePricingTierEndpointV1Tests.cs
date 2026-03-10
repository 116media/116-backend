using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier.V1;

/// <summary>
/// Unit tests for <see cref="DeactivatePricingTierResponse"/>.
/// </summary>
public class DeactivatePricingTierEndpointV1Tests
{
    [Fact]
    public void DeactivatePricingTierResponse_ShouldConstructCorrectly()
    {
        // Arrange
        PricingTierDto pricingTier = CreatePricingTierDto();

        // Act
        var response = new DeactivatePricingTierResponse(PricingTier: pricingTier);

        // Assert
        response.PricingTier.Should().NotBeNull();
        response.PricingTier.Should().Be(pricingTier);
    }

    private static PricingTierDto CreatePricingTierDto() => new(Guid.NewGuid(), "Basic", "Basic tier", true);
}
