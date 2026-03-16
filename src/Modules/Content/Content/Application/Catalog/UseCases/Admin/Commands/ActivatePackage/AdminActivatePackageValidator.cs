using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivatePackage;

/// <summary>
/// Validator for the <see cref="AdminActivatePackageCommand" /> ensuring a valid package ID is provided.
/// </summary>
public class AdminActivatePackageValidator : AbstractValidator<AdminActivatePackageCommand>
{
    /// <summary>
    /// Configures validation rules for package activation.
    /// </summary>
    public AdminActivatePackageValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Package ID");
    }
}
