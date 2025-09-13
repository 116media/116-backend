using FluentValidation;

namespace _116.User.Application.Admin.UseCases.Commands.Login;

/// <summary>
/// Validator for the <see cref="AdminLoginCommand"/> ensuring proper admin credential format.
/// </summary>
public class AdminLoginValidator : AbstractValidator<AdminLoginCommand>
{
    /// <summary>
    /// Configure validation rules for admin authentication.
    /// </summary>
    /// <remarks>
    /// Validates email and password presence for admin login attempts.
    /// </remarks>
    public AdminLoginValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password cannot be empty.");
    }
}

