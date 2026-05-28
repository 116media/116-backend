using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCustomer;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCustomer;

/// <summary>
/// Unit tests for <see cref="AdminUpdateCustomerValidator"/>.
/// </summary>
public class AdminUpdateCustomerValidatorTests
{
    private readonly CustomerErrorMessage _i18n = LocalizerFactory.CreateMessage<CustomerErrorMessage>();
    private readonly AdminUpdateCustomerValidator _validator;

    public AdminUpdateCustomerValidatorTests()
    {
        _validator = new AdminUpdateCustomerValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateCustomerCommand(
            Id: Guid.NewGuid().ToString(),
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
    public async Task Validate_WithOptionalFieldsNull_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateCustomerCommand(
            Id: Guid.NewGuid().ToString(),
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

    #region Id Validation Tests

    [Fact]
    public async Task Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateCustomerCommand(
            Id: "",
            FullName: TestConstants.Content.Customer.ValidFullName,
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
                e.PropertyName == nameof(AdminUpdateCustomerCommand.Id)
                && e.ErrorMessage == _i18n.Localizer["IdRequired"].Value
            );
    }

    #endregion

    #region FullName Validation Tests

    [Fact]
    public async Task Validate_WithEmptyFullName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateCustomerCommand(
            Id: Guid.NewGuid().ToString(),
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
                e.PropertyName == nameof(AdminUpdateCustomerCommand.FullName)
                && e.ErrorMessage == _i18n.FullNameRequired()
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
        var validator = new AdminUpdateCustomerValidator(i18n);
        var command = new AdminUpdateCustomerCommand(
            Id: "",
            FullName: TestConstants.Content.Customer.ValidFullName,
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
                e.PropertyName == nameof(AdminUpdateCustomerCommand.Id)
                && e.ErrorMessage == i18n.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
