using _116.Content.Application.Shared.OutboundEmails;
using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Content.Application.Commerce.OutboundEmails;

/// <summary>
/// Promoted content was taken down early: which content, and on what grounds.
/// </summary>
/// <param name="Customer">The customer the order belongs to.</param>
/// <param name="ContentTitle">The content that was unpromoted.</param>
/// <param name="Reason">Why the promotion was removed.</param>
/// <param name="RemovedAt">When the promotion was removed.</param>
public record PromotionRemovedEmail(
    EmailRecipient Customer,
    string ContentTitle,
    string Reason,
    DateTimeOffset RemovedAt
) : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => ContentEmailTemplates.PromotionForceRemoved;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [Customer];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["customerName"] = Customer.DisplayName ?? string.Empty,
            ["contentTitle"] = ContentTitle,
            ["reason"] = Reason,
            ["removedAt"] = RemovedAt.ToString("u"),
        };
}
