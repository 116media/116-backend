using _116.Content.Application.Shared.Messages;
using _116.Mailer.Contracts.Application.Messages;

namespace _116.Content.Application.Commerce.Messages;

/// <summary>
/// A shoot was booked for commissioned content: which content, and when.
/// </summary>
/// <param name="Customer">The customer the order belongs to.</param>
/// <param name="ContentTitle">The content the shoot produces.</param>
/// <param name="ShootDate">When the shoot takes place.</param>
public record ShootScheduledMessage(MessageRecipient Customer, string ContentTitle, DateTime ShootDate) : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => ContentMessageTemplates.ShootScheduled;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [Customer];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["customerName"] = Customer.DisplayName ?? string.Empty,
            ["contentTitle"] = ContentTitle,
            ["shootDate"] = ShootDate.ToString("u"),
        };
}
