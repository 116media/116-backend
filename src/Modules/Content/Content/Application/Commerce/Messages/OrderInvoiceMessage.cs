using _116.Content.Application.Shared.Messages;
using _116.Mailer.Contracts.Application.Messages;

namespace _116.Content.Application.Commerce.Messages;

/// <summary>
/// An order was placed: the invoice listing what it covers and how it can be paid.
/// </summary>
/// <param name="Customer">The customer the order belongs to.</param>
/// <param name="OrderReference">The customer-facing order reference.</param>
/// <param name="AmountUsd">The order total, formatted to two decimals.</param>
/// <param name="PaymentMethods">The offline payment methods accepted.</param>
/// <param name="ItemSummary">What the order covers, in one line.</param>
public record OrderInvoiceMessage(
    MessageRecipient Customer,
    string OrderReference,
    string AmountUsd,
    string PaymentMethods,
    string ItemSummary
) : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => ContentMessageTemplates.OrderInvoice;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [Customer];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["customerName"] = Customer.DisplayName ?? string.Empty,
            ["orderReference"] = OrderReference,
            ["amountUsd"] = AmountUsd,
            ["paymentMethods"] = PaymentMethods,
            ["itemSummary"] = ItemSummary,
        };
}
