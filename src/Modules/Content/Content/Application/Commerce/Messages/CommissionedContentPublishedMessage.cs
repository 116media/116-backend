using _116.Content.Application.Shared.Messages;
using _116.Mailer.Contracts.Application.Messages;

namespace _116.Content.Application.Commerce.Messages;

/// <summary>
/// Commissioned content went live: which content, and where to read it.
/// </summary>
/// <param name="Customer">The customer the order belongs to.</param>
/// <param name="ContentTitle">The content that was published.</param>
/// <param name="PublicUrl">Where the published content can be read.</param>
public record CommissionedContentPublishedMessage(MessageRecipient Customer, string ContentTitle, string PublicUrl)
    : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => ContentMessageTemplates.CommissionedContentPublished;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [Customer];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["customerName"] = Customer.DisplayName ?? string.Empty,
            ["contentTitle"] = ContentTitle,
            ["publicUrl"] = PublicUrl,
        };
}
