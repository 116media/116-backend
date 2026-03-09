using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType;

/// <summary>
/// Unit tests for <see cref="UpdateContentTypeValidator"/>.
/// </summary>
public class UpdateContentTypeValidatorTests
{
    private readonly UpdateContentTypeValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidIdAndName_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new UpdateContentTypeCommand(
            Id: Guid.NewGuid(),
            Name: TestConstants.Content.ContentType.ValidName
        );

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
        var command = new UpdateContentTypeCommand(
            Id: Guid.NewGuid(),
            Name: new string('a', TestConstants.Content.ContentType.NameMaxLength)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Id Validation Tests

    [Fact]
    public async Task Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateContentTypeCommand(Id: Guid.Empty, Name: TestConstants.Content.ContentType.ValidName);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(UpdateContentTypeCommand.Id)
                && e.ErrorMessage == "Content type ID is required."
            );
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateContentTypeCommand(Id: Guid.NewGuid(), Name: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(UpdateContentTypeCommand.Name)
                && e.ErrorMessage == "Content type name is required."
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateContentTypeCommand(
            Id: Guid.NewGuid(),
            Name: new string('a', TestConstants.Content.ContentType.NameMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(UpdateContentTypeCommand.Name)
                && e.ErrorMessage
                    == $"Content type name must not exceed {TestConstants.Content.ContentType.NameMaxLength} characters."
            );
    }

    [Fact]
    public async Task Validate_WithBothEmptyIdAndName_ShouldHaveTwoErrors()
    {
        // Arrange
        var command = new UpdateContentTypeCommand(Id: Guid.Empty, Name: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }

    #endregion
}
