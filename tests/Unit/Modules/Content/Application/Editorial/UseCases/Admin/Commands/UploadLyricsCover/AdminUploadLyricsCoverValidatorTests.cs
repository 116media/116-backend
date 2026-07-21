using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadLyricsCover;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadLyricsCover;

/// <summary>
/// Unit tests for <see cref="AdminUploadLyricsCoverValidator"/>.
/// </summary>
public class AdminUploadLyricsCoverValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminUploadLyricsCoverValidator _validator;

    public AdminUploadLyricsCoverValidatorTests()
    {
        _validator = new AdminUploadLyricsCoverValidator(_i18n);
    }

    [Fact]
    public async Task Validate_WithFileProvided_ShouldNotHaveErrors()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var command = new AdminUploadLyricsCoverCommand(LyricsId: Guid.NewGuid(), File: fileMock.Object);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithMissingFile_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUploadLyricsCoverCommand(LyricsId: Guid.NewGuid(), File: null!);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUploadLyricsCoverCommand.File)
                && e.ErrorMessage == _i18n.Lyrics.Msg.FileRequired()
            );
    }
}
