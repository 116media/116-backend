using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;

/// <summary>
/// Validator for the <see cref="AdminAddItemTierCommand" />.
/// </summary>
public class AdminAddItemTierValidator : AbstractValidator<AdminAddItemTierCommand>
{
    /// <summary>
    /// Configures validation rules for adding an item tier.
    /// </summary>
    public AdminAddItemTierValidator()
    {
        RuleFor(x => x.OrderId).IsValidGuid("Order ID");
        RuleFor(x => x.OrderItemId).IsValidGuid("Order item ID");
        RuleFor(x => x.PricingTierId).IsValidGuid("Pricing tier ID");
    }
}
