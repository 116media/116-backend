using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Validators;

/// <summary>
/// Unit tests for <see cref="RoleValidation"/>.
/// </summary>
public class RoleValidationTests
{
    private readonly ValidationErrorMessage _enMsg = LocalizerFactory.CreateMessage<ValidationErrorMessage>();

    private class TestNameCommand
    {
        public string? Name { get; set; }
    }

    private class TestDescriptionCommand
    {
        public string? Description { get; set; }
    }

    private class TestNameCommandValidator : AbstractValidator<TestNameCommand>
    {
        public TestNameCommandValidator(ValidationErrorMessage i18n, bool isRequired = true)
        {
            RuleFor(x => x.Name).ValidRoleName(i18n, isRequired: isRequired);
        }
    }

    private class TestDescriptionCommandValidator : AbstractValidator<TestDescriptionCommand>
    {
        public TestDescriptionCommandValidator(ValidationErrorMessage i18n, bool isRequired = true)
        {
            RuleFor(x => x.Description).ValidRoleDescription(i18n, isRequired: isRequired);
        }
    }

    #region ValidRoleName — required (default)

    [Fact]
    public void ValidRoleName_WithValidName_ShouldPass()
    {
        // Arrange
        var validator = new TestNameCommandValidator(_enMsg);
        var command = new TestNameCommand { Name = "Admin" };

        // Act
        TestValidationResult<TestNameCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ValidRoleName_WithMaxLengthName_ShouldPass()
    {
        // Arrange
        var validator = new TestNameCommandValidator(_enMsg);
        var command = new TestNameCommand { Name = new string('a', RoleConstants.MaxRoleNameLength) };

        // Act
        TestValidationResult<TestNameCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ValidRoleName_WithNullName_ShouldFail()
    {
        // Arrange
        var validator = new TestNameCommandValidator(_enMsg);
        var command = new TestNameCommand { Name = null };

        // Act
        TestValidationResult<TestNameCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage(_enMsg.RoleNameRequired());
    }

    [Fact]
    public void ValidRoleName_WithEmptyName_ShouldFail()
    {
        // Arrange
        var validator = new TestNameCommandValidator(_enMsg);
        var command = new TestNameCommand { Name = string.Empty };

        // Act
        TestValidationResult<TestNameCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage(_enMsg.RoleNameRequired());
    }

    [Fact]
    public void ValidRoleName_WithWhitespaceName_ShouldFail()
    {
        // Arrange
        var validator = new TestNameCommandValidator(_enMsg);
        var command = new TestNameCommand { Name = "   " };

        // Act
        TestValidationResult<TestNameCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage(_enMsg.RoleNameRequired());
    }

    [Fact]
    public void ValidRoleName_WithNameExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var validator = new TestNameCommandValidator(_enMsg);
        var command = new TestNameCommand { Name = new string('a', RoleConstants.MaxRoleNameLength + 1) };

        // Act
        TestValidationResult<TestNameCommand> result = validator.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(_enMsg.RoleNameTooLong(RoleConstants.MaxRoleNameLength));
    }

    #endregion

    #region ValidRoleName — optional (isRequired = false)

    [Fact]
    public void ValidRoleName_Optional_WithValidName_ShouldPass()
    {
        // Arrange
        var validator = new TestNameCommandValidator(_enMsg, isRequired: false);
        var command = new TestNameCommand { Name = "Admin" };

        // Act
        TestValidationResult<TestNameCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ValidRoleName_Optional_WithNullName_ShouldPass()
    {
        // Arrange
        var validator = new TestNameCommandValidator(_enMsg, isRequired: false);
        var command = new TestNameCommand { Name = null };

        // Act
        TestValidationResult<TestNameCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ValidRoleName_Optional_WithEmptyName_ShouldPass()
    {
        // Arrange
        var validator = new TestNameCommandValidator(_enMsg, isRequired: false);
        var command = new TestNameCommand { Name = string.Empty };

        // Act
        TestValidationResult<TestNameCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ValidRoleName_Optional_WithWhitespaceName_ShouldPass()
    {
        // Arrange
        var validator = new TestNameCommandValidator(_enMsg, isRequired: false);
        var command = new TestNameCommand { Name = "   " };

        // Act
        TestValidationResult<TestNameCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ValidRoleName_Optional_WithNameExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var validator = new TestNameCommandValidator(_enMsg, isRequired: false);
        var command = new TestNameCommand { Name = new string('a', RoleConstants.MaxRoleNameLength + 1) };

        // Act
        TestValidationResult<TestNameCommand> result = validator.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(_enMsg.RoleNameTooLong(RoleConstants.MaxRoleNameLength));
    }

    #endregion

    #region ValidRoleDescription — required (default)

    [Fact]
    public void ValidRoleDescription_WithValidDescription_ShouldPass()
    {
        // Arrange
        var validator = new TestDescriptionCommandValidator(_enMsg);
        var command = new TestDescriptionCommand { Description = "A valid role description." };

        // Act
        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ValidRoleDescription_WithMaxLengthDescription_ShouldPass()
    {
        // Arrange
        var validator = new TestDescriptionCommandValidator(_enMsg);
        var command = new TestDescriptionCommand
        {
            Description = new string('a', RoleConstants.MaxRoleDescriptionLength),
        };

        // Act
        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ValidRoleDescription_WithNullDescription_ShouldFail()
    {
        // Arrange
        var validator = new TestDescriptionCommandValidator(_enMsg);
        var command = new TestDescriptionCommand { Description = null };

        // Act
        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description).WithErrorMessage(_enMsg.RoleDescriptionRequired());
    }

    [Fact]
    public void ValidRoleDescription_WithEmptyDescription_ShouldFail()
    {
        // Arrange
        var validator = new TestDescriptionCommandValidator(_enMsg);
        var command = new TestDescriptionCommand { Description = string.Empty };

        // Act
        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description).WithErrorMessage(_enMsg.RoleDescriptionRequired());
    }

    [Fact]
    public void ValidRoleDescription_WithWhitespaceDescription_ShouldFail()
    {
        // Arrange
        var validator = new TestDescriptionCommandValidator(_enMsg);
        var command = new TestDescriptionCommand { Description = "   " };

        // Act
        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description).WithErrorMessage(_enMsg.RoleDescriptionRequired());
    }

    [Fact]
    public void ValidRoleDescription_WithDescriptionExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var validator = new TestDescriptionCommandValidator(_enMsg);
        var command = new TestDescriptionCommand
        {
            Description = new string('a', RoleConstants.MaxRoleDescriptionLength + 1),
        };

        // Act
        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(_enMsg.RoleDescriptionTooLong(RoleConstants.MaxRoleDescriptionLength));
    }

    #endregion

    #region ValidRoleDescription — optional (isRequired = false)

    [Fact]
    public void ValidRoleDescription_Optional_WithValidDescription_ShouldPass()
    {
        // Arrange
        var validator = new TestDescriptionCommandValidator(_enMsg, isRequired: false);
        var command = new TestDescriptionCommand { Description = "A valid role description." };

        // Act
        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ValidRoleDescription_Optional_WithNullDescription_ShouldPass()
    {
        // Arrange
        var validator = new TestDescriptionCommandValidator(_enMsg, isRequired: false);
        var command = new TestDescriptionCommand { Description = null };

        // Act
        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ValidRoleDescription_Optional_WithEmptyDescription_ShouldPass()
    {
        // Arrange
        var validator = new TestDescriptionCommandValidator(_enMsg, isRequired: false);
        var command = new TestDescriptionCommand { Description = string.Empty };

        // Act
        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ValidRoleDescription_Optional_WithWhitespaceDescription_ShouldPass()
    {
        // Arrange
        var validator = new TestDescriptionCommandValidator(_enMsg, isRequired: false);
        var command = new TestDescriptionCommand { Description = "   " };

        // Act
        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ValidRoleDescription_Optional_WithDescriptionExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var validator = new TestDescriptionCommandValidator(_enMsg, isRequired: false);
        var command = new TestDescriptionCommand
        {
            Description = new string('a', RoleConstants.MaxRoleDescriptionLength + 1),
        };

        // Act
        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(_enMsg.RoleDescriptionTooLong(RoleConstants.MaxRoleDescriptionLength));
    }

    #endregion
}
