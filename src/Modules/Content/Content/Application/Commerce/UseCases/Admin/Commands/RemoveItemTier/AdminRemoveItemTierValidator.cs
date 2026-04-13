using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier;

/// <summary>
/// Validator for the <see cref="AdminRemoveItemTierCommand" />.
/// </summary>
public class AdminRemoveItemTierValidator : AbstractValidator<AdminRemoveItemTierCommand>
{
    /// <summary>
    /// Configures validation rules for removing an item tier.
    /// </summary>
    public AdminRemoveItemTierValidator()
    {
        RuleFor(x => x.OrderId).IsValidGuid("Order ID");
        RuleFor(x => x.ItemId).IsValidGuid("Item ID");
        RuleFor(x => x.TierId).IsValidGuid("Tier ID");
    }
}
