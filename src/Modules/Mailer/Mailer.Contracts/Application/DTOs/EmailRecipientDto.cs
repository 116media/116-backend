namespace _116.Mailer.Contracts.Application.DTOs;

/// <summary>
/// The destination of an email: address plus optional display name.
/// </summary>
/// <param name="Address">The recipient email address.</param>
/// <param name="DisplayName">The optional display name shown by mail clients.</param>
public record EmailRecipientDto(string Address, string? DisplayName = null, string Locale = "en");
