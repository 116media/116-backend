using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;

/// <summary>
/// Unit tests for <see cref="AdminEditOrderValidator"/>.
/// </summary>
public class AdminEditOrderValidatorTests
{
    private readonly ContentOrderErrorMessage _orderI18n = LocalizerFactory.CreateMessage<ContentOrderErrorMessage>();
    private readonly CustomerErrorMessage _customerI18n = LocalizerFactory.CreateMessage<CustomerErrorMessage>();
    private readonly AdminEditOrderValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminEditOrderValidatorTests"/>.
    /// </summary>
    public AdminEditOrderValidatorTests()
    {
        _validator = new AdminEditOrderValidator(_orderI18n, _customerI18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidDataAndCustomerId_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminEditOrderCommand(
            OrderId: Guid.NewGuid().ToString(),
            CustomerId: Guid.NewGuid().ToString(),
            PackageId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithNullCustomerId_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminEditOrderCommand(OrderId: Guid.NewGuid().ToString(), CustomerId: null, PackageId: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region OrderId Validation Tests

    [Fact]
    public async Task Validate_WithInvalidGuidOrderId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminEditOrderCommand(OrderId: "not-a-guid", CustomerId: null, PackageId: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminEditOrderCommand.OrderId)
                && e.ErrorMessage == _orderI18n.Localizer["IdInvalid"].Value
            );
    }

    [Fact]
    public async Task Validate_WithEmptyOrderId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminEditOrderCommand(OrderId: "", CustomerId: null, PackageId: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region CustomerId Validation Tests

    [Fact]
    public async Task Validate_WithInvalidGuidCustomerId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminEditOrderCommand(
            OrderId: Guid.NewGuid().ToString(),
            CustomerId: "not-a-guid",
            PackageId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminEditOrderCommand.CustomerId)
                && e.ErrorMessage == _customerI18n.Localizer["IdInvalid"].Value
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
        var orderI18n = LocalizerFactory.CreateMessage<ContentOrderErrorMessage>(culture);
        var customerI18n = LocalizerFactory.CreateMessage<CustomerErrorMessage>(culture);
        var validator = new AdminEditOrderValidator(orderI18n, customerI18n);
        var command = new AdminEditOrderCommand(OrderId: "not-a-guid", CustomerId: null, PackageId: null);

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminEditOrderCommand.OrderId)
                && e.ErrorMessage == orderI18n.Localizer["IdInvalid"].Value
            );
    }

    #endregion
}
