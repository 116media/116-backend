using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage;

/// <summary>
/// Validator for the <see cref="AdminCreatePackageCommand" /> ensuring proper package data format.
/// </summary>
public class AdminCreatePackageValidator : AbstractValidator<AdminCreatePackageCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreatePackageValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Package validation error messages.</param>
    public AdminCreatePackageValidator(PackageErrorMessage i18n)
    {
        RuleFor(x => x.Name).ValidPackageName(i18n);
        RuleFor(x => x.Description).ValidPackageDescription(i18n);
    }
}
