using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;

/// <summary>
/// Validator for the <see cref="AdminCreateOrderCommand" />.
/// </summary>
public class AdminCreateOrderValidator : AbstractValidator<AdminCreateOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateOrderValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminCreateOrderValidator(ContentI18n i18n)
    {
        RuleFor(x => x.CustomerId).IsValidGuid(i18n.Customer.Msg.Localizer);
    }
}
