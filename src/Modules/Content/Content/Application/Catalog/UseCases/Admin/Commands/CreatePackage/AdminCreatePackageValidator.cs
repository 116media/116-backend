using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage;

/// <summary>
/// Validator for the <see cref="AdminCreatePackageCommand" /> ensuring proper package data format.
/// </summary>
public class AdminCreatePackageValidator : AbstractValidator<AdminCreatePackageCommand>
{
    /// <summary>
    /// Configures validation rules for package creation.
    /// </summary>
    public AdminCreatePackageValidator()
    {
        RuleFor(x => x.Name).ValidPackageName();
        RuleFor(x => x.Description).ValidPackageDescription();
        RuleFor(x => x.FlatPriceUsd).ValidFlatPriceUsd();
    }
}
