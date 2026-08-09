using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;

/// <summary>
/// Unit tests for <see cref="AdminCreateShortVideoValidator"/>.
/// </summary>
public class AdminCreateShortVideoValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminCreateShortVideoValidator _validator;

    public AdminCreateShortVideoValidatorTests()
    {
        _validator = new AdminCreateShortVideoValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.ShortVideo.ValidTitle,
            Slug: TestConstants.ShortVideo.ValidSlug,
            AuthorId: Guid.NewGuid(),
            VideoId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Title Validation Tests

    [Fact]
    public async Task Validate_WithEmptyTitle_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateShortVideoCommand(
            Title: string.Empty,
            Slug: TestConstants.ShortVideo.ValidSlug,
            AuthorId: Guid.NewGuid(),
            VideoId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateShortVideoCommand.Title)
                && e.ErrorMessage == _i18n.ShortVideo.Msg.TitleRequired()
            );
    }

    [Fact]
    public async Task Validate_WithTitleExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateShortVideoCommand(
            Title: new string('a', TestConstants.ShortVideo.TitleMaxLength + 1),
            Slug: TestConstants.ShortVideo.ValidSlug,
            AuthorId: Guid.NewGuid(),
            VideoId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateShortVideoCommand.Title)
                && e.ErrorMessage == _i18n.ShortVideo.Msg.TitleTooLong(TestConstants.ShortVideo.TitleMaxLength)
            );
    }

    #endregion

    #region Slug Validation Tests

    [Fact]
    public async Task Validate_WithEmptySlug_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.ShortVideo.ValidTitle,
            Slug: string.Empty,
            AuthorId: Guid.NewGuid(),
            VideoId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateShortVideoCommand.Slug)
                && e.ErrorMessage == _i18n.ShortVideo.Msg.SlugRequired()
            );
    }

    [Fact]
    public async Task Validate_WithUppercaseSlug_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.ShortVideo.ValidTitle,
            Slug: "Invalid-Slug",
            AuthorId: Guid.NewGuid(),
            VideoId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateShortVideoCommand.Slug)
                && e.ErrorMessage == _i18n.ShortVideo.Msg.SlugInvalidFormat()
            );
    }

    #endregion
}
