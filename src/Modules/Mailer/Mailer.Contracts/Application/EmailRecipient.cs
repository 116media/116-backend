namespace _116.Mailer.Contracts.Application;

/// <summary>
/// The destination of an email: address plus optional display name.
/// </summary>
/// <param name="Address">The recipient email address.</param>
/// <param name="DisplayName">The optional display name shown by mail clients.</param>
public record EmailRecipient(string Address, string? DisplayName = null);
