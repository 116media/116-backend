using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier;

/// <summary>
/// Validator for the <see cref="AdminRemoveItemTierCommand" />.
/// </summary>
public class AdminRemoveItemTierValidator : AbstractValidator<AdminRemoveItemTierCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminRemoveItemTierValidator" /> with the specified error message providers.
    /// </summary>
    /// <param name="orderMsg">Content order validation error messages.</param>
    /// <param name="pricingTierMsg">Pricing tier validation error messages.</param>
    public AdminRemoveItemTierValidator(ContentOrderErrorMessage orderMsg, PricingTierErrorMessage pricingTierMsg)
    {
        RuleFor(x => x.OrderId).IsValidGuid(orderMsg.Localizer);
        RuleFor(x => x.ItemId).IsValidGuid(orderMsg.Localizer, "ItemIdRequired", "ItemIdInvalid");
        RuleFor(x => x.TierId).IsValidGuid(pricingTierMsg.Localizer);
    }
}
