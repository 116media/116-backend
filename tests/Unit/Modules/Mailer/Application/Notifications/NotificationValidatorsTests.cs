using _116.Mailer.Application.Notifications.UseCases.Public.Commands.MarkNotificationRead;
using _116.Mailer.Application.Shared.Errors;
using _116.Mailer.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Notifications;

/// <summary>
/// Unit tests for the notification command validators.
/// </summary>
public class NotificationValidatorsTests
{
    private static readonly NotificationErrors Errors = new(LocalizerFactory.CreateMessage<NotificationErrorMessage>());

    [Fact]
    public async Task MarkRead_EmptyNotificationId_ShouldFail()
    {
        var validator = new PublicMarkNotificationReadValidator(Errors);

        ValidationResult result = await validator.ValidateAsync(
            new PublicMarkNotificationReadCommand(Guid.NewGuid(), Guid.Empty)
        );

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task MarkRead_ValidNotificationId_ShouldPass()
    {
        var validator = new PublicMarkNotificationReadValidator(Errors);

        ValidationResult result = await validator.ValidateAsync(
            new PublicMarkNotificationReadCommand(Guid.NewGuid(), Guid.NewGuid())
        );

        result.IsValid.Should().BeTrue();
    }
}
