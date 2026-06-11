using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo;
using _116.Content.Application.Shared.Errors.Messages;
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
    private readonly AdminUpdateShortVideoValidator _validator = new(
        LocalizerFactory.CreateMessage<ShortVideoErrorMessage>()
    );

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
                && e.ErrorMessage == "Short video title is required."
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
                && e.ErrorMessage == "Short video title must not exceed 200 characters."
            );
    }

    #endregion
}
