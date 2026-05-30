using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;

/// <summary>
/// Validator for the <see cref="AdminEditOrderCommand" />.
/// </summary>
public class AdminEditOrderValidator : AbstractValidator<AdminEditOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminEditOrderValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminEditOrderValidator(ContentI18n i18n)
    {
        RuleFor(x => x.OrderId).IsValidGuid(i18n.ContentOrder.Msg.Localizer);

        When(x => x.CustomerId is not null, () => RuleFor(x => x.CustomerId!).IsValidGuid(i18n.Customer.Msg.Localizer));
    }
}
