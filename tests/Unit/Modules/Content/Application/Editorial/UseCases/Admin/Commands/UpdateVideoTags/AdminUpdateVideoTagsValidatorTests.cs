using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags;

/// <summary>
/// Unit tests for <see cref="AdminUpdateVideoTagsValidator"/>.
/// </summary>
public class AdminUpdateVideoTagsValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminUpdateVideoTagsValidator _validator;

    public AdminUpdateVideoTagsValidatorTests()
    {
        _validator = new AdminUpdateVideoTagsValidator(_i18n);
    }

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
                && e.ErrorMessage == _i18n.Video.Msg.Localizer["IdRequired"].Value
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
                && e.ErrorMessage == _i18n.Video.Msg.Localizer["IdInvalid"].Value
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
        result.Errors.Should().Contain(e => e.ErrorMessage == _i18n.Tag.Msg.NameRequired());
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
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains(_i18n.Tag.Msg.NameTooLong(50)));
    }

    #endregion
}
