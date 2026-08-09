using _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;

/// <summary>
/// Unit tests for <see cref="AdminAttachPaymentProofValidator"/>.
/// </summary>
public class AdminAttachPaymentProofValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminAttachPaymentProofValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminAttachPaymentProofValidatorTests"/>.
    /// </summary>
    public AdminAttachPaymentProofValidatorTests()
    {
        _validator = new AdminAttachPaymentProofValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.FileName).Returns("proof.jpg");

        var command = new AdminAttachPaymentProofCommand(
            OrderId: Guid.NewGuid().ToString(),
            File: fileMock.Object,
            PaymentMethod: EnumPaymentMethod.BankTransfer
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
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

        var command = new AdminAttachPaymentProofCommand(
            OrderId: "not-a-guid",
            File: fileMock.Object,
            PaymentMethod: EnumPaymentMethod.BankTransfer
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAttachPaymentProofCommand.OrderId)
                && e.ErrorMessage == _i18n.ContentOrder.Msg.Localizer["IdInvalid"].Value
            );
    }

    #endregion

    #region File Validation Tests

    [Fact]
    public async Task Validate_WithNullFile_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAttachPaymentProofCommand(
            OrderId: Guid.NewGuid().ToString(),
            File: null!,
            PaymentMethod: EnumPaymentMethod.BankTransfer
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAttachPaymentProofCommand.File)
                && e.ErrorMessage == _i18n.ContentOrder.Msg.PaymentProofRequired()
            );
    }

    #endregion

    #region PaymentMethod Validation Tests

    [Fact]
    public async Task Validate_WithInvalidPaymentMethod_ShouldHaveError()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

        var command = new AdminAttachPaymentProofCommand(
            OrderId: Guid.NewGuid().ToString(),
            File: fileMock.Object,
            PaymentMethod: (EnumPaymentMethod)999
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAttachPaymentProofCommand.PaymentMethod)
                && e.ErrorMessage == _i18n.ContentOrder.Msg.InvalidPaymentMethod()
            );
    }

    #endregion
}
