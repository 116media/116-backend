using _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl;
using _116.Content.Application.Shared.Errors.Facade;
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
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

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
            YoutubeVideoUrl: TestConstants.Video.ValidYoutubeVideoUrl
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
            YoutubeVideoUrl: TestConstants.Video.ValidYoutubeVideoUrl
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAttachYoutubeVideoUrlCommand.VideoId)
                && e.ErrorMessage == _i18n.Video.Msg.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidVideoId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: "not-a-guid",
            YoutubeVideoUrl: TestConstants.Video.ValidYoutubeVideoUrl
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAttachYoutubeVideoUrlCommand.VideoId)
                && e.ErrorMessage == _i18n.Video.Msg.Localizer["IdInvalid"].Value
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
                && e.ErrorMessage == _i18n.Video.Msg.YoutubeUrlRequired()
            );
    }

    [Fact]
    public async Task Validate_WithYoutubeVideoUrlExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: Guid.NewGuid().ToString(),
            YoutubeVideoUrl: new string('a', TestConstants.Video.YoutubeVideoUrlMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAttachYoutubeVideoUrlCommand.YoutubeVideoUrl)
                && e.ErrorMessage == _i18n.Video.Msg.YoutubeUrlTooLong(TestConstants.Video.YoutubeVideoUrlMaxLength)
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
                && e.ErrorMessage == _i18n.Video.Msg.YoutubeUrlInvalidFormat()
            );
    }

    #endregion
}
