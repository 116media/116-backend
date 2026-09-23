using _116.Mailer.Contracts.Application.Messages;

namespace _116.Identity.Application.Shared.Messages;

/// <summary>
/// A one-time code was issued: the code itself, delivered to the account that requested it.
/// The purpose selects the template, so both verification and reset flows share this record.
/// </summary>
/// <param name="User">The account the code was issued for.</param>
/// <param name="Template">The template the issuing purpose maps to.</param>
/// <param name="PlainCode">The code as the recipient must type it.</param>
/// <param name="ExpiryMinutes">How long the code stays valid.</param>
public record OtpIssuedMessage(MessageRecipient User, string Template, string PlainCode, int ExpiryMinutes) : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => Template;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [User];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["userName"] = User.DisplayName ?? string.Empty,
            ["otpCode"] = PlainCode,
            ["expiryMinutes"] = ExpiryMinutes.ToString(),
        };
}
