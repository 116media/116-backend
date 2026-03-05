using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivatePackage;

/// <summary>
/// Validator for the <see cref="DeactivatePackageCommand" /> ensuring a valid package ID is provided.
/// </summary>
public class DeactivatePackageValidator : AbstractValidator<DeactivatePackageCommand>
{
    /// <summary>
    /// Configures validation rules for package deactivation.
    /// </summary>
    public DeactivatePackageValidator()
    {
        RuleFor(x => x.Id).ValidPackageId();
    }
}
