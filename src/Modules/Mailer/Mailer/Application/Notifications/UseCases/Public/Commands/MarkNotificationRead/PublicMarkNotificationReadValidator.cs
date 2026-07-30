using _116.Mailer.Application.Shared.Errors;
using FluentValidation;

namespace _116.Mailer.Application.Notifications.UseCases.Public.Commands.MarkNotificationRead;

/// <summary>
/// Validator for the <see cref="PublicMarkNotificationReadCommand" /> ensuring
/// a notification identifier is present.
/// </summary>
public class PublicMarkNotificationReadValidator : AbstractValidator<PublicMarkNotificationReadCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicMarkNotificationReadValidator" />
    /// with validation rules.
    /// </summary>
    /// <param name="errors">Notification error factory providing localized messages.</param>
    public PublicMarkNotificationReadValidator(NotificationErrors errors)
    {
        RuleFor(x => x.NotificationId).NotEmpty().WithMessage(errors.Msg.NotificationIdRequired());
    }
}
