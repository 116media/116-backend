using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer;
using _116.Content.Application.Shared.Errors.Messages;
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
    private readonly CustomerErrorMessage _i18n = LocalizerFactory.CreateMessage<CustomerErrorMessage>();
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
            FullName: TestConstants.Content.Customer.ValidFullName,
            Email: TestConstants.Content.Customer.ValidEmail,
            Phone: TestConstants.Content.Customer.ValidPhone,
            Company: TestConstants.Content.Customer.ValidCompany,
            Notes: TestConstants.Content.Customer.ValidNotes
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
            FullName: TestConstants.Content.Customer.ValidFullName,
            Email: TestConstants.Content.Customer.ValidEmail,
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
            Email: TestConstants.Content.Customer.ValidEmail,
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
                && e.ErrorMessage == _i18n.FullNameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithFullNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCustomerCommand(
            FullName: new string('a', TestConstants.Content.Customer.FullNameMaxLength + 1),
            Email: TestConstants.Content.Customer.ValidEmail,
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
                && e.ErrorMessage == _i18n.FullNameTooLong(TestConstants.Content.Customer.FullNameMaxLength)
            );
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public async Task Validate_WithEmptyEmail_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Content.Customer.ValidFullName,
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
                e.PropertyName == nameof(AdminCreateCustomerCommand.Email) && e.ErrorMessage == _i18n.EmailRequired()
            );
    }

    [Fact]
    public async Task Validate_WithInvalidEmailFormat_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Content.Customer.ValidFullName,
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
                && e.ErrorMessage == _i18n.EmailInvalidFormat()
            );
    }

    [Fact]
    public async Task Validate_WithEmailExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Content.Customer.ValidFullName,
            Email: new string('a', TestConstants.Content.Customer.EmailMaxLength + 1),
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
            FullName: TestConstants.Content.Customer.ValidFullName,
            Email: TestConstants.Content.Customer.ValidEmail,
            Phone: new string('1', TestConstants.Content.Customer.PhoneMaxLength + 1),
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
                && e.ErrorMessage == _i18n.PhoneTooLong(TestConstants.Content.Customer.PhoneMaxLength)
            );
    }

    [Fact]
    public async Task Validate_WithCompanyExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Content.Customer.ValidFullName,
            Email: TestConstants.Content.Customer.ValidEmail,
            Phone: null,
            Company: new string('c', TestConstants.Content.Customer.CompanyMaxLength + 1),
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
                && e.ErrorMessage == _i18n.CompanyTooLong(TestConstants.Content.Customer.CompanyMaxLength)
            );
    }

    [Fact]
    public async Task Validate_WithNotesExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCustomerCommand(
            FullName: TestConstants.Content.Customer.ValidFullName,
            Email: TestConstants.Content.Customer.ValidEmail,
            Phone: null,
            Company: null,
            Notes: new string('n', TestConstants.Content.Customer.NotesMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCustomerCommand.Notes)
                && e.ErrorMessage == _i18n.NotesTooLong(TestConstants.Content.Customer.NotesMaxLength)
            );
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        var i18n = LocalizerFactory.CreateMessage<CustomerErrorMessage>(culture);
        var validator = new AdminCreateCustomerValidator(i18n);
        var command = new AdminCreateCustomerCommand(
            FullName: string.Empty,
            Email: TestConstants.Content.Customer.ValidEmail,
            Phone: null,
            Company: null,
            Notes: null
        );

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCustomerCommand.FullName)
                && e.ErrorMessage == i18n.FullNameRequired()
            );
    }

    #endregion
}
