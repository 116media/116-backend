using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.User.UseCases.Admin.Queries.GetUserRoles;

/// <summary>
/// Validator for <see cref="AdminGetUserRolesQuery" /> that ensures valid GUIDs.
/// </summary>
public class AdminGetUserRolesValidator : AbstractValidator<AdminGetUserRolesQuery>
{
    /// <summary>
    /// Configure validation rules for the query properties.
    /// </summary>
    public AdminGetUserRolesValidator()
    {
        RuleFor(x => x.UserId).IsValidGuid("User ID");
    }
}
