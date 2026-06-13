using System.Globalization;
using _116.BuildingBlocks.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
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
        IFormFile fileMock = FileTestHelpers.CreateMockVideoFile();
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            Slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            VideoFile: fileMock,
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
        IFormFile fileMock = FileTestHelpers.CreateMockVideoFile();
        var command = new AdminCreateShortVideoCommand(
            Title: string.Empty,
            Slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            VideoFile: fileMock,
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
        IFormFile fileMock = FileTestHelpers.CreateMockVideoFile();
        var command = new AdminCreateShortVideoCommand(
            Title: new string('a', TestConstants.Content.Editorial.ShortVideo.TitleMaxLength + 1),
            Slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            VideoFile: fileMock,
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
                && e.ErrorMessage
                    == _i18n.ShortVideo.Msg.TitleTooLong(TestConstants.Content.Editorial.ShortVideo.TitleMaxLength)
            );
    }

    #endregion

    #region Slug Validation Tests

    [Fact]
    public async Task Validate_WithEmptySlug_ShouldHaveError()
    {
        // Arrange
        IFormFile fileMock = FileTestHelpers.CreateMockVideoFile();
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            Slug: string.Empty,
            VideoFile: fileMock,
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
        IFormFile fileMock = FileTestHelpers.CreateMockVideoFile();
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            Slug: "Invalid-Slug",
            VideoFile: fileMock,
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
                && e.ErrorMessage == _i18n.ShortVideo.Msg.FileRequired()
            );
    }

    [Fact]
    public async Task Validate_WithEmptyVideoFile_ShouldHaveError()
    {
        // Arrange
        IFormFile fileMock = FileTestHelpers.CreateMockFormFile("clip.mp4", "video/mp4", 0);
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            Slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            VideoFile: fileMock,
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
                && e.ErrorMessage == _i18n.ShortVideo.Msg.FileEmpty()
            );
    }

    [Fact]
    public async Task Validate_WithVideoFileExceedingMaxSize_ShouldHaveError()
    {
        // Arrange
        long oversizedBytes = FileConstants.MaxVideoFileSizeBytes + 1;
        IFormFile fileMock = FileTestHelpers.CreateMockFormFile("clip.mp4", "video/mp4", oversizedBytes);
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            Slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            VideoFile: fileMock,
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
                && e.ErrorMessage
                    == _i18n.ShortVideo.Msg.FileTooLarge(FileConstants.MaxVideoFileSizeBytes / (1024 * 1024))
            );
    }

    [Fact]
    public async Task Validate_WithInvalidVideoExtension_ShouldHaveError()
    {
        // Arrange
        IFormFile fileMock = FileTestHelpers.CreateMockFormFile("photo.jpg", "image/jpeg", 5_000_000);
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            Slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            VideoFile: fileMock,
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
                && e.ErrorMessage
                    == _i18n.ShortVideo.Msg.FileInvalidExtension(
                        string.Join(", ", FileConstants.AllowedVideoExtensions)
                    )
            );
    }

    [Theory]
    [InlineData("clip.mp4", "video/mp4")]
    [InlineData("clip.mov", "video/quicktime")]
    [InlineData("clip.webm", "video/webm")]
    [InlineData("clip.avi", "video/x-msvideo")]
    [InlineData("clip.mkv", "video/x-matroska")]
    [InlineData("clip.3gp", "video/3gpp")]
    public async Task Validate_WithAllowedVideoFormats_ShouldNotHaveErrors(string fileName, string contentType)
    {
        // Arrange
        IFormFile fileMock = FileTestHelpers.CreateMockFormFile(fileName, contentType, 5_000_000);
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            Slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            VideoFile: fileMock,
            AuthorId: Guid.NewGuid(),
            VideoId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithVideoFileAtMaxSize_ShouldNotHaveErrors()
    {
        // Arrange
        IFormFile fileMock = FileTestHelpers.CreateMockFormFile(
            "clip.mp4",
            "video/mp4",
            FileConstants.MaxVideoFileSizeBytes
        );
        var command = new AdminCreateShortVideoCommand(
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            Slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            VideoFile: fileMock,
            AuthorId: Guid.NewGuid(),
            VideoId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
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
        var validator = new AdminCreateShortVideoValidator(_i18n);
        IFormFile fileMock = FileTestHelpers.CreateMockVideoFile();
        var command = new AdminCreateShortVideoCommand(
            Title: string.Empty,
            Slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            VideoFile: fileMock,
            AuthorId: Guid.NewGuid(),
            VideoId: null
        );

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateShortVideoCommand.Title)
                && e.ErrorMessage == _i18n.ShortVideo.Msg.TitleRequired()
            );
    }

    #endregion
}
