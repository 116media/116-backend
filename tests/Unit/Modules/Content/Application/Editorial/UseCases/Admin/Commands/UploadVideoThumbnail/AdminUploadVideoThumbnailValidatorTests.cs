using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;

/// <summary>
/// Unit tests for <see cref="AdminUploadVideoThumbnailValidator"/>.
/// </summary>
public class AdminUploadVideoThumbnailValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminUploadVideoThumbnailValidator _validator;

    public AdminUploadVideoThumbnailValidatorTests()
    {
        _validator = new AdminUploadVideoThumbnailValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        var command = new AdminUploadVideoThumbnailCommand(VideoId: Guid.NewGuid().ToString(), File: fileMock.Object);

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
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        var command = new AdminUploadVideoThumbnailCommand(VideoId: string.Empty, File: fileMock.Object);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUploadVideoThumbnailCommand.VideoId)
                && e.ErrorMessage == _i18n.Video.Msg.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidVideoId_ShouldHaveError()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        var command = new AdminUploadVideoThumbnailCommand(VideoId: "not-a-guid", File: fileMock.Object);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUploadVideoThumbnailCommand.VideoId)
                && e.ErrorMessage == _i18n.Video.Msg.Localizer["IdInvalid"].Value
            );
    }

    #endregion
}
