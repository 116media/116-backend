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
    private readonly VideoErrorMessage _i18n = LocalizerFactory.CreateMessage<VideoErrorMessage>();

    private readonly AdminAttachYoutubeVideoUrlValidator _validator;

    public AdminAttachYoutubeVideoUrlValidatorTests()
    {
        _validator = new AdminAttachYoutubeVideoUrlValidator(_i18n);
    }

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
                && e.ErrorMessage == _i18n.Localizer["IdRequired"].Value
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
                && e.ErrorMessage == _i18n.Localizer["IdInvalid"].Value
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
                && e.ErrorMessage == _i18n.YoutubeUrlRequired()
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
                    == _i18n.YoutubeUrlTooLong(TestConstants.Content.Editorial.Video.YoutubeVideoUrlMaxLength)
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
                && e.ErrorMessage == _i18n.YoutubeUrlInvalidFormat()
            );
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        var i18n = LocalizerFactory.CreateMessage<VideoErrorMessage>(culture);
        var validator = new AdminAttachYoutubeVideoUrlValidator(i18n);
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: Guid.NewGuid().ToString(),
            YoutubeVideoUrl: string.Empty
        );

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAttachYoutubeVideoUrlCommand.YoutubeVideoUrl)
                && e.ErrorMessage == i18n.YoutubeUrlRequired()
            );
    }

    #endregion
}
