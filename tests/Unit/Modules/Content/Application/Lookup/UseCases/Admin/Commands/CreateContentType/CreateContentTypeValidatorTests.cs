using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType;

/// <summary>
/// Unit tests for <see cref="CreateContentTypeValidator"/>.
/// </summary>
public class CreateContentTypeValidatorTests
{
    private readonly CreateContentTypeValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidName_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateContentTypeCommand(Name: TestConstants.Content.ContentType.ValidName);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithMaxLengthName_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateContentTypeCommand(
            Name: new string('a', TestConstants.Content.ContentType.NameMaxLength)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateContentTypeCommand(Name: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(CreateContentTypeCommand.Name)
                && e.ErrorMessage == "Content type name is required."
            );
    }

    [Fact]
    public async Task Validate_WithNullName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateContentTypeCommand(Name: null!);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(CreateContentTypeCommand.Name)
                && e.ErrorMessage == "Content type name is required."
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new CreateContentTypeCommand(
            Name: new string('a', TestConstants.Content.ContentType.NameMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(CreateContentTypeCommand.Name)
                && e.ErrorMessage == "Content type name must not exceed 30 characters."
            );
    }

    #endregion
}
