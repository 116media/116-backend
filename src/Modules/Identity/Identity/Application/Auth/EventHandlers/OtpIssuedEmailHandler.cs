using _116.BuildingBlocks.Constants;
using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Contracts.Domain;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Identity.Application.Auth.EventHandlers;

/// <summary>
/// Delivers a freshly issued one-time code by email. Reacting to the event rather than sending
/// inline means the code is queued only once the flow that issued it has committed, and the
/// event outbox re-delivers it if this handler dies.
/// </summary>
/// <param name="userLookupService">Lookup resolving the recipient's name and address by id.</param>
/// <param name="mailer">Outbox mailer sending the code.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class OtpIssuedEmailHandler(
    IUserLookupService userLookupService,
    IMailer mailer,
    ILogger<OtpIssuedEmailHandler> logger
) : IDomainEventHandler<OtpIssuedEvent>
{
    /// <inheritdoc />
    public async Task Handle(OtpIssuedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        EnumEmailTemplate? template = TemplateFor(domainEvent.Purpose);

        if (template is null)
        {
            logger.LogDebug("OTP delivery skipped: purpose {Purpose} has no email template.", domainEvent.Purpose);
            return;
        }

        AuthorInfo? user = await userLookupService.GetAuthorInfoByIdAsync(
            userId: domainEvent.UserId,
            ct: cancellationToken
        );

        if (user?.Email is null)
        {
            logger.LogDebug("OTP delivery skipped: user {UserId} has no email address.", domainEvent.UserId);
            return;
        }

        await mailer.EnqueueAsync(
            template: template.Value,
            to: new EmailRecipient(Address: user.Email, DisplayName: user.UserName),
            tokens: new Dictionary<string, string>
            {
                ["userName"] = user.UserName,
                ["otpCode"] = domainEvent.PlainCode,
                ["expiryMinutes"] = UserConstants.OtpExpirationMinutes.ToString(),
            },
            culture: domainEvent.Culture,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Maps a code's purpose to the template that delivers it.
    /// </summary>
    /// <param name="purpose">What the code authorises.</param>
    /// <returns>The template, or null when the purpose has no live delivery flow.</returns>
    private static EnumEmailTemplate? TemplateFor(EnumOtpPurpose purpose) =>
        purpose switch
        {
            EnumOtpPurpose.EmailVerification => EnumEmailTemplate.EmailVerificationOtp,
            EnumOtpPurpose.PasswordReset => EnumEmailTemplate.PasswordResetOtp,

            // TwoFactorAuthentication and AccountRecovery have no live flow and therefore no
            // template; the OTP row still rotates.
            _ => null,
        };
}
