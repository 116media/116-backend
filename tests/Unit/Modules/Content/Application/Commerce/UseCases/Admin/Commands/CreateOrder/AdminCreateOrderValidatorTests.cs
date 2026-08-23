using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;

/// <summary>
/// Unit tests for <see cref="AdminCreateOrderValidator"/>.
/// </summary>
public class AdminCreateOrderValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminCreateOrderValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateOrderValidatorTests"/>.
    /// </summary>
    public AdminCreateOrderValidatorTests()
    {
        _validator = new AdminCreateOrderValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateOrderCommand(CustomerId: Guid.NewGuid().ToString(), PackageId: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region CustomerId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyCustomerId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateOrderCommand(CustomerId: string.Empty, PackageId: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateOrderCommand.CustomerId)
                && e.ErrorMessage == _i18n.Customer.Msg.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidCustomerId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateOrderCommand(CustomerId: "not-a-guid", PackageId: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateOrderCommand.CustomerId)
                && e.ErrorMessage == _i18n.Customer.Msg.Localizer["IdInvalid"].Value
            );
    }

    #endregion
}
