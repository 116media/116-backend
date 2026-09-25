using _116.Mailer.Application.Newsletter.Messages;
using _116.Mailer.Application.Newsletter.UseCases.Public.Commands.ConfirmNewsletter;
using _116.Mailer.Application.Shared.Errors;
using _116.Mailer.Application.Shared.Errors.Messages;
using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Contracts.Application.Messages;
using _116.Mailer.Contracts.Domain.Enums;
using _116.Mailer.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Newsletter;

/// <summary>
/// Unit tests for <see cref="PublicConfirmNewsletterHandler" /> covering the
/// first click, the no-op re-click, and the unknown token.
/// </summary>
public class PublicConfirmNewsletterHandlerTests
{
    private readonly Mock<INewsletterRepository> _repository = new();
    private readonly Mock<IMailerUnitOfWork> _unitOfWork = new();
    private readonly Mock<IMessageDispatcher> _mailer = new();
    private readonly NewsletterErrors _errors = new(LocalizerFactory.CreateMessage<NewsletterErrorMessage>());

    private PublicConfirmNewsletterHandler Handler =>
        new(_repository.Object, _unitOfWork.Object, _mailer.Object, _errors);

    [Fact]
    public async Task Handle_UnknownToken_ShouldThrowNotFound()
    {
        // Arrange
        _repository
            .Setup(r => r.GetByConfirmationTokenAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((NewsletterSubscriberEntity?)null);

        // Act
        var act = () => Handler.Handle(new PublicConfirmNewsletterCommand("missing"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mailer.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_PendingSubscriber_ShouldCommitAndSendTheWelcomeEmail()
    {
        // Arrange
        var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");
        _repository
            .Setup(r => r.GetByConfirmationTokenAsync(subscriber.ConfirmationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriber);

        // Act
        PublicConfirmNewsletterResult result = await Handler.Handle(
            new PublicConfirmNewsletterCommand(subscriber.ConfirmationToken),
            CancellationToken.None
        );

        // Assert
        result.IsSubscribed.Should().BeTrue();
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mailer.Verify(
            d =>
                d.DispatchAsync(
                    It.Is<Message>(msg =>
                        msg.TemplateName == NewsletterMessageTemplates.NewsletterWelcome
                        && msg.Class == EnumMessageClass.Subscription
                        && msg.Recipients[0].Address == "fan@example.com"
                        && msg.Tokens["unsubscribeUrl"].Contains(subscriber.UnsubscribeToken)
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ReClick_ShouldChangeNothingAndSendNoSecondWelcome()
    {
        // Arrange
        var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");
        subscriber.Confirm(DateTime.UtcNow);
        _repository
            .Setup(r => r.GetByConfirmationTokenAsync(subscriber.ConfirmationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriber);

        // Act
        PublicConfirmNewsletterResult result = await Handler.Handle(
            new PublicConfirmNewsletterCommand(subscriber.ConfirmationToken),
            CancellationToken.None
        );

        // Assert
        result.IsSubscribed.Should().BeTrue();
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mailer.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_UnsubscribedRow_ShouldReportNotSubscribed()
    {
        // Arrange
        var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");
        subscriber.Confirm(DateTime.UtcNow);
        subscriber.Unsubscribe(DateTime.UtcNow);
        _repository
            .Setup(r => r.GetByConfirmationTokenAsync(subscriber.ConfirmationToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriber);

        // Act
        PublicConfirmNewsletterResult result = await Handler.Handle(
            new PublicConfirmNewsletterCommand(subscriber.ConfirmationToken),
            CancellationToken.None
        );

        // Assert
        result.IsSubscribed.Should().BeFalse();
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mailer.VerifyNoOtherCalls();
    }
}
