using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Extensions;

/// <summary>
/// Unit tests for <see cref="ValidationExtension"/>.
/// </summary>
public class ValidationExtensionTests
{
    private readonly CategoryErrorMessage _i18n = LocalizerFactory.CreateMessage<CategoryErrorMessage>();

    private class TestCommand
    {
        public string? Id { get; set; }
    }

    private class TestRequiredGuidValidator : AbstractValidator<TestCommand>
    {
        public TestRequiredGuidValidator(CategoryErrorMessage i18n)
        {
            RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
        }
    }

    private class TestOptionalGuidValidator : AbstractValidator<TestCommand>
    {
        public TestOptionalGuidValidator(CategoryErrorMessage i18n)
        {
            RuleFor(x => x.Id).IsValidGuid(i18n.Localizer, isRequired: false);
        }
    }

    private class TestCustomKeysGuidValidator : AbstractValidator<TestCommand>
    {
        public TestCustomKeysGuidValidator(CategoryErrorMessage i18n)
        {
            RuleFor(x => x.Id).IsValidGuid(i18n.Localizer, requiredKey: "IdRequired", invalidKey: "IdInvalid");
        }
    }

    private class TestCustomKeysOptionalGuidValidator : AbstractValidator<TestCommand>
    {
        public TestCustomKeysOptionalGuidValidator(CategoryErrorMessage i18n)
        {
            RuleFor(x => x.Id)
                .IsValidGuid(i18n.Localizer, requiredKey: "IdRequired", invalidKey: "IdInvalid", isRequired: false);
        }
    }

    #region IsValidGuid — required (default)

    [Fact]
    public void IsValidGuid_WithValidGuid_ShouldPass()
    {
        var validator = new TestRequiredGuidValidator(_i18n);
        var command = new TestCommand { Id = Guid.NewGuid().ToString() };

        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void IsValidGuid_WithNullId_ShouldFailWithRequiredMessage()
    {
        var validator = new TestRequiredGuidValidator(_i18n);
        var command = new TestCommand { Id = null };

        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id).WithErrorMessage(_i18n.Localizer["IdRequired"].Value);
    }

    [Fact]
    public void IsValidGuid_WithEmptyId_ShouldFailWithRequiredMessage()
    {
        var validator = new TestRequiredGuidValidator(_i18n);
        var command = new TestCommand { Id = string.Empty };

        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id).WithErrorMessage(_i18n.Localizer["IdRequired"].Value);
    }

    [Fact]
    public void IsValidGuid_WithInvalidGuid_ShouldFailWithInvalidMessage()
    {
        var validator = new TestRequiredGuidValidator(_i18n);
        var command = new TestCommand { Id = "not-a-guid" };

        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id).WithErrorMessage(_i18n.Localizer["IdInvalid"].Value);
    }

    [Fact]
    public void IsValidGuid_WithEmptyId_ShouldStopCascading()
    {
        // CascadeMode.Stop: only the NotEmpty error fires, not the format error
        var validator = new TestRequiredGuidValidator(_i18n);
        var command = new TestCommand { Id = string.Empty };

        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(TestCommand.Id));
    }

    #endregion

    #region IsValidGuid — optional (isRequired: false)

    [Fact]
    public void IsValidGuid_Optional_WithNullId_ShouldPass()
    {
        var validator = new TestOptionalGuidValidator(_i18n);
        var command = new TestCommand { Id = null };

        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void IsValidGuid_Optional_WithEmptyId_ShouldPass()
    {
        var validator = new TestOptionalGuidValidator(_i18n);
        var command = new TestCommand { Id = string.Empty };

        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void IsValidGuid_Optional_WithWhitespaceId_ShouldPass()
    {
        var validator = new TestOptionalGuidValidator(_i18n);
        var command = new TestCommand { Id = "   " };

        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void IsValidGuid_Optional_WithValidGuid_ShouldPass()
    {
        var validator = new TestOptionalGuidValidator(_i18n);
        var command = new TestCommand { Id = Guid.NewGuid().ToString() };

        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void IsValidGuid_Optional_WithInvalidGuid_ShouldFailWithInvalidMessage()
    {
        var validator = new TestOptionalGuidValidator(_i18n);
        var command = new TestCommand { Id = "not-a-guid" };

        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id).WithErrorMessage(_i18n.Localizer["IdInvalid"].Value);
    }

    #endregion

    #region IsValidGuid with custom keys — required (default)

    [Fact]
    public void IsValidGuid_CustomKeys_WithValidGuid_ShouldPass()
    {
        var validator = new TestCustomKeysGuidValidator(_i18n);
        var command = new TestCommand { Id = Guid.NewGuid().ToString() };

        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void IsValidGuid_CustomKeys_WithEmptyId_ShouldFailWithRequiredMessage()
    {
        var validator = new TestCustomKeysGuidValidator(_i18n);
        var command = new TestCommand { Id = string.Empty };

        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id).WithErrorMessage(_i18n.Localizer["IdRequired"].Value);
    }

    [Fact]
    public void IsValidGuid_CustomKeys_WithInvalidGuid_ShouldFailWithInvalidMessage()
    {
        var validator = new TestCustomKeysGuidValidator(_i18n);
        var command = new TestCommand { Id = "not-a-guid" };

        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id).WithErrorMessage(_i18n.Localizer["IdInvalid"].Value);
    }

    #endregion

    #region IsValidGuid with custom keys — optional (isRequired: false)

    [Fact]
    public void IsValidGuid_CustomKeys_Optional_WithNullId_ShouldPass()
    {
        var validator = new TestCustomKeysOptionalGuidValidator(_i18n);
        var command = new TestCommand { Id = null };

        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void IsValidGuid_CustomKeys_Optional_WithInvalidGuid_ShouldFailWithInvalidMessage()
    {
        var validator = new TestCustomKeysOptionalGuidValidator(_i18n);
        var command = new TestCommand { Id = "not-a-guid" };

        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id).WithErrorMessage(_i18n.Localizer["IdInvalid"].Value);
    }

    #endregion
}
