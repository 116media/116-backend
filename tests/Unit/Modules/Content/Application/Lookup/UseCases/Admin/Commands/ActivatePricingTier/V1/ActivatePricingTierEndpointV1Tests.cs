using _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePricingTier.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePricingTier.V1;

/// <summary>
/// Unit tests for <see cref="ActivatePricingTierResponse"/>.
/// </summary>
public class ActivatePricingTierEndpointV1Tests
{
    [Fact]
    public void ActivatePricingTierResponse_ShouldConstructCorrectly()
    {
        // Arrange
        PricingTierDto pricingTier = CreatePricingTierDto();

        // Act
        var response = new ActivatePricingTierResponse(PricingTier: pricingTier);

        // Assert
        response.PricingTier.Should().NotBeNull();
        response.PricingTier.Should().Be(pricingTier);
    }

    private static PricingTierDto CreatePricingTierDto() => new(Guid.NewGuid(), "Basic", "Basic tier", true);
}
