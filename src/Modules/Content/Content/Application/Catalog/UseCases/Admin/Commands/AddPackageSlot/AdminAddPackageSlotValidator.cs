using _116.Content.Application.Shared.Errors.Facade;
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
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminAddPackageSlotValidator(ContentI18n i18n)
    {
        RuleFor(x => x.PackageId).IsValidGuid(i18n.Package.Msg.Localizer);
        RuleFor(x => x.Quantity).ValidSlotQuantity(i18n.Package.Msg);
    }
}
