using _116.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster.V1;

/// <summary>
/// Unit tests for <see cref="AdminUploadCategoryPosterResponse"/>.
/// </summary>
public class AdminUploadCategoryPosterEndpointV1Tests
{
    [Fact]
    public void AdminUploadCategoryPosterResponse_ShouldConstructCorrectly()
    {
        // Arrange
        CategoryDto category = CreateCategoryDto();

        // Act
        var response = new AdminUploadCategoryPosterResponse(Category: category);

        // Assert
        response.Category.Should().NotBeNull();
        response.Category.Should().Be(category);
    }

    [Fact]
    public void AdminUploadCategoryPosterResponse_WithPosterUrl_ShouldReturnUrl()
    {
        // Arrange
        CategoryDto category = CreateCategoryDtoWithPoster();

        // Act
        var response = new AdminUploadCategoryPosterResponse(Category: category);

        // Assert
        response.Category.PosterUrl.Should().NotBeNullOrEmpty();
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

    private static CategoryDto CreateCategoryDtoWithPoster() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Video",
            "Show Name",
            "show-name",
            "A show with a poster",
            false,
            true,
            false,
            false,
            "https://cloudinary.com/poster.jpg",
            []
        );
}
