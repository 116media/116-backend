using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot;

/// <summary>
/// Validator for the <see cref="AdminRemovePackageSlotCommand" /> ensuring valid IDs are provided.
/// </summary>
public class AdminRemovePackageSlotValidator : AbstractValidator<AdminRemovePackageSlotCommand>
{
    /// <summary>
    /// Configures validation rules for package slot removal.
    /// </summary>
    public AdminRemovePackageSlotValidator()
    {
        RuleFor(x => x.PackageId).IsValidGuid("Package ID");
    }
}
