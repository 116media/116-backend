using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer;

/// <summary>
/// Unit tests for <see cref="AdminCreateCustomerValidator"/>.
/// </summary>
public class AdminCreateCustomerValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminCreateCustomerValidator _validator;

    public AdminCreateCustomerValidatorTests()
    {
        _validator = new AdminCreateCustomerValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Customer.ValidFullName,
            Email: TestConstants.Customer.ValidEmail,
            Phone: TestConstants.Customer.ValidPhone,
            Company: TestConstants.Customer.ValidCompany,
            Notes: TestConstants.Customer.ValidNotes
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithMinimalData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Customer.ValidFullName,
            Email: TestConstants.Customer.ValidEmail,
            Phone: null,
            Company: null,
            Notes: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region FullName Validation Tests

    [Fact]
    public async Task Validate_WithEmptyFullName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCustomerCommand(
            FullName: string.Empty,
            Email: TestConstants.Customer.ValidEmail,
            Phone: null,
            Company: null,
            Notes: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCustomerCommand.FullName)
                && e.ErrorMessage == _i18n.Customer.Msg.FullNameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithFullNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCustomerCommand(
            FullName: new string('a', TestConstants.Customer.FullNameMaxLength + 1),
            Email: TestConstants.Customer.ValidEmail,
            Phone: null,
            Company: null,
            Notes: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCustomerCommand.FullName)
                && e.ErrorMessage == _i18n.Customer.Msg.FullNameTooLong(TestConstants.Customer.FullNameMaxLength)
            );
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public async Task Validate_WithEmptyEmail_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Customer.ValidFullName,
            Email: string.Empty,
            Phone: null,
            Company: null,
            Notes: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCustomerCommand.Email)
                && e.ErrorMessage == _i18n.Customer.Msg.EmailRequired()
            );
    }

    [Fact]
    public async Task Validate_WithInvalidEmailFormat_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Customer.ValidFullName,
            Email: "not-email",
            Phone: null,
            Company: null,
            Notes: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCustomerCommand.Email)
                && e.ErrorMessage == _i18n.Customer.Msg.EmailInvalidFormat()
            );
    }

    [Fact]
    public async Task Validate_WithEmailExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Customer.ValidFullName,
            Email: new string('a', TestConstants.Customer.EmailMaxLength + 1),
            Phone: null,
            Company: null,
            Notes: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminCreateCustomerCommand.Email));
    }

    #endregion

    #region Optional Fields Validation Tests

    [Fact]
    public async Task Validate_WithPhoneExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Customer.ValidFullName,
            Email: TestConstants.Customer.ValidEmail,
            Phone: new string('1', TestConstants.Customer.PhoneMaxLength + 1),
            Company: null,
            Notes: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCustomerCommand.Phone)
                && e.ErrorMessage == _i18n.Customer.Msg.PhoneTooLong(TestConstants.Customer.PhoneMaxLength)
            );
    }

    [Fact]
    public async Task Validate_WithCompanyExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Customer.ValidFullName,
            Email: TestConstants.Customer.ValidEmail,
            Phone: null,
            Company: new string('c', TestConstants.Customer.CompanyMaxLength + 1),
            Notes: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCustomerCommand.Company)
                && e.ErrorMessage == _i18n.Customer.Msg.CompanyTooLong(TestConstants.Customer.CompanyMaxLength)
            );
    }

    [Fact]
    public async Task Validate_WithNotesExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Customer.ValidFullName,
            Email: TestConstants.Customer.ValidEmail,
            Phone: null,
            Company: null,
            Notes: new string('n', TestConstants.Customer.NotesMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCustomerCommand.Notes)
                && e.ErrorMessage == _i18n.Customer.Msg.NotesTooLong(TestConstants.Customer.NotesMaxLength)
            );
    }

    #endregion
}
