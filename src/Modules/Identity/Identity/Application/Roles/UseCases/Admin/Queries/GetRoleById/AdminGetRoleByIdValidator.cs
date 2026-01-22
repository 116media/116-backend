using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetRoleById;

/// <summary>
/// Validator for <see cref="AdminGetRoleByIdQuery" /> that ensures valid GUIDs.
/// </summary>
public class AdminGetRoleByIdValidator : AbstractValidator<AdminGetRoleByIdQuery>
{
    /// <summary>
    /// Configure validation rules for the query properties.
    /// </summary>
    public AdminGetRoleByIdValidator()
    {
        RuleFor(x => x.RoleId).Cascade(CascadeMode.Stop).IsValidGuid("Role ID");
    }
}
