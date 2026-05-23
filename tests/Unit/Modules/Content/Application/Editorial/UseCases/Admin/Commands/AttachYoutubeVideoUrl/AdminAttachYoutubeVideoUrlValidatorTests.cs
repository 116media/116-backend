using _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl;

/// <summary>
/// Unit tests for <see cref="AdminAttachYoutubeVideoUrlValidator"/>.
/// </summary>
public class AdminAttachYoutubeVideoUrlValidatorTests
{
    private readonly AdminAttachYoutubeVideoUrlValidator _validator = new(
        LocalizerFactory.CreateMessage<VideoErrorMessage>()
    );

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: Guid.NewGuid().ToString(),
            YoutubeVideoUrl: TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl
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
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: string.Empty,
            YoutubeVideoUrl: TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAttachYoutubeVideoUrlCommand.VideoId)
                && e.ErrorMessage == "Video ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidVideoId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: "not-a-guid",
            YoutubeVideoUrl: TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAttachYoutubeVideoUrlCommand.VideoId)
                && e.ErrorMessage == "Video ID is invalid."
            );
    }

    #endregion

    #region YoutubeVideoUrl Validation Tests

    [Fact]
    public async Task Validate_WithEmptyYoutubeVideoUrl_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAttachYoutubeVideoUrlCommand(
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
                e.PropertyName == nameof(AdminAttachYoutubeVideoUrlCommand.YoutubeVideoUrl)
                && e.ErrorMessage == "YouTube video URL is required."
            );
    }

    [Fact]
    public async Task Validate_WithYoutubeVideoUrlExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: Guid.NewGuid().ToString(),
            YoutubeVideoUrl: new string('a', TestConstants.Content.Editorial.Video.YoutubeVideoUrlMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAttachYoutubeVideoUrlCommand.YoutubeVideoUrl)
                && e.ErrorMessage
                    == $"YouTube video URL must not exceed {TestConstants.Content.Editorial.Video.YoutubeVideoUrlMaxLength} characters."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidYoutubeUrl_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAttachYoutubeVideoUrlCommand(
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
                e.PropertyName == nameof(AdminAttachYoutubeVideoUrlCommand.YoutubeVideoUrl)
                && e.ErrorMessage == "Must be a valid YouTube video URL."
            );
    }

    #endregion
}
