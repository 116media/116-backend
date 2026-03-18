using _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;
using _116.Content.Domain.Enums;
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
    private readonly AdminAttachPaymentProofValidator _validator = new();

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
                && e.ErrorMessage == "Order ID is invalid."
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
                && e.ErrorMessage == "Payment proof file is required."
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
                && e.ErrorMessage == "Payment method is invalid."
            );
    }

    #endregion
}
