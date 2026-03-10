using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing.V1;

/// <summary>
/// Unit tests for <see cref="AddCategoryPricingResponse"/>.
/// </summary>
public class AddCategoryPricingEndpointV1Tests
{
    [Fact]
    public void AddCategoryPricingResponse_ShouldConstructCorrectly()
    {
        // Arrange
        CategoryPricingDto pricing = CreateCategoryPricingDto();

        // Act
        var response = new AddCategoryPricingResponse(Pricing: pricing);

        // Assert
        response.Pricing.Should().NotBeNull();
        response.Pricing.Should().Be(pricing);
    }

    private static CategoryPricingDto CreateCategoryPricingDto() => new(Guid.NewGuid(), "Basic", 9.99m);
}
