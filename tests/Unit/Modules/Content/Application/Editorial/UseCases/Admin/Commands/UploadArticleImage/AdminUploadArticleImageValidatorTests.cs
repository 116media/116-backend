using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Helpers;
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
    private readonly ArticleErrorMessage _i18n = LocalizerFactory.CreateMessage<ArticleErrorMessage>();

    private readonly AdminUploadArticleImageValidator _validator;

    public AdminUploadArticleImageValidatorTests()
    {
        _validator = new AdminUploadArticleImageValidator(_i18n);
    }

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
                && e.ErrorMessage == _i18n.Localizer["IdRequired"].Value
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
                && e.ErrorMessage == _i18n.Localizer["IdInvalid"].Value
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
                e.PropertyName == nameof(AdminUploadArticleImageCommand.File) && e.ErrorMessage == _i18n.FileRequired()
            );
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        var i18n = LocalizerFactory.CreateMessage<ArticleErrorMessage>(culture);
        var validator = new AdminUploadArticleImageValidator(i18n);
        var command = new AdminUploadArticleImageCommand(
            ArticleId: string.Empty,
            File: null!,
            ImageType: EnumArticleImageType.Cover
        );

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUploadArticleImageCommand.ArticleId)
                && e.ErrorMessage == i18n.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
