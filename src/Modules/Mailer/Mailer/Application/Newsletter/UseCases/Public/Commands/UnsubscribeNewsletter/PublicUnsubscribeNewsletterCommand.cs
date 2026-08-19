using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Commands.UnsubscribeNewsletter;

/// <summary>
/// Command for one-click newsletter unsubscription via the emailed token.
/// </summary>
/// <param name="Token">The unsubscribe token carried by the emailed link.</param>
/// <remarks>
/// Idempotent: re-clicking an already used link succeeds without side effects.
/// An unknown token resolves to a 404 problem.
/// </remarks>
public record PublicUnsubscribeNewsletterCommand(string Token) : ICommand<PublicUnsubscribeNewsletterResult>;

/// <summary>
/// Result of the <see cref="PublicUnsubscribeNewsletterCommand" />.
/// </summary>
/// <param name="IsUnsubscribed">Whether the subscriber is opted out after the call.</param>
public record PublicUnsubscribeNewsletterResult(bool IsUnsubscribed);
