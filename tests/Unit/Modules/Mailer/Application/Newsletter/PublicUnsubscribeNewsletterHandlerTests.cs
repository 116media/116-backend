using _116.Mailer.Application.Newsletter.UseCases.Public.Commands.UnsubscribeNewsletter;
using _116.Mailer.Application.Shared.Errors;
using _116.Mailer.Application.Shared.Errors.Messages;
using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Newsletter;

/// <summary>
/// Unit tests for <see cref="PublicUnsubscribeNewsletterHandler" /> covering
/// the opt-out, the idempotent re-click, and the unknown token.
/// </summary>
public class PublicUnsubscribeNewsletterHandlerTests
{
    private readonly Mock<INewsletterRepository> _repository = new();
    private readonly Mock<IMailerUnitOfWork> _unitOfWork = new();
    private readonly NewsletterErrors _errors = new(LocalizerFactory.CreateMessage<NewsletterErrorMessage>());

    private PublicUnsubscribeNewsletterHandler Handler => new(_repository.Object, _unitOfWork.Object, _errors);

    [Fact]
    public async Task Handle_UnknownToken_ShouldThrowNotFound()
    {
        // Arrange
        _repository
            .Setup(r => r.GetByUnsubscribeTokenAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((NewsletterSubscriberEntity?)null);

        // Act
        var act = () => Handler.Handle(new PublicUnsubscribeNewsletterCommand("missing"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SubscribedRow_ShouldOptOutAndCommit()
    {
        // Arrange
        var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");
        subscriber.Confirm(DateTime.UtcNow);
        _repository
            .Setup(r => r.GetByUnsubscribeTokenAsync(subscriber.UnsubscribeToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriber);

        // Act
        PublicUnsubscribeNewsletterResult result = await Handler.Handle(
            new PublicUnsubscribeNewsletterCommand(subscriber.UnsubscribeToken),
            CancellationToken.None
        );

        // Assert
        result.IsUnsubscribed.Should().BeTrue();
        subscriber.Status.Should().Be(EnumNewsletterStatus.Unsubscribed);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReClick_ShouldChangeNothingAndStillReportUnsubscribed()
    {
        // Arrange
        var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");
        subscriber.Confirm(DateTime.UtcNow);
        subscriber.Unsubscribe(DateTime.UtcNow);
        _repository
            .Setup(r => r.GetByUnsubscribeTokenAsync(subscriber.UnsubscribeToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriber);

        // Act
        PublicUnsubscribeNewsletterResult result = await Handler.Handle(
            new PublicUnsubscribeNewsletterCommand(subscriber.UnsubscribeToken),
            CancellationToken.None
        );

        // Assert
        result.IsUnsubscribed.Should().BeTrue();
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
