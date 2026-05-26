using _116.Content.Application.Shared.Errors.Messages;
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
    /// Initializes a new instance of <see cref="AdminAddPackageSlotValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Package validation error messages.</param>
    public AdminAddPackageSlotValidator(PackageErrorMessage i18n)
    {
        RuleFor(x => x.PackageId).IsValidGuid(i18n.Localizer);
        RuleFor(x => x.Quantity).ValidSlotQuantity(i18n);
    }
}
