using _116.Content.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminVerifyPaymentValidator(ContentI18n i18n)
    {
        RuleFor(x => x.OrderId).IsValidGuid(i18n.ContentOrder.Msg.Localizer);
        RuleFor(x => x.ReceiptUrl).ValidReceiptUrl(i18n.ContentOrder.Msg);
        RuleFor(x => x.AdminUserId).ValidAdminUserId(i18n.ContentOrder.Msg);
    }
}
