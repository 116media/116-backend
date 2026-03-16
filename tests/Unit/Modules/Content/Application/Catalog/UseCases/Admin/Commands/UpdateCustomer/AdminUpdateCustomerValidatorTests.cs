using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCustomer;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCustomer;

/// <summary>
/// Unit tests for <see cref="AdminUpdateCustomerValidator"/>.
/// </summary>
public class AdminUpdateCustomerValidatorTests
{
    private readonly AdminUpdateCustomerValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateCustomerCommand(
            Id: Guid.NewGuid().ToString(),
            FullName: TestConstants.Content.Customer.ValidFullName,
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
                e.PropertyName == nameof(AdminUpdateCustomerCommand.Id) && e.ErrorMessage == "Customer ID is required."
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
                && e.ErrorMessage == "Customer full name is required."
            );
    }

    #endregion
}
