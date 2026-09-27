namespace _116.Mailer.Contracts.Application.OutboundEmails;

/// <summary>
/// A resolved recipient: who they are, where the message goes, and the locale it renders in
/// for them.
/// </summary>
/// <param name="UserId">The identity user, when the recipient is a known user.</param>
/// <param name="Address">The email address the message is delivered to.</param>
/// <param name="DisplayName">The name shown in the greeting and the To header.</param>
/// <param name="Locale">The locale this recipient's copy renders in.</param>
public record EmailRecipient(Guid? UserId, string Address, string? DisplayName, string Locale);
