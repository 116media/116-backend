using _116.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing.V1;

/// <summary>
/// Unit tests for <see cref="AdminRemoveCategoryPricingResponse"/>.
/// </summary>
public class AdminRemoveCategoryPricingEndpointV1Tests
{
    [Fact]
    public void AdminRemoveCategoryPricingResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<CategoryPricingDto> pricing = [CreateCategoryPricingDto()];

        // Act
        var response = new AdminRemoveCategoryPricingResponse(Pricing: pricing, IsSuccess: true);

        // Assert
        response.Pricing.Should().ContainSingle();
        response.IsSuccess.Should().BeTrue();
    }

    private static CategoryPricingDto CreateCategoryPricingDto() => new(Guid.NewGuid(), "Basic", 9.99m);
}
