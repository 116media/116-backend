using _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole;
using _116.Tests.Fixtures.Builders.Commands.Roles;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole;

/// <summary>
/// Unit tests for <see cref="AdminUpdateRoleValidator"/>.
/// </summary>
public class AdminUpdateRoleValidatorTests
{
    private readonly AdminUpdateRoleValidator _validator = new();
    private readonly Guid _validRoleId = Guid.NewGuid();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithAllValidValues_ShouldNotHaveErrors()
    {
        // Arrange
        AdminUpdateRoleCommand command = CommandFactory.Role.UpdateValidCommand(_validRoleId);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithOnlyName_ShouldNotHaveErrors()
    {
        // Arrange
        AdminUpdateRoleCommand command = CommandFactory.Role.UpdateCommand(
            _validRoleId,
            TestConstants.Role.ValidName,
            null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithOnlyDescription_ShouldNotHaveErrors()
    {
        // Arrange
        AdminUpdateRoleCommand command = CommandFactory.Role.UpdateCommand(
            _validRoleId,
            null,
            TestConstants.Role.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithAllNullValues_ShouldNotHaveErrors()
    {
        // Arrange
        AdminUpdateRoleCommand command = CommandFactory.Role.UpdateCommand(_validRoleId, null, null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithMaxLengthValues_ShouldNotHaveErrors()
    {
        // Arrange
        AdminUpdateRoleCommand command = new(
            RoleId: _validRoleId.ToString(),
            Name: new string('a', TestConstants.Role.NameMaxLength),
            Description: new string('a', TestConstants.Role.DescriptionMaxLength)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public async Task Validate_WithEmptyName_ShouldNotHaveErrors()
    {
        // Arrange - Update commands treat empty as "not provided" (optional field)
        AdminUpdateRoleCommand command = new UpdateRoleCommandBuilder().WithEmptyName().Build();

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithWhitespaceName_ShouldNotHaveErrors()
    {
        // Arrange - Whitespace-only is treated as "not provided"
        AdminUpdateRoleCommand command = new(
            RoleId: _validRoleId.ToString(),
            Name: "   ",
            Description: TestConstants.Role.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        AdminUpdateRoleCommand command = new UpdateRoleCommandBuilder().WithNameExceedingMaxLength().Build();

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminUpdateRoleCommand.Name)
                && e.ErrorMessage == TestConstants.ValidationMessages.Role.NameTooLong
            );
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public async Task Validate_WithEmptyDescription_ShouldNotHaveErrors()
    {
        // Arrange - Update commands treat empty as "not provided" (optional field)
        AdminUpdateRoleCommand command = new(
            RoleId: _validRoleId.ToString(),
            Name: TestConstants.Role.ValidName,
            Description: string.Empty
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithWhitespaceDescription_ShouldNotHaveErrors()
    {
        // Arrange - Whitespace-only is treated as "not provided"
        AdminUpdateRoleCommand command = new(
            RoleId: _validRoleId.ToString(),
            Name: TestConstants.Role.ValidName,
            Description: "   "
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        AdminUpdateRoleCommand command = new(
            RoleId: _validRoleId.ToString(),
            Name: TestConstants.Role.ValidName,
            Description: new string('a', TestConstants.Role.DescriptionMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminUpdateRoleCommand.Description)
                && e.ErrorMessage == TestConstants.ValidationMessages.Role.DescriptionTooLong
            );
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllValuesExceedingMaxLength_ShouldHaveMultipleErrors()
    {
        // Arrange
        AdminUpdateRoleCommand command = new(
            RoleId: _validRoleId.ToString(),
            Name: new string('a', TestConstants.Role.NameMaxLength + 1),
            Description: new string('a', TestConstants.Role.DescriptionMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateRoleCommand.Name));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateRoleCommand.Description));
    }

    #endregion
}
