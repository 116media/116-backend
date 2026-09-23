using _116.Content.Application.Shared.Messages;
using _116.Mailer.Contracts.Application.Messages;

namespace _116.Content.Application.Editorial.Messages;

/// <summary>
/// An artist profile claim was verified: the claimant now owns the profile.
/// </summary>
/// <param name="Owner">The user whose claim was verified.</param>
/// <param name="ArtistName">The artist profile that was claimed.</param>
/// <param name="ArtistUrl">Where the profile can be seen.</param>
public record ArtistVerifiedMessage(MessageRecipient Owner, string ArtistName, string ArtistUrl) : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Notification;

    /// <inheritdoc />
    public override string TemplateName => ContentMessageTemplates.ArtistVerified;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [Owner];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["userName"] = Owner.DisplayName ?? string.Empty,
            ["artistName"] = ArtistName,
            ["artistUrl"] = ArtistUrl,
        };
}
