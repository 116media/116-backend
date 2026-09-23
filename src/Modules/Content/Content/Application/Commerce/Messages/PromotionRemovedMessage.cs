using _116.Content.Application.Shared.Messages;
using _116.Mailer.Contracts.Application.Messages;

namespace _116.Content.Application.Commerce.Messages;

/// <summary>
/// Promoted content was taken down early: which content, and on what grounds.
/// </summary>
/// <param name="Customer">The customer the order belongs to.</param>
/// <param name="ContentTitle">The content that was unpromoted.</param>
/// <param name="Reason">Why the promotion was removed.</param>
/// <param name="RemovedAt">When the promotion was removed.</param>
public record PromotionRemovedMessage(
    MessageRecipient Customer,
    string ContentTitle,
    string Reason,
    DateTimeOffset RemovedAt
) : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => ContentMessageTemplates.PromotionForceRemoved;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [Customer];

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
