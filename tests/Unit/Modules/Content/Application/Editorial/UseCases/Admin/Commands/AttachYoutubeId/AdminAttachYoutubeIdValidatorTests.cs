using _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeId;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeId;

/// <summary>
/// Unit tests for <see cref="AdminAttachYoutubeIdValidator"/>.
/// </summary>
public class AdminAttachYoutubeIdValidatorTests
{
    private readonly AdminAttachYoutubeIdValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminAttachYoutubeIdCommand(
            VideoId: Guid.NewGuid().ToString(),
            YoutubeVideoUrl: TestConstants.Content.Editorial.Video.ValidYoutubeVideoId
        );

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
        var command = new AdminAttachYoutubeIdCommand(
            VideoId: string.Empty,
            YoutubeVideoUrl: TestConstants.Content.Editorial.Video.ValidYoutubeVideoId
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAttachYoutubeIdCommand.VideoId)
                && e.ErrorMessage == "Video ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidVideoId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAttachYoutubeIdCommand(
            VideoId: "not-a-guid",
            YoutubeVideoUrl: TestConstants.Content.Editorial.Video.ValidYoutubeVideoId
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAttachYoutubeIdCommand.VideoId)
                && e.ErrorMessage == "Video ID is invalid."
            );
    }

    #endregion

    #region YoutubeVideoId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyYoutubeVideoId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAttachYoutubeIdCommand(
            VideoId: Guid.NewGuid().ToString(),
            YoutubeVideoUrl: string.Empty
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAttachYoutubeIdCommand.YoutubeVideoUrl)
                && e.ErrorMessage == "YouTube video URL is required."
            );
    }

    [Fact]
    public async Task Validate_WithYoutubeVideoIdExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAttachYoutubeIdCommand(
            VideoId: Guid.NewGuid().ToString(),
            YoutubeVideoUrl: new string('a', TestConstants.Content.Editorial.Video.YoutubeVideoIdMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAttachYoutubeIdCommand.YoutubeVideoUrl)
                && e.ErrorMessage
                    == $"YouTube video URL must not exceed {TestConstants.Content.Editorial.Video.YoutubeVideoIdMaxLength} characters."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidYoutubeUrl_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAttachYoutubeIdCommand(
            VideoId: Guid.NewGuid().ToString(),
            YoutubeVideoUrl: "https://www.vimeo.com/watch?v=dQw4w9WgXcQ"
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAttachYoutubeIdCommand.YoutubeVideoUrl)
                && e.ErrorMessage == "Must be a valid YouTube video URL."
            );
    }

    #endregion
}
