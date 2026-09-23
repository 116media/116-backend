using _116.Content.Application.Shared.Messages;
using _116.Mailer.Contracts.Application.Messages;

namespace _116.Content.Application.Commerce.Messages;

/// <summary>
/// A payment was verified: the receipt confirming the order is now paid.
/// </summary>
/// <param name="Customer">The customer the order belongs to.</param>
/// <param name="OrderReference">The customer-facing order reference.</param>
/// <param name="AmountUsd">The amount received, formatted to two decimals.</param>
/// <param name="ReceiptUrl">Where the stored proof of payment can be seen.</param>
/// <param name="PaidAt">When the payment was verified.</param>
public record PaymentReceiptMessage(
    MessageRecipient Customer,
    string OrderReference,
    string AmountUsd,
    string ReceiptUrl,
    DateTimeOffset PaidAt
) : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => ContentMessageTemplates.PaymentReceipt;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [Customer];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["customerName"] = Customer.DisplayName ?? string.Empty,
            ["orderReference"] = OrderReference,
            ["amountUsd"] = AmountUsd,
            ["receiptUrl"] = ReceiptUrl,
            ["paidAt"] = PaidAt.ToString("u"),
        };
}
