using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivatePackage;

/// <summary>
/// Validator for the <see cref="ActivatePackageCommand" /> ensuring a valid package ID is provided.
/// </summary>
public class ActivatePackageValidator : AbstractValidator<ActivatePackageCommand>
{
    /// <summary>
    /// Configures validation rules for package activation.
    /// </summary>
    public ActivatePackageValidator()
    {
        RuleFor(x => x.Id).ValidPackageId();
    }
}
