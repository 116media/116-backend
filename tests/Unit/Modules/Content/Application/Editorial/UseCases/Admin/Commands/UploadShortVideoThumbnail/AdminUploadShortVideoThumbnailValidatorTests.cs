using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
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
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminUploadShortVideoThumbnailValidator _validator;

    public AdminUploadShortVideoThumbnailValidatorTests()
    {
        _validator = new AdminUploadShortVideoThumbnailValidator(_i18n);
    }

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
                && e.ErrorMessage == _i18n.ShortVideo.Msg.Localizer["IdRequired"].Value
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
                && e.ErrorMessage == _i18n.ShortVideo.Msg.Localizer["IdInvalid"].Value
            );
    }

    #endregion
}
