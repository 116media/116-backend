using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.RemoveRoleFromUser;

/// <summary>
/// Validator for the <see cref="AdminRemoveRoleFromUserCommand" /> ensuring a valid user ID.
/// </summary>
public class AdminRemoveRoleFromUserValidator : AbstractValidator<AdminRemoveRoleFromUserCommand>
{
    /// <summary>
    /// Configure validation rules for removing a role from a user.
    /// </summary>
    public AdminRemoveRoleFromUserValidator()
    {
        RuleFor(x => x.UserId).IsValidGuid("User ID");
        RuleFor(x => x.RoleId).IsValidGuid("Role ID");
    }
}
