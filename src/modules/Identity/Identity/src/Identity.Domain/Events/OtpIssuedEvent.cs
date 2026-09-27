using _116.Identity.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Identity.Domain.Events;

/// <summary>
/// Raised when a one-time code is issued to a user and still needs delivering. The event is
/// written in the same save as the OTP row, so a flow that fails part-way delivers no code and a
/// flow that commits always delivers one.
/// </summary>
/// <param name="UserId">The user the code was issued to.</param>
/// <param name="PlainCode">The code to deliver; only the hash is stored on the OTP row.</param>
/// <param name="Purpose">What the code authorises, which selects the email template.</param>
/// The culture captured where the code was issued. Delivery runs in a fresh scope that no longer
/// carries the request's culture, so it has to travel with the event.
/// </param>
public record OtpIssuedEvent(Guid UserId, string PlainCode, EnumOtpPurpose Purpose) : DomainEvent;
