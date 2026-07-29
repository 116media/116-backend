using _116.Shared.Contracts.Application.CQRS;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Commands.SubscribeNewsletter;

/// <summary>
/// Command for signing an email address up to the newsletter with double opt-in.
/// </summary>
/// <param name="Email">The email address to subscribe.</param>
/// <remarks>
/// Always resolves successfully regardless of whether the address is already
/// subscribed, pending, or unknown — the neutral outcome prevents subscriber
/// enumeration. A confirmation email is (re)issued for every non-subscribed state.
/// </remarks>
public record PublicSubscribeNewsletterCommand(string Email) : ICommand<PublicSubscribeNewsletterResult>;

/// <summary>
/// Result of the <see cref="PublicSubscribeNewsletterCommand" />.
/// </summary>
/// <param name="IsSuccess">Always true, to prevent subscriber enumeration.</param>
/// <param name="Email">The email address from the request for client reference.</param>
public record PublicSubscribeNewsletterResult(bool IsSuccess, string Email);
