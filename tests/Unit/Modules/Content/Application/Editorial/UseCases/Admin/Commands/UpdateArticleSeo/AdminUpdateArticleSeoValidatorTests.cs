using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleSeo;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleSeo;

/// <summary>
/// Unit tests for <see cref="AdminUpdateArticleSeoValidator"/>.
/// </summary>
public class AdminUpdateArticleSeoValidatorTests
{
    private readonly AdminUpdateArticleSeoValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateArticleSeoCommand(
            Id: Guid.NewGuid().ToString(),
            MetaTitle: null,
            MetaDescription: null
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
    public async Task Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateArticleSeoCommand(Id: string.Empty, MetaTitle: null, MetaDescription: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleSeoCommand.Id) && e.ErrorMessage == "Article ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateArticleSeoCommand(Id: "not-a-guid", MetaTitle: null, MetaDescription: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArticleSeoCommand.Id) && e.ErrorMessage == "Article ID is invalid."
            );
    }

    #endregion

    #region MetaTitle Validation Tests

    [Fact]
    public async Task Validate_WithMetaTitleTooShort_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateArticleSeoCommand(
            Id: Guid.NewGuid().ToString(),
            MetaTitle: new string('a', 5),
            MetaDescription: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateArticleSeoCommand.MetaTitle));
    }

    [Fact]
    public async Task Validate_WithMetaTitleExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateArticleSeoCommand(
            Id: Guid.NewGuid().ToString(),
            MetaTitle: new string('a', 71),
            MetaDescription: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateArticleSeoCommand.MetaTitle));
    }

    [Fact]
    public async Task Validate_WithNullMetaTitle_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateArticleSeoCommand(
            Id: Guid.NewGuid().ToString(),
            MetaTitle: null,
            MetaDescription: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region MetaDescription Validation Tests

    [Fact]
    public async Task Validate_WithMetaDescriptionTooShort_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateArticleSeoCommand(
            Id: Guid.NewGuid().ToString(),
            MetaTitle: null,
            MetaDescription: new string('a', 10)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateArticleSeoCommand.MetaDescription));
    }

    [Fact]
    public async Task Validate_WithMetaDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateArticleSeoCommand(
            Id: Guid.NewGuid().ToString(),
            MetaTitle: null,
            MetaDescription: new string('a', 161)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateArticleSeoCommand.MetaDescription));
    }

    [Fact]
    public async Task Validate_WithNullMetaDescription_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateArticleSeoCommand(
            Id: Guid.NewGuid().ToString(),
            MetaTitle: null,
            MetaDescription: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
