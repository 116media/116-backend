using _116.Content.Application.Shared.Errors.Messages;
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
    /// Initializes a new instance of <see cref="AdminVerifyPaymentValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Content order validation error messages.</param>
    public AdminVerifyPaymentValidator(ContentOrderErrorMessage msg)
    {
        RuleFor(x => x.OrderId).IsValidGuid("Order ID");
        RuleFor(x => x.ReceiptUrl).ValidReceiptUrl(msg);
        RuleFor(x => x.AdminUserId).ValidAdminUserId(msg);
    }
}
