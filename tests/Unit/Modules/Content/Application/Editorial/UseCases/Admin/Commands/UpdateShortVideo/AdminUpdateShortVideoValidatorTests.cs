using System.Globalization;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo;

/// <summary>
/// Unit tests for <see cref="AdminUpdateShortVideoValidator"/>.
/// </summary>
public class AdminUpdateShortVideoValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminUpdateShortVideoValidator _validator;

    public AdminUpdateShortVideoValidatorTests()
    {
        _validator = new AdminUpdateShortVideoValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateShortVideoCommand(
            Id: Guid.NewGuid().ToString(),
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            VideoId: null,
            VideoFile: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Id Validation Tests

    [Fact]
    public async Task Validate_WithInvalidId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateShortVideoCommand(
            Id: "not-a-guid",
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            VideoId: null,
            VideoFile: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateShortVideoCommand.Id));
    }

    [Fact]
    public async Task Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateShortVideoCommand(
            Id: string.Empty,
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            VideoId: null,
            VideoFile: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateShortVideoCommand.Id));
    }

    #endregion

    #region Title Validation Tests

    [Fact]
    public async Task Validate_WithEmptyTitle_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateShortVideoCommand(
            Id: Guid.NewGuid().ToString(),
            Title: string.Empty,
            VideoId: null,
            VideoFile: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateShortVideoCommand.Title)
                && e.ErrorMessage == _i18n.ShortVideo.Msg.TitleRequired()
            );
    }

    [Fact]
    public async Task Validate_WithTitleExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateShortVideoCommand(
            Id: Guid.NewGuid().ToString(),
            Title: new string('a', TestConstants.Content.Editorial.ShortVideo.TitleMaxLength + 1),
            VideoId: null,
            VideoFile: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateShortVideoCommand.Title)
                && e.ErrorMessage
                    == _i18n.ShortVideo.Msg.TitleTooLong(TestConstants.Content.Editorial.ShortVideo.TitleMaxLength)
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
        Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        var validator = new AdminUpdateShortVideoValidator(_i18n);
        var command = new AdminUpdateShortVideoCommand(
            Id: Guid.NewGuid().ToString(),
            Title: string.Empty,
            VideoId: null,
            VideoFile: null
        );

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateShortVideoCommand.Title)
                && e.ErrorMessage == _i18n.ShortVideo.Msg.TitleRequired()
            );
    }

    #endregion
}
