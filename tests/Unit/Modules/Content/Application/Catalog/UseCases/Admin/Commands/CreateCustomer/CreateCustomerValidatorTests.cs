using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer;

/// <summary>
/// Unit tests for <see cref="CreateCustomerValidator"/>.
/// </summary>
public class CreateCustomerValidatorTests
{
    private readonly CreateCustomerValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateCustomerCommand(
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
        var command = new CreateCustomerCommand(
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
        var command = new CreateCustomerCommand(
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
                e.PropertyName == nameof(CreateCustomerCommand.FullName)
                && e.ErrorMessage == "Customer full name is required."
            );
    }

    [Fact]
    public async Task Validate_WithFullNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new CreateCustomerCommand(
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
                e.PropertyName == nameof(CreateCustomerCommand.FullName)
                && e.ErrorMessage == "Customer full name must not exceed 100 characters."
            );
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public async Task Validate_WithEmptyEmail_ShouldHaveError()
    {
        // Arrange
        var command = new CreateCustomerCommand(
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
                e.PropertyName == nameof(CreateCustomerCommand.Email) && e.ErrorMessage == "Customer email is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidEmailFormat_ShouldHaveError()
    {
        // Arrange
        var command = new CreateCustomerCommand(
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
                e.PropertyName == nameof(CreateCustomerCommand.Email)
                && e.ErrorMessage == "Customer email must be a valid email address."
            );
    }

    [Fact]
    public async Task Validate_WithEmailExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new CreateCustomerCommand(
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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCustomerCommand.Email));
    }

    #endregion

    #region Optional Fields Validation Tests

    [Fact]
    public async Task Validate_WithPhoneExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new CreateCustomerCommand(
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
                e.PropertyName == nameof(CreateCustomerCommand.Phone)
                && e.ErrorMessage == "Customer phone must not exceed 30 characters."
            );
    }

    [Fact]
    public async Task Validate_WithCompanyExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new CreateCustomerCommand(
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
                e.PropertyName == nameof(CreateCustomerCommand.Company)
                && e.ErrorMessage == "Customer company must not exceed 100 characters."
            );
    }

    [Fact]
    public async Task Validate_WithNotesExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new CreateCustomerCommand(
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
                e.PropertyName == nameof(CreateCustomerCommand.Notes)
                && e.ErrorMessage == "Customer notes must not exceed 500 characters."
            );
    }

    #endregion
}
