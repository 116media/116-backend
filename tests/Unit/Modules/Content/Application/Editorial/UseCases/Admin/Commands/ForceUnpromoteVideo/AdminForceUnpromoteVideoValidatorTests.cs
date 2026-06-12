using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo;

/// <summary>
/// Unit tests for <see cref="AdminForceUnpromoteVideoValidator"/>.
/// </summary>
public class AdminForceUnpromoteVideoValidatorTests
{
    private readonly ArticleErrorMessage _articleI18n = LocalizerFactory.CreateMessage<ArticleErrorMessage>();
    private readonly VideoErrorMessage _videoI18n = LocalizerFactory.CreateMessage<VideoErrorMessage>();

    private readonly AdminForceUnpromoteVideoValidator _validator;

    public AdminForceUnpromoteVideoValidatorTests()
    {
        _validator = new AdminForceUnpromoteVideoValidator(_articleI18n, _videoI18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminForceUnpromoteVideoCommand(
            Slug: "my-video-slug",
            Reason: "Government takedown request."
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Slug Validation Tests

    [Fact]
    public async Task Validate_WithEmptySlug_ShouldHaveError()
    {
        // Arrange
        var command = new AdminForceUnpromoteVideoCommand(Slug: string.Empty, Reason: "Government takedown request.");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminForceUnpromoteVideoCommand.Slug)
                && e.ErrorMessage == _videoI18n.SlugRequired()
            );
    }

    #endregion

    #region Reason Validation Tests

    [Fact]
    public async Task Validate_WithEmptyReason_ShouldHaveError()
    {
        // Arrange
        var command = new AdminForceUnpromoteVideoCommand(Slug: "my-video-slug", Reason: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminForceUnpromoteVideoCommand.Reason)
                && e.ErrorMessage == _articleI18n.RejectionReasonRequired()
            );
    }

    [Fact]
    public async Task Validate_WithReasonExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminForceUnpromoteVideoCommand(Slug: "my-video-slug", Reason: new string('a', 501));

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminForceUnpromoteVideoCommand.Reason)
                && e.ErrorMessage == _articleI18n.RejectionReasonTooLong(500)
            );
    }

    [Fact]
    public async Task Validate_WithReasonAtMaxLength_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminForceUnpromoteVideoCommand(Slug: "my-video-slug", Reason: new string('a', 500));

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        var articleI18n = LocalizerFactory.CreateMessage<ArticleErrorMessage>(culture);
        var videoI18n = LocalizerFactory.CreateMessage<VideoErrorMessage>(culture);
        var validator = new AdminForceUnpromoteVideoValidator(articleI18n, videoI18n);
        var command = new AdminForceUnpromoteVideoCommand(Slug: "my-video-slug", Reason: string.Empty);

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminForceUnpromoteVideoCommand.Reason)
                && e.ErrorMessage == articleI18n.RejectionReasonRequired()
            );
    }

    #endregion
}
