using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage;

/// <summary>
/// Validator for the <see cref="CreatePackageCommand" /> ensuring proper package data format.
/// </summary>
public class CreatePackageValidator : AbstractValidator<CreatePackageCommand>
{
    /// <summary>
    /// Configures validation rules for package creation.
    /// </summary>
    public CreatePackageValidator()
    {
        RuleFor(x => x.Name).ValidPackageName();
        RuleFor(x => x.Description).ValidPackageDescription();
        RuleFor(x => x.FlatPriceUsd).ValidFlatPriceUsd();
    }
}
