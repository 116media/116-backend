using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AttachPaymentProof;

/// <summary>
/// Validator for the <see cref="AdminAttachPaymentProofCommand" />.
/// </summary>
public class AdminAttachPaymentProofValidator : AbstractValidator<AdminAttachPaymentProofCommand>
{
    /// <summary>
    /// Configures validation rules for attaching a payment proof.
    /// </summary>
    public AdminAttachPaymentProofValidator()
    {
        RuleFor(x => x.OrderId).IsValidGuid("Order ID");
        RuleFor(x => x.File).ValidPaymentProofFile();
        RuleFor(x => x.PaymentMethod).IsInEnum().WithMessage("Payment method is invalid.");
    }
}
