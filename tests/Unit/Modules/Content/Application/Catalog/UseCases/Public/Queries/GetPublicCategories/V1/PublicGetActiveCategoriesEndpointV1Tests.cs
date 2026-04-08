using _116.Content.Application.Catalog.UseCases.Public.Queries.GetActiveCategories.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Public.Queries.GetPublicCategories.V1;

/// <summary>
/// Unit tests for <see cref="PublicGetActiveCategoriesResponse"/>.
/// </summary>
public class PublicGetActiveCategoriesEndpointV1Tests
{
    [Fact]
    public void PublicGetActiveCategoriesResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<CategoryDto> categories = [CreateCategoryDto()];

        // Act
        var response = new PublicGetActiveCategoriesResponse(Categories: categories);

        // Assert
        response.Categories.Should().NotBeNull();
        response.Categories.Should().ContainSingle();
    }

    private static CategoryDto CreateCategoryDto() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Article",
            "Technology",
            "technology",
            "Test category description",
            true,
            true,
            []
        );
}
