using _116.Content.Application.Shared.OutboundEmails;
using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Content.Application.Commerce.OutboundEmails;

/// <summary>
/// A payment was verified: the receipt confirming the order is now paid.
/// </summary>
/// <param name="Customer">The customer the order belongs to.</param>
/// <param name="OrderReference">The customer-facing order reference.</param>
/// <param name="AmountUsd">The amount received, formatted to two decimals.</param>
/// <param name="ReceiptUrl">Where the stored proof of payment can be seen.</param>
/// <param name="PaidAt">When the payment was verified.</param>
public record PaymentReceiptEmail(
    EmailRecipient Customer,
    string OrderReference,
    string AmountUsd,
    string ReceiptUrl,
    DateTimeOffset PaidAt
) : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => ContentEmailTemplates.PaymentReceipt;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [Customer];

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
