using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.OutboundEmails;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application.OutboundEmails;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Application.Auth.EventHandlers;

/// <summary>
/// Delivers a freshly issued one-time code by email. Reacting to the event rather than sending
/// inline means the code is queued only once the flow that issued it has committed, and the
/// event outbox re-delivers it if this handler dies.
/// </summary>
/// <param name="userLookupService">Lookup resolving the recipient's name and address by id.</param>
/// <param name="messageDispatcher">Dispatcher routing the message to its recipients.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class OtpIssuedEmailHandler(
    IUserLookupService userLookupService,
    IEmailDispatcher messageDispatcher,
    ILogger<OtpIssuedEmailHandler> logger
) : IDomainEventHandler<OtpIssuedEvent>
{
    /// <inheritdoc />
    public async Task Handle(OtpIssuedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        string? template = TemplateFor(domainEvent.Purpose);

        if (template is null)
        {
            logger.LogDebug("OTP delivery skipped: purpose {Purpose} has no email template.", domainEvent.Purpose);
            return;
        }

        AuthorDto? user = await userLookupService.GetAuthorInfoByIdAsync(
            userId: domainEvent.UserId,
            ct: cancellationToken
        );

        if (user?.Email is null)
        {
            logger.LogDebug("OTP delivery skipped: user {UserId} has no email address.", domainEvent.UserId);
            return;
        }

        var message = new OtpIssuedEmail(
            User: new EmailRecipient(
                UserId: domainEvent.UserId,
                Address: user.Email,
                DisplayName: user.UserName,
                Locale: user.PreferredLocale
            ),
            Template: template,
            PlainCode: domainEvent.PlainCode,
            ExpiryMinutes: UserConstants.OtpExpirationMinutes
        );

        await messageDispatcher.DispatchAsync(message: message, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Maps a code's purpose to the template that delivers it.
    /// </summary>
    /// <param name="purpose">What the code authorises.</param>
    /// <returns>The template, or null when the purpose has no live delivery flow.</returns>
    private static string? TemplateFor(EnumOtpPurpose purpose) =>
        purpose switch
        {
            EnumOtpPurpose.EmailVerification => IdentityEmailTemplates.EmailVerificationOtp,
            EnumOtpPurpose.PasswordReset => IdentityEmailTemplates.PasswordResetOtp,

            // TwoFactorAuthentication and AccountRecovery have no live flow and therefore no
            // template; the OTP row still rotates.
            _ => null,
        };
}
