using _116.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster;

/// <summary>
/// Unit tests for <see cref="AdminUploadCategoryPosterValidator"/>.
/// </summary>
public class AdminUploadCategoryPosterValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminUploadCategoryPosterValidator _validator;

    public AdminUploadCategoryPosterValidatorTests()
    {
        _validator = new AdminUploadCategoryPosterValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidIdAndFile_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUploadCategoryPosterCommand(Id: Guid.NewGuid().ToString(), File: CreateMockFormFile());

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Id Validation Tests

    [Fact]
    public async Task Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUploadCategoryPosterCommand(Id: string.Empty, File: CreateMockFormFile());

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUploadCategoryPosterCommand.Id));
    }

    [Fact]
    public async Task Validate_WithInvalidGuid_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUploadCategoryPosterCommand(Id: "not-a-guid", File: CreateMockFormFile());

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUploadCategoryPosterCommand.Id));
    }

    #endregion

    #region File Validation Tests

    [Fact]
    public async Task Validate_WithNullFile_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUploadCategoryPosterCommand(Id: Guid.NewGuid().ToString(), File: null!);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUploadCategoryPosterCommand.File)
                && e.ErrorMessage == _i18n.Category.Msg.FileRequired()
            );
    }

    #endregion

    private static IFormFile CreateMockFormFile()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("poster.jpg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        return fileMock.Object;
    }
}
