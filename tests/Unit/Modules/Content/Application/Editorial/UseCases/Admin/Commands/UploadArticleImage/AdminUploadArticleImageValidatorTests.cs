using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage;

/// <summary>
/// Unit tests for <see cref="AdminUploadArticleImageValidator"/>.
/// </summary>
public class AdminUploadArticleImageValidatorTests
{
    private readonly AdminUploadArticleImageValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        var command = new AdminUploadArticleImageCommand(
            ArticleId: Guid.NewGuid().ToString(),
            File: fileMock.Object,
            ImageType: EnumArticleImageType.Cover
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region ArticleId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyArticleId_ShouldHaveError()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        var command = new AdminUploadArticleImageCommand(
            ArticleId: string.Empty,
            File: fileMock.Object,
            ImageType: EnumArticleImageType.Cover
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUploadArticleImageCommand.ArticleId)
                && e.ErrorMessage == "Article ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidArticleId_ShouldHaveError()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        var command = new AdminUploadArticleImageCommand(
            ArticleId: "not-a-guid",
            File: fileMock.Object,
            ImageType: EnumArticleImageType.Cover
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUploadArticleImageCommand.ArticleId)
                && e.ErrorMessage == "Article ID is invalid."
            );
    }

    #endregion

    #region File Validation Tests

    [Fact]
    public async Task Validate_WithNullFile_ShouldHaveError()
    {
        // Arrange
        IFormFile? file = null;
        var command = new AdminUploadArticleImageCommand(
            ArticleId: Guid.NewGuid().ToString(),
            File: file!,
            ImageType: EnumArticleImageType.Cover
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUploadArticleImageCommand.File)
                && e.ErrorMessage == "Article image file is required."
            );
    }

    #endregion
}
