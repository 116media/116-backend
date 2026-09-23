using _116.Content.Application.Shared.Messages;
using _116.Mailer.Contracts.Application.Messages;

namespace _116.Content.Application.Commerce.Messages;

/// <summary>
/// A payment proof was rejected: what was wrong, so the customer can send a correct one.
/// </summary>
/// <param name="Customer">The customer the order belongs to.</param>
/// <param name="OrderReference">The customer-facing order reference.</param>
/// <param name="Notes">Why the proof was rejected.</param>
public record PaymentRejectedMessage(MessageRecipient Customer, string OrderReference, string Notes) : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => ContentMessageTemplates.PaymentRejected;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [Customer];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["customerName"] = Customer.DisplayName ?? string.Empty,
            ["orderReference"] = OrderReference,
            ["notes"] = Notes,
        };
}
