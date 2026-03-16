using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo;

/// <summary>
/// Unit tests for <see cref="AdminUpdateVideoSeoValidator"/>.
/// </summary>
public class AdminUpdateVideoSeoValidatorTests
{
    private readonly AdminUpdateVideoSeoValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateVideoSeoCommand(
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
        var command = new AdminUpdateVideoSeoCommand(Id: string.Empty, MetaTitle: null, MetaDescription: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateVideoSeoCommand.Id) && e.ErrorMessage == "Video ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateVideoSeoCommand(Id: "not-a-guid", MetaTitle: null, MetaDescription: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateVideoSeoCommand.Id) && e.ErrorMessage == "Video ID is invalid."
            );
    }

    #endregion
}
