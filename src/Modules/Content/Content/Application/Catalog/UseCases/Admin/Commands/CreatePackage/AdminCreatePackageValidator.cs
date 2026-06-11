using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Validators;
using _116.Content.Domain.Constants;
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
    /// <param name="msg">Package validation error messages.</param>
    public AdminCreatePackageValidator(PackageErrorMessage msg)
    {
        RuleFor(x => x.Name)
            .ValidPackageName(
                nameRequired: msg.NameRequired(),
                nameTooLong: msg.NameTooLong(ContentConstants.MaxPackageNameLength)
            );
        RuleFor(x => x.Description)
            .ValidPackageDescription(
                descriptionRequired: msg.DescriptionRequired(),
                descriptionTooLong: msg.DescriptionTooLong(ContentConstants.MaxPackageDescriptionLength)
            );
    }
}
