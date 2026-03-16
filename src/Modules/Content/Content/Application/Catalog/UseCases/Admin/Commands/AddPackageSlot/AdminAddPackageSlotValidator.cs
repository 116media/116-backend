using _116.Content.Application.Shared.Validators;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;

/// <summary>
/// Validator for the <see cref="AdminAddPackageSlotCommand" /> ensuring proper slot data format.
/// </summary>
public class AdminAddPackageSlotValidator : AbstractValidator<AdminAddPackageSlotCommand>
{
    /// <summary>
    /// Configures validation rules for adding a package slot.
    /// </summary>
    public AdminAddPackageSlotValidator()
    {
        RuleFor(x => x.PackageId).IsValidGuid("Package ID");
        RuleFor(x => x.Quantity).ValidSlotQuantity();
    }
}
