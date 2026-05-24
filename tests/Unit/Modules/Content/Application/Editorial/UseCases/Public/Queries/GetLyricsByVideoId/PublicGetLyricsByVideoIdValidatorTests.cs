using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;

/// <summary>
/// Unit tests for <see cref="PublicGetLyricsByVideoIdValidator"/>.
/// </summary>
public class PublicGetLyricsByVideoIdValidatorTests
{
    private readonly PublicGetLyricsByVideoIdValidator _validator = new();

    #region Valid Query Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var query = new PublicGetLyricsByVideoIdQuery(VideoId: Guid.NewGuid().ToString());

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region VideoId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyVideoId_ShouldHaveError()
    {
        // Arrange
        var query = new PublicGetLyricsByVideoIdQuery(VideoId: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(PublicGetLyricsByVideoIdQuery.VideoId)
                && e.ErrorMessage == "Video ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidVideoId_ShouldHaveError()
    {
        // Arrange
        var query = new PublicGetLyricsByVideoIdQuery(VideoId: "not-a-guid");

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(PublicGetLyricsByVideoIdQuery.VideoId)
                && e.ErrorMessage == "Video ID is invalid."
            );
    }

    #endregion
}
