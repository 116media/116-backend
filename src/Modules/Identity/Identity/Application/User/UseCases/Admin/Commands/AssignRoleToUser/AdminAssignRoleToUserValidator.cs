using _116.Shared.Application.Extensions;
using FluentValidation;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser;

/// <summary>
/// Validator for the <see cref="AdminAssignRoleToUserCommand" /> ensuring a valid user ID.
/// </summary>
public class AdminAssignRoleToUserValidator : AbstractValidator<AdminAssignRoleToUserCommand>
{
    /// <summary>
    /// Configure validation rules for assigning a role to a user.
    /// </summary>
    public AdminAssignRoleToUserValidator()
    {
        RuleFor(x => x.UserId).IsValidGuid("User ID");
    }
}
