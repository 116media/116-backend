using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot;

/// <summary>
/// Validator for the <see cref="AdminRemovePackageSlotCommand" /> ensuring valid IDs are provided.
/// </summary>
public class AdminRemovePackageSlotValidator : AbstractValidator<AdminRemovePackageSlotCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminRemovePackageSlotValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Package validation error messages.</param>
    public AdminRemovePackageSlotValidator(PackageErrorMessage i18n)
    {
        RuleFor(x => x.PackageId).IsValidGuid(i18n.Localizer);
        RuleFor(x => x.SlotId).IsValidGuid(i18n.Localizer, "SlotIdRequired", "SlotIdInvalid");
    }
}
