using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole;
using _116.Unit.Tests.Common.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.Admin.Commands.CreateRole;

/// <summary>
/// Unit tests for <see cref="AdminCreateRoleValidator"/>.
/// </summary>
public class AdminCreateRoleValidatorTests
{
    private readonly AdminCreateRoleValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdminCreateRoleCommand command = new(
            Name: TestConstants.Role.ValidName,
            Description: TestConstants.Role.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithMinimumValidValues_ShouldNotHaveErrors()
    {
        // Arrange
        AdminCreateRoleCommand command = new(Name: "A", Description: "D");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithMaxLengthValues_ShouldNotHaveErrors()
    {
        // Arrange
        AdminCreateRoleCommand command = new(
            Name: new string('A', TestConstants.Role.NameMaxLength),
            Description: new string('D', TestConstants.Role.DescriptionMaxLength)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public async Task Validate_WithNullName_ShouldHaveError()
    {
        // Arrange
        AdminCreateRoleCommand command = new(Name: null!, Description: TestConstants.Role.ValidDescription);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateRoleCommand.Name)
                && e.ErrorMessage == TestConstants.ValidationMessages.Role.NameRequired
            );
    }

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        AdminCreateRoleCommand command = new(Name: string.Empty, Description: TestConstants.Role.ValidDescription);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateRoleCommand.Name)
                && e.ErrorMessage == TestConstants.ValidationMessages.Role.NameRequired
            );
    }

    [Fact]
    public async Task Validate_WithWhitespaceName_ShouldHaveError()
    {
        // Arrange
        AdminCreateRoleCommand command = new(Name: "   ", Description: TestConstants.Role.ValidDescription);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateRoleCommand.Name)
                && e.ErrorMessage == TestConstants.ValidationMessages.Role.NameRequired
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        AdminCreateRoleCommand command = new(
            Name: new string('A', TestConstants.Role.NameMaxLength + 1),
            Description: TestConstants.Role.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateRoleCommand.Name)
                && e.ErrorMessage == TestConstants.ValidationMessages.Role.NameTooLong
            );
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public async Task Validate_WithNullDescription_ShouldHaveError()
    {
        // Arrange
        AdminCreateRoleCommand command = new(Name: TestConstants.Role.ValidName, Description: null!);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateRoleCommand.Description)
                && e.ErrorMessage == TestConstants.ValidationMessages.Role.DescriptionRequired
            );
    }

    [Fact]
    public async Task Validate_WithEmptyDescription_ShouldHaveError()
    {
        // Arrange
        AdminCreateRoleCommand command = new(Name: TestConstants.Role.ValidName, Description: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateRoleCommand.Description)
                && e.ErrorMessage == TestConstants.ValidationMessages.Role.DescriptionRequired
            );
    }

    [Fact]
    public async Task Validate_WithWhitespaceDescription_ShouldHaveError()
    {
        // Arrange
        AdminCreateRoleCommand command = new(Name: TestConstants.Role.ValidName, Description: "   ");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateRoleCommand.Description)
                && e.ErrorMessage == TestConstants.ValidationMessages.Role.DescriptionRequired
            );
    }

    [Fact]
    public async Task Validate_WithDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        AdminCreateRoleCommand command = new(
            Name: TestConstants.Role.ValidName,
            Description: new string('D', TestConstants.Role.DescriptionMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreateRoleCommand.Description)
                && e.ErrorMessage == TestConstants.ValidationMessages.Role.DescriptionTooLong
            );
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        AdminCreateRoleCommand command = new(Name: string.Empty, Description: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminCreateRoleCommand.Name));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminCreateRoleCommand.Description));
    }

    #endregion
}
