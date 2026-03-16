using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing.V1;

/// <summary>
/// Unit tests for <see cref="AdminAddCategoryPricingResponse"/>.
/// </summary>
public class AdminAddCategoryPricingEndpointV1Tests
{
    [Fact]
    public void AdminAddCategoryPricingResponse_ShouldConstructCorrectly()
    {
        // Arrange
        CategoryPricingDto pricing = CreateCategoryPricingDto();

        // Act
        var response = new AdminAddCategoryPricingResponse(Pricing: pricing);

        // Assert
        response.Pricing.Should().NotBeNull();
        response.Pricing.Should().Be(pricing);
    }

    private static CategoryPricingDto CreateCategoryPricingDto() => new(Guid.NewGuid(), "Basic", 9.99m);
}
