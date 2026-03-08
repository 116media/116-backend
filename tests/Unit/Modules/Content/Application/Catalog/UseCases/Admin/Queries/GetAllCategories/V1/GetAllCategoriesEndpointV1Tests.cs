using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCategories.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCategories.V1;

/// <summary>
/// Unit tests for <see cref="GetAllCategoriesResponse"/>.
/// </summary>
public class GetAllCategoriesEndpointV1Tests
{
    [Fact]
    public void GetAllCategoriesResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var paginated = new PaginatedResult<CategoryDto>(1, 10, 1, [CreateCategoryDto()]);

        // Act
        var response = new GetAllCategoriesResponse(Categories: paginated);

        // Assert
        response.Should().NotBeNull();
        response.Categories.Should().Be(paginated);
    }

    private static CategoryDto CreateCategoryDto() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Article", "Technology", "technology", true, true, []);
}
