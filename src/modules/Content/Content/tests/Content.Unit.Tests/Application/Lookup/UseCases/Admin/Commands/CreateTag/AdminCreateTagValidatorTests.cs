using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Lookup.UseCases.Admin.Commands.CreateTag;

/// <summary>
/// Unit tests for <see cref="AdminCreateTagValidator"/>.
/// </summary>
public class AdminCreateTagValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminCreateTagValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateTagValidatorTests"/>.
    /// </summary>
    public AdminCreateTagValidatorTests()
    {
        _validator = new AdminCreateTagValidator(_i18n);
    }

    #region Valid Command Tests
    [Fact]
    public async Task Validate_WithSlugContainingNumbers_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateTagCommand(Name: TestConstants.Tag.ValidName);

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
        var command = new AdminCreateTagCommand(Name: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateTagCommand.Name) && e.ErrorMessage == _i18n.Tag.Msg.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNullName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateTagCommand(Name: null!);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateTagCommand.Name) && e.ErrorMessage == _i18n.Tag.Msg.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateTagCommand(Name: new string('a', TestConstants.Tag.NameMaxLength + 1));

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateTagCommand.Name)
                && e.ErrorMessage == _i18n.Tag.Msg.NameTooLong(TestConstants.Tag.NameMaxLength)
            );
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        var command = new AdminCreateTagCommand(Name: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminCreateTagCommand.Name));
    }

    #endregion
}
