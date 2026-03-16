using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;

/// <summary>
/// Unit tests for <see cref="AdminCreateShortVideoValidator"/>.
/// </summary>
public class AdminCreateShortVideoValidatorTests
{
    private readonly AdminCreateShortVideoValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            Slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            VideoFile: fileMock.Object,
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
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        var command = new AdminCreateShortVideoCommand(
            Title: string.Empty,
            Slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            VideoFile: fileMock.Object,
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
                && e.ErrorMessage == "Short video title is required."
            );
    }

    [Fact]
    public async Task Validate_WithTitleExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        var command = new AdminCreateShortVideoCommand(
            Title: new string('a', TestConstants.Content.Editorial.ShortVideo.TitleMaxLength + 1),
            Slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            VideoFile: fileMock.Object,
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
                && e.ErrorMessage == "Short video title must not exceed 200 characters."
            );
    }

    #endregion

    #region Slug Validation Tests

    [Fact]
    public async Task Validate_WithEmptySlug_ShouldHaveError()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            Slug: string.Empty,
            VideoFile: fileMock.Object,
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
                && e.ErrorMessage == "Short video slug is required."
            );
    }

    [Fact]
    public async Task Validate_WithUppercaseSlug_ShouldHaveError()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            Slug: "Invalid-Slug",
            VideoFile: fileMock.Object,
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
                && e.ErrorMessage
                    == "Short video slug must be lowercase and contain only letters, numbers, and hyphens."
            );
    }

    #endregion

    #region VideoFile Validation Tests

    [Fact]
    public async Task Validate_WithNullVideoFile_ShouldHaveError()
    {
        // Arrange
        IFormFile? file = null;
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            Slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            VideoFile: file!,
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
                e.PropertyName == nameof(AdminCreateShortVideoCommand.VideoFile)
                && e.ErrorMessage == "Short video file is required."
            );
    }

    #endregion
}
