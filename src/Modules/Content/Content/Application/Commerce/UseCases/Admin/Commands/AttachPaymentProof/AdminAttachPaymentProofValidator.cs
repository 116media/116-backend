using _116.Content.Application.Shared.Errors.Messages;
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
    /// Initializes a new instance of <see cref="AdminAttachPaymentProofValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="msg">Content order validation error messages.</param>
    public AdminAttachPaymentProofValidator(ContentOrderErrorMessage msg)
    {
        RuleFor(x => x.OrderId).IsValidGuid("Order ID");
        RuleFor(x => x.File).ValidPaymentProofFile(msg.PaymentProofRequired());
        RuleFor(x => x.PaymentMethod).ValidPaymentMethod(msg.InvalidPaymentMethod());
    }
}
