using _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivateCategory.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.ActivateCategory.V1;

/// <summary>
/// Unit tests for <see cref="ActivateCategoryResponse"/>.
/// </summary>
public class ActivateCategoryEndpointV1Tests
{
    [Fact]
    public void ActivateCategoryResponse_ShouldConstructCorrectly()
    {
        // Arrange
        CategoryDto category = CreateCategoryDto();

        // Act
        var response = new ActivateCategoryResponse(Category: category);

        // Assert
        response.Category.Should().NotBeNull();
        response.Category.Should().Be(category);
    }

    private static CategoryDto CreateCategoryDto() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Article", "Technology", "technology", true, true, []);
}
