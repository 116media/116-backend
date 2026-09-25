using _116.Content.Application.Shared.OutboundEmails;
using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Content.Application.Commerce.OutboundEmails;

/// <summary>
/// A shoot was booked for commissioned content: which content, and when.
/// </summary>
/// <param name="Customer">The customer the order belongs to.</param>
/// <param name="ContentTitle">The content the shoot produces.</param>
/// <param name="ShootDate">When the shoot takes place.</param>
public record ShootScheduledEmail(EmailRecipient Customer, string ContentTitle, DateTime ShootDate) : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => ContentEmailTemplates.ShootScheduled;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [Customer];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["customerName"] = Customer.DisplayName ?? string.Empty,
            ["contentTitle"] = ContentTitle,
            ["shootDate"] = ShootDate.ToString("u"),
        };
}
