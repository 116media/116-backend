using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById;

/// <summary>
/// Validator for the <see cref="AdminGetPackageByIdQuery" /> ensuring a valid package ID is provided.
/// </summary>
public class AdminGetPackageByIdValidator : AbstractValidator<AdminGetPackageByIdQuery>
{
    /// <summary>
    /// Configures validation rules for package retrieval by ID.
    /// </summary>
    public AdminGetPackageByIdValidator()
    {
        RuleFor(x => x.Id).IsValidGuid("Package ID");
    }
}
