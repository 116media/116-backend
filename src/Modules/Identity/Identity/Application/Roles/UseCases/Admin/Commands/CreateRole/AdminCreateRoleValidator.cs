using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using FluentValidation;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole;

/// <summary>
/// Validator for the <see cref="AdminCreateRoleCommand" /> ensuring proper role data format.
/// </summary>
public class AdminCreateRoleValidator : AbstractValidator<AdminCreateRoleCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreateRoleValidator" /> with validation rules.
    /// </summary>
    /// <param name="msg">
    /// Validation error messages for rule configuration.
    /// </param>
    public AdminCreateRoleValidator(ValidationErrorMessage msg)
    {
        RuleFor(x => x.Name).ValidRoleName(msg);
        RuleFor(x => x.Description).ValidRoleDescription(msg);
    }
}
