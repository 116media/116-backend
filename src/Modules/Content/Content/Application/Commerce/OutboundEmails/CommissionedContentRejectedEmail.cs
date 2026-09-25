using _116.Content.Application.Shared.OutboundEmails;
using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Content.Application.Commerce.OutboundEmails;

/// <summary>
/// Commissioned content was rejected in review: which content, and on what grounds.
/// </summary>
/// <param name="Customer">The customer the order belongs to.</param>
/// <param name="ContentTitle">The content that was rejected.</param>
/// <param name="Reason">Why the content was rejected.</param>
public record CommissionedContentRejectedEmail(EmailRecipient Customer, string ContentTitle, string Reason)
    : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => ContentEmailTemplates.CommissionedContentRejected;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [Customer];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["customerName"] = Customer.DisplayName ?? string.Empty,
            ["contentTitle"] = ContentTitle,
            ["reason"] = Reason,
        };
}
