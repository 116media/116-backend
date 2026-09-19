using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RejectPayment;

/// <summary>
/// Validator for the <see cref="AdminRejectPaymentCommand" />.
/// </summary>
public class AdminRejectPaymentValidator : AbstractValidator<AdminRejectPaymentCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminRejectPaymentValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminRejectPaymentValidator(ContentI18n i18n)
    {
        RuleFor(x => x.OrderId).IsValidGuid(i18n.ContentOrder.Msg.Localizer);

        RuleFor(x => x.Notes).ValidPaymentNotes(i18n.ContentOrder.Msg);
    }
}
