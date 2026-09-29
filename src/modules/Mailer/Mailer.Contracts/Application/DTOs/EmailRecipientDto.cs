namespace _116.Mailer.Contracts.Application.DTOs;

/// <summary>
/// The destination of an email: where it goes, the language it renders in, and the name
/// shown by mail clients.
/// </summary>
/// <param name="Address">The recipient email address.</param>
/// <param name="Locale">The locale this recipient's copy renders in.</param>
/// <param name="DisplayName">The optional display name shown by mail clients.</param>
public record EmailRecipientDto(string Address, string Locale, string? DisplayName = null);
