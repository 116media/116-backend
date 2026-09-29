using _116.Content.Application.Shared.OutboundEmails;
using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Content.Application.Commerce.OutboundEmails;

/// <summary>
/// A payment proof was rejected: what was wrong, so the customer can send a correct one.
/// </summary>
/// <param name="Customer">The customer the order belongs to.</param>
/// <param name="OrderReference">The customer-facing order reference.</param>
/// <param name="Notes">Why the proof was rejected.</param>
public record PaymentRejectedEmail(EmailRecipient Customer, string OrderReference, string Notes) : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => ContentEmailTemplates.PaymentRejected;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [Customer];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["customerName"] = Customer.DisplayName ?? string.Empty,
            ["orderReference"] = OrderReference,
            ["notes"] = Notes,
        };
}
