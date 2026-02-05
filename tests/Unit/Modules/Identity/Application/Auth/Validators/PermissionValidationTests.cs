using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Validators;

/// <summary>
/// Unit tests for <see cref="PermissionValidation"/>.
/// </summary>
public class PermissionValidationTests
{
    private class TestResourceCommand
    {
        public string? Resource { get; set; }
    }

    private class TestActionCommand
    {
        public string? Action { get; set; }
    }

    private class TestDescriptionCommand
    {
        public string? Description { get; set; }
    }

    private class TestResourceCommandValidator : AbstractValidator<TestResourceCommand>
    {
        public TestResourceCommandValidator(bool isRequired = true)
        {
            RuleFor(x => x.Resource).ValidPermissionResource(isRequired);
        }
    }

    private class TestActionCommandValidator : AbstractValidator<TestActionCommand>
    {
        public TestActionCommandValidator(bool isRequired = true)
        {
            RuleFor(x => x.Action).ValidPermissionAction(isRequired);
        }
    }

    private class TestDescriptionCommandValidator : AbstractValidator<TestDescriptionCommand>
    {
        public TestDescriptionCommandValidator(bool isRequired = true)
        {
            RuleFor(x => x.Description).ValidPermissionDescription(isRequired);
        }
    }

    #region ValidPermissionResource — required (default)

    [Fact]
    public void ValidPermissionResource_WithValidResource_ShouldPass()
    {
        var validator = new TestResourceCommandValidator();
        var command = new TestResourceCommand { Resource = "users" };

        TestValidationResult<TestResourceCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Resource);
    }

    [Fact]
    public void ValidPermissionResource_WithMaxLengthResource_ShouldPass()
    {
        var validator = new TestResourceCommandValidator();
        var command = new TestResourceCommand
        {
            Resource = new string('a', PermissionConstants.MaxPermissionResourceLength),
        };

        TestValidationResult<TestResourceCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Resource);
    }

    [Fact]
    public void ValidPermissionResource_WithNullResource_ShouldFail()
    {
        var validator = new TestResourceCommandValidator();
        var command = new TestResourceCommand { Resource = null };

        TestValidationResult<TestResourceCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Resource).WithErrorMessage("Permission resource is required");
    }

    [Fact]
    public void ValidPermissionResource_WithEmptyResource_ShouldFail()
    {
        var validator = new TestResourceCommandValidator();
        var command = new TestResourceCommand { Resource = string.Empty };

        TestValidationResult<TestResourceCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Resource).WithErrorMessage("Permission resource is required");
    }

    [Fact]
    public void ValidPermissionResource_WithWhitespaceResource_ShouldFail()
    {
        var validator = new TestResourceCommandValidator();
        var command = new TestResourceCommand { Resource = "   " };

        TestValidationResult<TestResourceCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Resource).WithErrorMessage("Permission resource is required");
    }

    [Fact]
    public void ValidPermissionResource_WithResourceExceedingMaxLength_ShouldFail()
    {
        var validator = new TestResourceCommandValidator();
        var command = new TestResourceCommand
        {
            Resource = new string('a', PermissionConstants.MaxPermissionResourceLength + 1),
        };

        TestValidationResult<TestResourceCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.Resource)
            .WithErrorMessage(
                $"Permission resource cannot exceed {PermissionConstants.MaxPermissionResourceLength} characters"
            );
    }

    #endregion

    #region ValidPermissionResource — optional (isRequired = false)

    [Fact]
    public void ValidPermissionResource_Optional_WithValidResource_ShouldPass()
    {
        var validator = new TestResourceCommandValidator(isRequired: false);
        var command = new TestResourceCommand { Resource = "users" };

        TestValidationResult<TestResourceCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Resource);
    }

    [Fact]
    public void ValidPermissionResource_Optional_WithNullResource_ShouldPass()
    {
        var validator = new TestResourceCommandValidator(isRequired: false);
        var command = new TestResourceCommand { Resource = null };

        TestValidationResult<TestResourceCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Resource);
    }

    [Fact]
    public void ValidPermissionResource_Optional_WithEmptyResource_ShouldPass()
    {
        var validator = new TestResourceCommandValidator(isRequired: false);
        var command = new TestResourceCommand { Resource = string.Empty };

        TestValidationResult<TestResourceCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Resource);
    }

    [Fact]
    public void ValidPermissionResource_Optional_WithWhitespaceResource_ShouldPass()
    {
        var validator = new TestResourceCommandValidator(isRequired: false);
        var command = new TestResourceCommand { Resource = "   " };

        TestValidationResult<TestResourceCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Resource);
    }

    [Fact]
    public void ValidPermissionResource_Optional_WithResourceExceedingMaxLength_ShouldFail()
    {
        var validator = new TestResourceCommandValidator(isRequired: false);
        var command = new TestResourceCommand
        {
            Resource = new string('a', PermissionConstants.MaxPermissionResourceLength + 1),
        };

        TestValidationResult<TestResourceCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.Resource)
            .WithErrorMessage(
                $"Permission resource cannot exceed {PermissionConstants.MaxPermissionResourceLength} characters"
            );
    }

    #endregion

    #region ValidPermissionAction — required (default)

    [Fact]
    public void ValidPermissionAction_WithValidAction_ShouldPass()
    {
        var validator = new TestActionCommandValidator();
        var command = new TestActionCommand { Action = "read" };

        TestValidationResult<TestActionCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Action);
    }

    [Fact]
    public void ValidPermissionAction_WithMaxLengthAction_ShouldPass()
    {
        var validator = new TestActionCommandValidator();
        var command = new TestActionCommand { Action = new string('a', PermissionConstants.MaxPermissionActionLength) };

        TestValidationResult<TestActionCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Action);
    }

    [Fact]
    public void ValidPermissionAction_WithNullAction_ShouldFail()
    {
        var validator = new TestActionCommandValidator();
        var command = new TestActionCommand { Action = null };

        TestValidationResult<TestActionCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Action).WithErrorMessage("Permission action is required");
    }

    [Fact]
    public void ValidPermissionAction_WithEmptyAction_ShouldFail()
    {
        var validator = new TestActionCommandValidator();
        var command = new TestActionCommand { Action = string.Empty };

        TestValidationResult<TestActionCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Action).WithErrorMessage("Permission action is required");
    }

    [Fact]
    public void ValidPermissionAction_WithWhitespaceAction_ShouldFail()
    {
        var validator = new TestActionCommandValidator();
        var command = new TestActionCommand { Action = "   " };

        TestValidationResult<TestActionCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Action).WithErrorMessage("Permission action is required");
    }

    [Fact]
    public void ValidPermissionAction_WithActionExceedingMaxLength_ShouldFail()
    {
        var validator = new TestActionCommandValidator();
        var command = new TestActionCommand
        {
            Action = new string('a', PermissionConstants.MaxPermissionActionLength + 1),
        };

        TestValidationResult<TestActionCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.Action)
            .WithErrorMessage(
                $"Permission action cannot exceed {PermissionConstants.MaxPermissionActionLength} characters"
            );
    }

    #endregion

    #region ValidPermissionAction — optional (isRequired = false)

    [Fact]
    public void ValidPermissionAction_Optional_WithValidAction_ShouldPass()
    {
        var validator = new TestActionCommandValidator(isRequired: false);
        var command = new TestActionCommand { Action = "read" };

        TestValidationResult<TestActionCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Action);
    }

    [Fact]
    public void ValidPermissionAction_Optional_WithNullAction_ShouldPass()
    {
        var validator = new TestActionCommandValidator(isRequired: false);
        var command = new TestActionCommand { Action = null };

        TestValidationResult<TestActionCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Action);
    }

    [Fact]
    public void ValidPermissionAction_Optional_WithEmptyAction_ShouldPass()
    {
        var validator = new TestActionCommandValidator(isRequired: false);
        var command = new TestActionCommand { Action = string.Empty };

        TestValidationResult<TestActionCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Action);
    }

    [Fact]
    public void ValidPermissionAction_Optional_WithWhitespaceAction_ShouldPass()
    {
        var validator = new TestActionCommandValidator(isRequired: false);
        var command = new TestActionCommand { Action = "   " };

        TestValidationResult<TestActionCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Action);
    }

    [Fact]
    public void ValidPermissionAction_Optional_WithActionExceedingMaxLength_ShouldFail()
    {
        var validator = new TestActionCommandValidator(isRequired: false);
        var command = new TestActionCommand
        {
            Action = new string('a', PermissionConstants.MaxPermissionActionLength + 1),
        };

        TestValidationResult<TestActionCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.Action)
            .WithErrorMessage(
                $"Permission action cannot exceed {PermissionConstants.MaxPermissionActionLength} characters"
            );
    }

    #endregion

    #region ValidPermissionDescription — required (default)

    [Fact]
    public void ValidPermissionDescription_WithValidDescription_ShouldPass()
    {
        var validator = new TestDescriptionCommandValidator();
        var command = new TestDescriptionCommand { Description = "Allows reading user data." };

        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ValidPermissionDescription_WithMaxLengthDescription_ShouldPass()
    {
        var validator = new TestDescriptionCommandValidator();
        var command = new TestDescriptionCommand
        {
            Description = new string('a', PermissionConstants.MaxPermissionDescriptionLength),
        };

        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ValidPermissionDescription_WithNullDescription_ShouldFail()
    {
        var validator = new TestDescriptionCommandValidator();
        var command = new TestDescriptionCommand { Description = null };

        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description).WithErrorMessage("Permission description is required");
    }

    [Fact]
    public void ValidPermissionDescription_WithEmptyDescription_ShouldFail()
    {
        var validator = new TestDescriptionCommandValidator();
        var command = new TestDescriptionCommand { Description = string.Empty };

        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description).WithErrorMessage("Permission description is required");
    }

    [Fact]
    public void ValidPermissionDescription_WithWhitespaceDescription_ShouldFail()
    {
        var validator = new TestDescriptionCommandValidator();
        var command = new TestDescriptionCommand { Description = "   " };

        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description).WithErrorMessage("Permission description is required");
    }

    [Fact]
    public void ValidPermissionDescription_WithDescriptionExceedingMaxLength_ShouldFail()
    {
        var validator = new TestDescriptionCommandValidator();
        var command = new TestDescriptionCommand
        {
            Description = new string('a', PermissionConstants.MaxPermissionDescriptionLength + 1),
        };

        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(
                $"Permission description cannot exceed {PermissionConstants.MaxPermissionDescriptionLength} characters"
            );
    }

    #endregion

    #region ValidPermissionDescription — optional (isRequired = false)

    [Fact]
    public void ValidPermissionDescription_Optional_WithValidDescription_ShouldPass()
    {
        var validator = new TestDescriptionCommandValidator(isRequired: false);
        var command = new TestDescriptionCommand { Description = "Allows reading user data." };

        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ValidPermissionDescription_Optional_WithNullDescription_ShouldPass()
    {
        var validator = new TestDescriptionCommandValidator(isRequired: false);
        var command = new TestDescriptionCommand { Description = null };

        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ValidPermissionDescription_Optional_WithEmptyDescription_ShouldPass()
    {
        var validator = new TestDescriptionCommandValidator(isRequired: false);
        var command = new TestDescriptionCommand { Description = string.Empty };

        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ValidPermissionDescription_Optional_WithWhitespaceDescription_ShouldPass()
    {
        var validator = new TestDescriptionCommandValidator(isRequired: false);
        var command = new TestDescriptionCommand { Description = "   " };

        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ValidPermissionDescription_Optional_WithDescriptionExceedingMaxLength_ShouldFail()
    {
        var validator = new TestDescriptionCommandValidator(isRequired: false);
        var command = new TestDescriptionCommand
        {
            Description = new string('a', PermissionConstants.MaxPermissionDescriptionLength + 1),
        };

        TestValidationResult<TestDescriptionCommand> result = validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(
                $"Permission description cannot exceed {PermissionConstants.MaxPermissionDescriptionLength} characters"
            );
    }

    #endregion
}
