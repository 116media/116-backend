using _116.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission;
using _116.Unit.Tests.Common.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission;

/// <summary>
/// Unit tests for <see cref="AdminCreatePermissionValidator"/>.
/// </summary>
public class AdminCreatePermissionValidatorTests
{
    private readonly AdminCreatePermissionValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: TestConstants.Permission.ValidResource,
            Action: TestConstants.Permission.ValidAction,
            Description: TestConstants.Permission.ValidDescription
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
        AdminCreatePermissionCommand command = new(Resource: "R", Action: "A", Description: "D");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithMaxLengthValues_ShouldNotHaveErrors()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
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
    public async Task Validate_WithNullResource_ShouldHaveError()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: null!,
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
                e.PropertyName == nameof(AdminCreatePermissionCommand.Resource)
                && e.ErrorMessage == TestConstants.ValidationMessages.Permission.ResourceRequired
            );
    }

    [Fact]
    public async Task Validate_WithEmptyResource_ShouldHaveError()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: string.Empty,
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
                e.PropertyName == nameof(AdminCreatePermissionCommand.Resource)
                && e.ErrorMessage == TestConstants.ValidationMessages.Permission.ResourceRequired
            );
    }

    [Fact]
    public async Task Validate_WithWhitespaceResource_ShouldHaveError()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: "   ",
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
                e.PropertyName == nameof(AdminCreatePermissionCommand.Resource)
                && e.ErrorMessage == TestConstants.ValidationMessages.Permission.ResourceRequired
            );
    }

    [Fact]
    public async Task Validate_WithResourceExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
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
                e.PropertyName == nameof(AdminCreatePermissionCommand.Resource)
                && e.ErrorMessage == TestConstants.ValidationMessages.Permission.ResourceTooLong
            );
    }

    #endregion

    #region Action Validation Tests

    [Fact]
    public async Task Validate_WithNullAction_ShouldHaveError()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: TestConstants.Permission.ValidResource,
            Action: null!,
            Description: TestConstants.Permission.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePermissionCommand.Action)
                && e.ErrorMessage == TestConstants.ValidationMessages.Permission.ActionRequired
            );
    }

    [Fact]
    public async Task Validate_WithEmptyAction_ShouldHaveError()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: TestConstants.Permission.ValidResource,
            Action: string.Empty,
            Description: TestConstants.Permission.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePermissionCommand.Action)
                && e.ErrorMessage == TestConstants.ValidationMessages.Permission.ActionRequired
            );
    }

    [Fact]
    public async Task Validate_WithWhitespaceAction_ShouldHaveError()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: TestConstants.Permission.ValidResource,
            Action: "   ",
            Description: TestConstants.Permission.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePermissionCommand.Action)
                && e.ErrorMessage == TestConstants.ValidationMessages.Permission.ActionRequired
            );
    }

    [Fact]
    public async Task Validate_WithActionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
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
                e.PropertyName == nameof(AdminCreatePermissionCommand.Action)
                && e.ErrorMessage == TestConstants.ValidationMessages.Permission.ActionTooLong
            );
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public async Task Validate_WithNullDescription_ShouldHaveError()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: TestConstants.Permission.ValidResource,
            Action: TestConstants.Permission.ValidAction,
            Description: null!
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePermissionCommand.Description)
                && e.ErrorMessage == TestConstants.ValidationMessages.Permission.DescriptionRequired
            );
    }

    [Fact]
    public async Task Validate_WithEmptyDescription_ShouldHaveError()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: TestConstants.Permission.ValidResource,
            Action: TestConstants.Permission.ValidAction,
            Description: string.Empty
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePermissionCommand.Description)
                && e.ErrorMessage == TestConstants.ValidationMessages.Permission.DescriptionRequired
            );
    }

    [Fact]
    public async Task Validate_WithWhitespaceDescription_ShouldHaveError()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: TestConstants.Permission.ValidResource,
            Action: TestConstants.Permission.ValidAction,
            Description: "   "
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePermissionCommand.Description)
                && e.ErrorMessage == TestConstants.ValidationMessages.Permission.DescriptionRequired
            );
    }

    [Fact]
    public async Task Validate_WithDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
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
                e.PropertyName == nameof(AdminCreatePermissionCommand.Description)
                && e.ErrorMessage == TestConstants.ValidationMessages.Permission.DescriptionTooLong
            );
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: string.Empty,
            Action: string.Empty,
            Description: string.Empty
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminCreatePermissionCommand.Resource));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminCreatePermissionCommand.Action));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminCreatePermissionCommand.Description));
    }

    [Fact]
    public async Task Validate_WithAllValuesExceedingMaxLength_ShouldHaveMultipleErrors()
    {
        // Arrange
        AdminCreatePermissionCommand command = new(
            Resource: new string('R', TestConstants.Permission.ResourceMaxLength + 1),
            Action: new string('A', TestConstants.Permission.ActionMaxLength + 1),
            Description: new string('D', TestConstants.Permission.DescriptionMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
    }

    #endregion
}
