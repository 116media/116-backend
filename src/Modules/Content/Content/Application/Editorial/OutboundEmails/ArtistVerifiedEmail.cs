using _116.Content.Application.Shared.OutboundEmails;
using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Content.Application.Editorial.OutboundEmails;

/// <summary>
/// An artist profile claim was verified: the claimant now owns the profile.
/// </summary>
/// <param name="Owner">The user whose claim was verified.</param>
/// <param name="ArtistName">The artist profile that was claimed.</param>
/// <param name="ArtistUrl">Where the profile can be seen.</param>
public record ArtistVerifiedEmail(EmailRecipient Owner, string ArtistName, string ArtistUrl) : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Notification;

    /// <inheritdoc />
    public override string TemplateName => ContentEmailTemplates.ArtistVerified;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [Owner];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["userName"] = Owner.DisplayName ?? string.Empty,
            ["artistName"] = ArtistName,
            ["artistUrl"] = ArtistUrl,
        };
}
