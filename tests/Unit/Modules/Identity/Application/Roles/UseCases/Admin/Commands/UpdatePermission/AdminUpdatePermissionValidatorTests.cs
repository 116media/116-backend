using _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission;

/// <summary>
/// Unit tests for <see cref="AdminUpdatePermissionValidator"/>.
/// </summary>
public class AdminUpdatePermissionValidatorTests
{
    private readonly AdminUpdatePermissionValidator _validator = new();
    private readonly Guid _validPermissionId = Guid.NewGuid();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithAllValidValues_ShouldNotHaveErrors()
    {
        // Arrange
        AdminUpdatePermissionCommand command = CommandFactory.Permission.UpdateValidCommand(_validPermissionId);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithOnlyResource_ShouldNotHaveErrors()
    {
        // Arrange
        AdminUpdatePermissionCommand command = CommandFactory.Permission.UpdateCommand(
            _validPermissionId,
            TestConstants.Permission.ValidResource,
            null,
            null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithOnlyAction_ShouldNotHaveErrors()
    {
        // Arrange
        AdminUpdatePermissionCommand command = CommandFactory.Permission.UpdateCommand(
            _validPermissionId,
            null,
            TestConstants.Permission.ValidAction,
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
        AdminUpdatePermissionCommand command = CommandFactory.Permission.UpdateCommand(
            _validPermissionId,
            null,
            null,
            TestConstants.Permission.ValidDescription
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
        AdminUpdatePermissionCommand command = CommandFactory.Permission.UpdateCommand(
            _validPermissionId,
            null,
            null,
            null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithMaxLengthValues_ShouldNotHaveErrors()
    {
        // Arrange
        AdminUpdatePermissionCommand command = new(
            PermissionId: _validPermissionId.ToString(),
            Resource: new string('R', TestConstants.Permission.ResourceMaxLength),
            Action: new string('A', TestConstants.Permission.ActionMaxLength),
            Description: new string('D', TestConstants.Permission.DescriptionMaxLength)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Resource Validation Tests

    [Fact]
    public async Task Validate_WithEmptyResource_ShouldNotHaveErrors()
    {
        // Arrange - Update commands allow empty/null for optional updates
        AdminUpdatePermissionCommand command = new(
            PermissionId: _validPermissionId.ToString(),
            Resource: string.Empty,
            Action: TestConstants.Permission.ValidAction,
            Description: TestConstants.Permission.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert - Empty is treated as "not provided" for optional fields
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithWhitespaceResource_ShouldNotHaveErrors()
    {
        // Arrange - Whitespace-only is treated as "not provided"
        AdminUpdatePermissionCommand command = new(
            PermissionId: _validPermissionId.ToString(),
            Resource: "   ",
            Action: TestConstants.Permission.ValidAction,
            Description: TestConstants.Permission.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithResourceExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        AdminUpdatePermissionCommand command = new(
            PermissionId: _validPermissionId.ToString(),
            Resource: new string('R', TestConstants.Permission.ResourceMaxLength + 1),
            Action: TestConstants.Permission.ValidAction,
            Description: TestConstants.Permission.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminUpdatePermissionCommand.Resource)
                && e.ErrorMessage == TestConstants.ValidationMessages.Permission.ResourceTooLong
            );
    }

    #endregion

    #region Action Validation Tests

    [Fact]
    public async Task Validate_WithEmptyAction_ShouldNotHaveErrors()
    {
        // Arrange - Update commands allow empty/null for optional updates
        AdminUpdatePermissionCommand command = new(
            PermissionId: _validPermissionId.ToString(),
            Resource: TestConstants.Permission.ValidResource,
            Action: string.Empty,
            Description: TestConstants.Permission.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert - Empty is treated as "not provided" for optional fields
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithWhitespaceAction_ShouldNotHaveErrors()
    {
        // Arrange - Whitespace-only is treated as "not provided"
        AdminUpdatePermissionCommand command = new(
            PermissionId: _validPermissionId.ToString(),
            Resource: TestConstants.Permission.ValidResource,
            Action: "   ",
            Description: TestConstants.Permission.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithActionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        AdminUpdatePermissionCommand command = new(
            PermissionId: _validPermissionId.ToString(),
            Resource: TestConstants.Permission.ValidResource,
            Action: new string('A', TestConstants.Permission.ActionMaxLength + 1),
            Description: TestConstants.Permission.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminUpdatePermissionCommand.Action)
                && e.ErrorMessage == TestConstants.ValidationMessages.Permission.ActionTooLong
            );
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public async Task Validate_WithEmptyDescription_ShouldNotHaveErrors()
    {
        // Arrange - Update commands allow empty/null for optional updates
        AdminUpdatePermissionCommand command = new(
            PermissionId: _validPermissionId.ToString(),
            Resource: TestConstants.Permission.ValidResource,
            Action: TestConstants.Permission.ValidAction,
            Description: string.Empty
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert - Empty is treated as "not provided" for optional fields
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithWhitespaceDescription_ShouldNotHaveErrors()
    {
        // Arrange - Whitespace-only is treated as "not provided"
        AdminUpdatePermissionCommand command = new(
            PermissionId: _validPermissionId.ToString(),
            Resource: TestConstants.Permission.ValidResource,
            Action: TestConstants.Permission.ValidAction,
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
        AdminUpdatePermissionCommand command = new(
            PermissionId: _validPermissionId.ToString(),
            Resource: TestConstants.Permission.ValidResource,
            Action: TestConstants.Permission.ValidAction,
            Description: new string('D', TestConstants.Permission.DescriptionMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminUpdatePermissionCommand.Description)
                && e.ErrorMessage == TestConstants.ValidationMessages.Permission.DescriptionTooLong
            );
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllValuesExceedingMaxLength_ShouldHaveMultipleErrors()
    {
        // Arrange
        AdminUpdatePermissionCommand command = new(
            PermissionId: _validPermissionId.ToString(),
            Resource: new string('R', TestConstants.Permission.ResourceMaxLength + 1),
            Action: new string('A', TestConstants.Permission.ActionMaxLength + 1),
            Description: new string('D', TestConstants.Permission.DescriptionMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdatePermissionCommand.Resource));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdatePermissionCommand.Action));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdatePermissionCommand.Description));
    }

    #endregion
}
