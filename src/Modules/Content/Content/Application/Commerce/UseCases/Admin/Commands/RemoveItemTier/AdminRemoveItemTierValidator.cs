using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier;

/// <summary>
/// Validator for the <see cref="AdminRemoveItemTierCommand" />.
/// </summary>
public class AdminRemoveItemTierValidator : AbstractValidator<AdminRemoveItemTierCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminRemoveItemTierValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminRemoveItemTierValidator(ContentI18n i18n)
    {
        RuleFor(x => x.OrderId).IsValidGuid(i18n.ContentOrder.Msg.Localizer);
        RuleFor(x => x.ItemId).IsValidGuid(i18n.ContentOrder.Msg.Localizer, "ItemIdRequired", "ItemIdInvalid");
        RuleFor(x => x.TierId).IsValidGuid(i18n.PricingTier.Msg.Localizer);
    }
}
