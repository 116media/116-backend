using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier.V1;

/// <summary>
/// Unit tests for <see cref="AdminUpdatePricingTierResponse"/>.
/// </summary>
public class AdminUpdatePricingTierEndpointV1Tests
{
    [Fact]
    public void AdminUpdatePricingTierResponse_ShouldConstructCorrectly()
    {
        // Arrange
        PricingTierDto pricingTier = CreatePricingTierDto();

        // Act
        var response = new AdminUpdatePricingTierResponse(PricingTier: pricingTier);

        // Assert
        response.PricingTier.Should().NotBeNull();
        response.PricingTier.Should().Be(pricingTier);
    }

    private static PricingTierDto CreatePricingTierDto() => new(Guid.NewGuid(), "Basic", "Basic tier", true);
}
