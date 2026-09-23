using _116.Content.Application.Shared.Messages;
using _116.Mailer.Contracts.Application.Messages;

namespace _116.Content.Application.Commerce.Messages;

/// <summary>
/// Commissioned content was rejected in review: which content, and on what grounds.
/// </summary>
/// <param name="Customer">The customer the order belongs to.</param>
/// <param name="ContentTitle">The content that was rejected.</param>
/// <param name="Reason">Why the content was rejected.</param>
public record CommissionedContentRejectedMessage(MessageRecipient Customer, string ContentTitle, string Reason)
    : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => ContentMessageTemplates.CommissionedContentRejected;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [Customer];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["customerName"] = Customer.DisplayName ?? string.Empty,
            ["contentTitle"] = ContentTitle,
            ["reason"] = Reason,
        };
}
