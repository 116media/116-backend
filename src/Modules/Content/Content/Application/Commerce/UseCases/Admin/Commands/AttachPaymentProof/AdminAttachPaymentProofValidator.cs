using _116.Content.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminAttachPaymentProofValidator(ContentI18n i18n)
    {
        RuleFor(x => x.OrderId).IsValidGuid(i18n.ContentOrder.Msg.Localizer);
        RuleFor(x => x.File).ValidPaymentProofFile(i18n.ContentOrder.Msg);
        RuleFor(x => x.PaymentMethod).ValidPaymentMethod(i18n.ContentOrder.Msg);
    }
}
