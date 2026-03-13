using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;
using AwesomeAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;

/// <summary>
/// Unit tests for <see cref="AdminUploadShortVideoThumbnailValidator"/>.
/// </summary>
public class AdminUploadShortVideoThumbnailValidatorTests
{
    private readonly AdminUploadShortVideoThumbnailValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        var command = new AdminUploadShortVideoThumbnailCommand(
            ShortVideoId: Guid.NewGuid().ToString(),
            File: fileMock.Object
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region ShortVideoId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyShortVideoId_ShouldHaveError()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        var command = new AdminUploadShortVideoThumbnailCommand(ShortVideoId: string.Empty, File: fileMock.Object);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUploadShortVideoThumbnailCommand.ShortVideoId)
                && e.ErrorMessage == "Short Video ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidShortVideoId_ShouldHaveError()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        var command = new AdminUploadShortVideoThumbnailCommand(ShortVideoId: "not-a-guid", File: fileMock.Object);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUploadShortVideoThumbnailCommand.ShortVideoId)
                && e.ErrorMessage == "Short Video ID is invalid."
            );
    }

    #endregion
}
