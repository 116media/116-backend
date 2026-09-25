using _116.Content.Application.Shared.OutboundEmails;
using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Content.Application.Commerce.OutboundEmails;

/// <summary>
/// An order was cancelled: the confirmation that nothing further is owed on it.
/// </summary>
/// <param name="Customer">The customer the order belongs to.</param>
/// <param name="OrderReference">The customer-facing order reference.</param>
public record OrderCancelledEmail(EmailRecipient Customer, string OrderReference) : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => ContentEmailTemplates.OrderCancelled;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [Customer];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["customerName"] = Customer.DisplayName ?? string.Empty,
            ["orderReference"] = OrderReference,
        };
}
