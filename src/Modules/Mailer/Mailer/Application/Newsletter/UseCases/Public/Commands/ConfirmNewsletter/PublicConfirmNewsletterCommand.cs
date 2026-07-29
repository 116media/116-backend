using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Commands.ConfirmNewsletter;

/// <summary>
/// Command for confirming a newsletter subscription via the emailed token.
/// </summary>
/// <param name="Token">The confirmation token carried by the emailed link.</param>
/// <remarks>
/// Idempotent: re-clicking an already used link succeeds without side effects.
/// An unknown token resolves to a 404 problem.
/// </remarks>
public record PublicConfirmNewsletterCommand(string Token) : ICommand<PublicConfirmNewsletterResult>;

/// <summary>
/// Result of the <see cref="PublicConfirmNewsletterCommand" />.
/// </summary>
/// <param name="IsSubscribed">Whether the subscriber is confirmed after the call.</param>
public record PublicConfirmNewsletterResult(bool IsSubscribed);
