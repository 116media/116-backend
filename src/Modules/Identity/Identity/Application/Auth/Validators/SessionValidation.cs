using FluentValidation;

namespace _116.Identity.Application.Auth.Validators;

/// <summary>
/// FluentValidation extensions for session-related validation (refresh tokens).
/// </summary>
public static class SessionValidation
{
    /// <summary>
    /// Validates that a refresh token is provided and not empty.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the refresh token property.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> ValidRefreshToken<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
    {
        return ruleBuilder.NotEmpty().WithMessage("Refresh token is required.");
    }
}
