using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivatePackage;

/// <summary>
/// Validator for the <see cref="AdminActivatePackageCommand" /> ensuring a valid package ID is provided.
/// </summary>
public class AdminActivatePackageValidator : AbstractValidator<AdminActivatePackageCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminActivatePackageValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Package validation error messages.</param>
    public AdminActivatePackageValidator(PackageErrorMessage i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Localizer);
    }
}
