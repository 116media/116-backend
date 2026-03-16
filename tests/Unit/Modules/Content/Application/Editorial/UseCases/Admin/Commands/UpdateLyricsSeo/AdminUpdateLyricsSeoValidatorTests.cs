using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo;

/// <summary>
/// Unit tests for <see cref="AdminUpdateLyricsSeoValidator"/>.
/// </summary>
public class AdminUpdateLyricsSeoValidatorTests
{
    private readonly AdminUpdateLyricsSeoValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateLyricsSeoCommand(
            Id: Guid.NewGuid().ToString(),
            MetaTitle: null,
            MetaDescription: null,
            MetaKeywords: null,
            StructuredData: null
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
        var command = new AdminUpdateLyricsSeoCommand(
            Id: string.Empty,
            MetaTitle: null,
            MetaDescription: null,
            MetaKeywords: null,
            StructuredData: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsSeoCommand.Id) && e.ErrorMessage == "Lyrics ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateLyricsSeoCommand(
            Id: "not-a-guid",
            MetaTitle: null,
            MetaDescription: null,
            MetaKeywords: null,
            StructuredData: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsSeoCommand.Id) && e.ErrorMessage == "Lyrics ID is invalid."
            );
    }

    #endregion
}
