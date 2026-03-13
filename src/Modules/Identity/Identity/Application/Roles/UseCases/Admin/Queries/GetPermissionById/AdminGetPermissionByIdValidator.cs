using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetPermissionById;

/// <summary>
/// Validator for <see cref="AdminGetPermissionByIdQuery" /> that ensures valid GUIDs.
/// </summary>
public class AdminGetPermissionByIdValidator : AbstractValidator<AdminGetPermissionByIdQuery>
{
    /// <summary>
    /// Configure validation rules for the query properties.
    /// </summary>
    public AdminGetPermissionByIdValidator()
    {
        RuleFor(x => x.PermissionId).IsValidGuid("Permission ID");
    }
}
