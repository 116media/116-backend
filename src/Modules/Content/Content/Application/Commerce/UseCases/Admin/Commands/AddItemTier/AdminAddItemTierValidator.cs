using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;

/// <summary>
/// Validator for the <see cref="AdminAddItemTierCommand" />.
/// </summary>
public class AdminAddItemTierValidator : AbstractValidator<AdminAddItemTierCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminAddItemTierValidator" /> with the specified error message providers.
    /// </summary>
    /// <param name="orderMsg">Content order validation error messages.</param>
    /// <param name="pricingTierMsg">Pricing tier validation error messages.</param>
    public AdminAddItemTierValidator(ContentOrderErrorMessage orderMsg, PricingTierErrorMessage pricingTierMsg)
    {
        RuleFor(x => x.OrderId).IsValidGuid(orderMsg.Localizer);
        RuleFor(x => x.OrderItemId).IsValidGuid(orderMsg.Localizer, "OrderItemIdRequired", "OrderItemIdInvalid");
        RuleFor(x => x.PricingTierId).IsValidGuid(pricingTierMsg.Localizer);
    }
}
