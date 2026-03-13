using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot;

/// <summary>
/// Validator for the <see cref="RemovePackageSlotCommand" /> ensuring valid IDs are provided.
/// </summary>
public class RemovePackageSlotValidator : AbstractValidator<RemovePackageSlotCommand>
{
    /// <summary>
    /// Configures validation rules for package slot removal.
    /// </summary>
    public RemovePackageSlotValidator()
    {
        RuleFor(x => x.PackageId).IsValidGuid("Package ID");
    }
}
