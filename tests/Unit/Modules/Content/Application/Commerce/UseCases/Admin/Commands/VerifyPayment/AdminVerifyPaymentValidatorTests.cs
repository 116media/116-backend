using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;

/// <summary>
/// Unit tests for <see cref="AdminVerifyPaymentValidator"/>.
/// </summary>
public class AdminVerifyPaymentValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminVerifyPaymentValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminVerifyPaymentValidatorTests"/>.
    /// </summary>
    public AdminVerifyPaymentValidatorTests()
    {
        _validator = new AdminVerifyPaymentValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminVerifyPaymentCommand(
            OrderId: Guid.NewGuid().ToString(),
            ReceiptUrl: TestConstants.Commerce.ValidReceiptUrl,
            AdminUserId: Guid.NewGuid()
        );

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
        var command = new AdminVerifyPaymentCommand(
            OrderId: "not-a-guid",
            ReceiptUrl: TestConstants.Commerce.ValidReceiptUrl,
            AdminUserId: Guid.NewGuid()
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminVerifyPaymentCommand.OrderId)
                && e.ErrorMessage == _i18n.ContentOrder.Msg.Localizer["IdInvalid"].Value
            );
    }

    #endregion

    #region ReceiptUrl Validation Tests

    [Fact]
    public async Task Validate_WithEmptyReceiptUrl_ShouldHaveError()
    {
        // Arrange
        var command = new AdminVerifyPaymentCommand(
            OrderId: Guid.NewGuid().ToString(),
            ReceiptUrl: string.Empty,
            AdminUserId: Guid.NewGuid()
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminVerifyPaymentCommand.ReceiptUrl)
                && e.ErrorMessage == _i18n.ContentOrder.Msg.ReceiptUrlRequired()
            );
    }

    #endregion

    #region AdminUserId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyAdminUserId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminVerifyPaymentCommand(
            OrderId: Guid.NewGuid().ToString(),
            ReceiptUrl: TestConstants.Commerce.ValidReceiptUrl,
            AdminUserId: Guid.Empty
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminVerifyPaymentCommand.AdminUserId)
                && e.ErrorMessage == _i18n.ContentOrder.Msg.AdminUserIdRequired()
            );
    }

    #endregion
}
