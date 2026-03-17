using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.V1;

/// <summary>
/// Unit tests for <see cref="AdminAddItemTierRequest"/> and <see cref="AdminAddItemTierResponse"/>.
/// </summary>
public class AdminAddItemTierEndpointV1Tests
{
    [Fact]
    public void AdminAddItemTierResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var tier = new ItemTierDto("Standard", TestConstants.Content.Commerce.ValidTierPriceUsd);

        // Act
        var response = new AdminAddItemTierResponse(Tier: tier);

        // Assert
        response.Tier.Should().NotBeNull();
        response.Tier.Should().Be(tier);
    }

    [Fact]
    public void AdminAddItemTierRequest_ShouldConstructCorrectly()
    {
        // Arrange
        string pricingTierId = Guid.NewGuid().ToString();

        // Act
        var request = new AdminAddItemTierRequest(PricingTierId: pricingTierId);

        // Assert
        request.PricingTierId.Should().Be(pricingTierId);
    }
}
