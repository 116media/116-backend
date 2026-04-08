using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory.V1;

/// <summary>
/// Unit tests for <see cref="AdminCreateCategoryResponse"/> and <see cref="AdminCreateCategoryRequest"/>.
/// </summary>
public class AdminCreateCategoryEndpointV1Tests
{
    [Fact]
    public void AdminCreateCategoryResponse_ShouldConstructCorrectly()
    {
        // Arrange
        CategoryDto category = CreateCategoryDto();

        // Act
        var response = new AdminCreateCategoryResponse(Category: category);

        // Assert
        response.Category.Should().NotBeNull();
        response.Category.Should().Be(category);
    }

    [Fact]
    public void AdminCreateCategoryRequest_WithDescription_ShouldConstructCorrectly()
    {
        // Arrange
        var contentTypeId = Guid.NewGuid();

        // Act
        var request = new AdminCreateCategoryRequest(
            Name: "Technology",
            Slug: "technology",
            Description: "Tech articles",
            IsFree: true
        );

        // Assert
        request.Name.Should().Be("Technology");
        request.IsFree.Should().BeTrue();
    }

    [Fact]
    public void AdminCreateCategoryRequest_WithDescription_ShouldConstructCorrectly_Alt()
    {
        // Arrange
        var contentTypeId = Guid.NewGuid();

        // Act
        var request = new AdminCreateCategoryRequest(
            Name: "Technology",
            Slug: "technology",
            Description: "Test category description",
            IsFree: false
        );

        // Assert
        request.Description.Should().Be("Test category description");
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
