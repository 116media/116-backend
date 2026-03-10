using _116.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing.V1;

/// <summary>
/// Unit tests for <see cref="RemoveCategoryPricingResponse"/>.
/// </summary>
public class RemoveCategoryPricingEndpointV1Tests
{
    [Fact]
    public void RemoveCategoryPricingResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<CategoryPricingDto> pricing = [CreateCategoryPricingDto()];

        // Act
        var response = new RemoveCategoryPricingResponse(Pricing: pricing, IsSuccess: true);

        // Assert
        response.Pricing.Should().HaveCount(1);
        response.IsSuccess.Should().BeTrue();
    }

    private static CategoryPricingDto CreateCategoryPricingDto() => new(Guid.NewGuid(), "Basic", 9.99m);
}
