using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;

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
        var command = new AdminCreateShortVideoCommand(Title: string.Empty, AuthorId: Guid.NewGuid(), VideoId: null);

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
}
