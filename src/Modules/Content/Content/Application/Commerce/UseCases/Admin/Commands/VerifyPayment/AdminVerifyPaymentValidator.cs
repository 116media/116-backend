using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;

/// <summary>
/// Validator for the <see cref="AdminVerifyPaymentCommand" />.
/// </summary>
public class AdminVerifyPaymentValidator : AbstractValidator<AdminVerifyPaymentCommand>
{
    /// <summary>
    /// Configures validation rules for verifying a payment.
    /// </summary>
    public AdminVerifyPaymentValidator()
    {
        RuleFor(x => x.OrderId).IsValidGuid("Order ID");
        RuleFor(x => x.ReceiptUrl).ValidReceiptUrl();
        RuleFor(x => x.AdminUserId).NotEmpty().WithMessage("Admin user ID is required.");
    }
}
