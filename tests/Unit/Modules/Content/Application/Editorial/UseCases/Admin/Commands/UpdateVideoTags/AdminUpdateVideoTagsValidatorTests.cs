using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags;

/// <summary>
/// Unit tests for <see cref="AdminUpdateVideoTagsValidator"/>.
/// </summary>
public class AdminUpdateVideoTagsValidatorTests
{
    private readonly AdminUpdateVideoTagsValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidTagNames_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateVideoTagsCommand(
            VideoId: Guid.NewGuid().ToString(),
            TagNames: new List<string> { "Fally Ipupa", "Kinshasa" }
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithEmptyTagNames_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateVideoTagsCommand(VideoId: Guid.NewGuid().ToString(), TagNames: new List<string>());

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

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
        var command = new AdminUpdateVideoTagsCommand(VideoId: string.Empty, TagNames: new List<string>());

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateVideoTagsCommand.VideoId)
                && e.ErrorMessage == "Video ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidVideoId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateVideoTagsCommand(VideoId: "not-a-guid", TagNames: new List<string>());

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateVideoTagsCommand.VideoId)
                && e.ErrorMessage == "Video ID is invalid."
            );
    }

    #endregion

    #region TagNames Validation Tests

    [Fact]
    public async Task Validate_WithEmptyTagName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateVideoTagsCommand(
            VideoId: Guid.NewGuid().ToString(),
            TagNames: new List<string> { string.Empty }
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Tag name is required.");
    }

    [Fact]
    public async Task Validate_WithTagNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateVideoTagsCommand(
            VideoId: Guid.NewGuid().ToString(),
            TagNames: new List<string> { new string('a', 51) }
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Tag name must not exceed 50 characters"));
    }

    #endregion
}
