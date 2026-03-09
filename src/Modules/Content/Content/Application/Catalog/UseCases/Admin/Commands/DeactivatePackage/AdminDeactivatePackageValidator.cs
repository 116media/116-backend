using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivatePackage;

/// <summary>
/// Validator for the <see cref="AdminDeactivatePackageCommand" /> ensuring a valid package ID is provided.
/// </summary>
public class AdminDeactivatePackageValidator : AbstractValidator<AdminDeactivatePackageCommand>
{
    /// <summary>
    /// Configures validation rules for package deactivation.
    /// </summary>
    public AdminDeactivatePackageValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Package ID");
    }
}
