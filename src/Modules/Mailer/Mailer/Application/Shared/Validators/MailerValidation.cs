using _116.Mailer.Application.Shared.Errors.Messages;
using _116.Mailer.Domain.Constants;
using FluentValidation;

namespace _116.Mailer.Application.Shared.Validators;

/// <summary>
/// Shared validation extension methods for Mailer use cases.
/// </summary>
public static class MailerValidation
{
    /// <summary>
    /// Validates a newsletter confirmation or unsubscribe token is present.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the token property.</param>
    /// <param name="i18n">The newsletter error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidNewsletterToken<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        NewsletterErrorMessage i18n
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(i18n.TokenInvalid());
    }

    /// <summary>
    /// Validates a newsletter subscriber email — required, well-formed, and within the column length.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the email property.</param>
    /// <param name="i18n">The newsletter error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, string?> ValidNewsletterEmail<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        NewsletterErrorMessage i18n
    )
    {
        return ruleBuilder
            .Cascade(cascadeMode: CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(i18n.EmailRequired())
            .EmailAddress()
            .WithMessage(i18n.EmailInvalid())
            .MaximumLength(maximumLength: MailerConstants.MaxSubscriberEmailLength)
            .WithMessage(i18n.EmailTooLong(MailerConstants.MaxSubscriberEmailLength));
    }

    /// <summary>
    /// Validates a notification identifier is present.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the notification id property.</param>
    /// <param name="i18n">The notification error message provider.</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, Guid> ValidNotificationId<T>(
        this IRuleBuilder<T, Guid> ruleBuilder,
        NotificationErrorMessage i18n
    )
    {
        return ruleBuilder.NotEmpty().WithMessage(i18n.NotificationIdRequired());
    }
}
