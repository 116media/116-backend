using _116.Content.Application.Shared.Errors.Facade;
using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivatePackage;

/// <summary>
/// Validator for the <see cref="AdminDeactivatePackageCommand" /> ensuring a valid package ID is provided.
/// </summary>
public class AdminDeactivatePackageValidator : AbstractValidator<AdminDeactivatePackageCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminDeactivatePackageValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminDeactivatePackageValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Package.Msg.Localizer);
    }
}
