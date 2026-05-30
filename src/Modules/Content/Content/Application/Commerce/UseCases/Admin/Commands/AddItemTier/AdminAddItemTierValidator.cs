using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;

/// <summary>
/// Validator for the <see cref="AdminAddItemTierCommand" />.
/// </summary>
public class AdminAddItemTierValidator : AbstractValidator<AdminAddItemTierCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminAddItemTierValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminAddItemTierValidator(ContentI18n i18n)
    {
        RuleFor(x => x.OrderId).IsValidGuid(i18n.ContentOrder.Msg.Localizer);
        RuleFor(x => x.OrderItemId)
            .IsValidGuid(i18n.ContentOrder.Msg.Localizer, "OrderItemIdRequired", "OrderItemIdInvalid");
        RuleFor(x => x.PricingTierId).IsValidGuid(i18n.PricingTier.Msg.Localizer);
    }
}
