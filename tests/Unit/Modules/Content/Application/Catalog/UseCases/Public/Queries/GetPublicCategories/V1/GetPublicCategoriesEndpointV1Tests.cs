using _116.Content.Application.Catalog.UseCases.Public.Queries.GetPublicCategories.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Public.Queries.GetPublicCategories.V1;

/// <summary>
/// Unit tests for <see cref="GetPublicCategoriesResponse"/>.
/// </summary>
public class GetPublicCategoriesEndpointV1Tests
{
    [Fact]
    public void GetPublicCategoriesResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<CategoryDto> categories = [CreateCategoryDto()];

        // Act
        var response = new GetPublicCategoriesResponse(Categories: categories);

        // Assert
        response.Categories.Should().NotBeNull();
        response.Categories.Should().HaveCount(1);
    }

    private static CategoryDto CreateCategoryDto() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Article", "Technology", "technology", true, true, []);
}
