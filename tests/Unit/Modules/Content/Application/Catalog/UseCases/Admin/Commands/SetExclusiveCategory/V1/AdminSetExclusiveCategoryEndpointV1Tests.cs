using _116.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory.V1;

/// <summary>
/// Unit tests for <see cref="AdminSetExclusiveCategoryResponse"/>.
/// </summary>
public class AdminSetExclusiveCategoryEndpointV1Tests
{
    [Fact]
    public void AdminSetExclusiveCategoryResponse_ShouldConstructCorrectly()
    {
        // Arrange
        CategoryDto category = CreateCategoryDto();

        // Act
        var response = new AdminSetExclusiveCategoryResponse(Category: category);

        // Assert
        response.Category.Should().NotBeNull();
        response.Category.Should().Be(category);
    }

    [Fact]
    public void AdminSetExclusiveCategoryResponse_CategoryShouldBeExclusive()
    {
        // Arrange
        CategoryDto category = CreateExclusiveCategoryDto();

        // Act
        var response = new AdminSetExclusiveCategoryResponse(Category: category);

        // Assert
        response.Category.IsExclusive.Should().BeTrue();
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
            false,
            false,
            null,
            []
        );

    private static CategoryDto CreateExclusiveCategoryDto() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Video",
            "Exclusive Show",
            "exclusive-show",
            "The exclusive show",
            false,
            true,
            false,
            true,
            "https://cloudinary.com/poster.jpg",
            []
        );
}
