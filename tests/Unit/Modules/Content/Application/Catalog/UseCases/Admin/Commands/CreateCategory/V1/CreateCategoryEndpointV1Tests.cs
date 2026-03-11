using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory.V1;

/// <summary>
/// Unit tests for <see cref="CreateCategoryResponse"/> and <see cref="CreateCategoryRequest"/>.
/// </summary>
public class CreateCategoryEndpointV1Tests
{
    [Fact]
    public void CreateCategoryResponse_ShouldConstructCorrectly()
    {
        // Arrange
        CategoryDto category = CreateCategoryDto();

        // Act
        var response = new CreateCategoryResponse(Category: category);

        // Assert
        response.Category.Should().NotBeNull();
        response.Category.Should().Be(category);
    }

    [Fact]
    public void CreateCategoryRequest_WithDescription_ShouldConstructCorrectly()
    {
        // Arrange
        var contentTypeId = Guid.NewGuid();

        // Act
        var request = new CreateCategoryRequest(
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
    public void CreateCategoryRequest_WithNullDescription_ShouldConstructCorrectly()
    {
        // Arrange
        var contentTypeId = Guid.NewGuid();

        // Act
        var request = new CreateCategoryRequest(
            Name: "Technology",
            Slug: "technology",
            Description: null,
            IsFree: false
        );

        // Assert
        request.Description.Should().BeNull();
    }

    private static CategoryDto CreateCategoryDto() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Article", "Technology", "technology", true, true, []);
}
