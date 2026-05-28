using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Enums;
using FluentValidation;

namespace _116.Identity.Application.Auth.Validators;

/// <summary>
/// FluentValidation extensions for social login validation (avatar URL, auth provider).
/// </summary>
public static class SocialLoginValidation
{
    /// <summary>
    /// Validates an optional avatar URL format.
    /// Allows null/empty values but validates URL format when provided.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the avatar URL property.</param>
    /// <param name="i18n">Validation error messages for rule configuration.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidAvatarUrl<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        ValidationErrorMessage i18n
    )
    {
        return ruleBuilder.Must(predicate: ValidationUtils.ValidUrl).WithMessage(i18n.AvatarUrlInvalid());
    }

    /// <summary>
    /// Validates that the auth provider is a valid enum value (e.g., Google, Facebook).
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the auth provider property.</param>
    /// <param name="i18n">Validation error messages for rule configuration.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string> ValidAuthProvider<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder,
        ValidationErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.AuthProviderRequired())
            .Must(provider => provider != null && Enum.IsDefined(typeof(EnumAuthProvider), value: provider))
            .WithMessage(i18n.AuthProviderInvalid());
    }
}
