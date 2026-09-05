using _116.Mailer.Application.Newsletter.UseCases.Public.Commands.SubscribeNewsletter;
using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Contracts.Domain;
using _116.Mailer.Domain.Entities;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Newsletter;

/// <summary>
/// Unit tests for <see cref="PublicSubscribeNewsletterHandler" /> covering the
/// three subscription states and the enumeration-proof neutral result.
/// </summary>
public class PublicSubscribeNewsletterHandlerTests
{
    private readonly Mock<INewsletterRepository> _repository = new();
    private readonly Mock<IMailerUnitOfWork> _unitOfWork = new();
    private readonly Mock<IMailer> _mailer = new();

    private PublicSubscribeNewsletterHandler Handler => new(_repository.Object, _unitOfWork.Object, _mailer.Object);

    [Fact]
    public async Task Handle_NewAddress_ShouldPersistPendingSubscriberAndSendConfirmation()
    {
        _repository
            .Setup(r => r.GetByEmailAsync("fan@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((NewsletterSubscriberEntity?)null);

        await Handler.Handle(new PublicSubscribeNewsletterCommand("fan@example.com"), CancellationToken.None);

        _repository.Verify(
            r => r.AddAsync(It.IsAny<NewsletterSubscriberEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mailer.Verify(
            m =>
                m.EnqueueAsync(
                    EnumEmailTemplate.NewsletterConfirm,
                    It.IsAny<EmailRecipient>(),
                    It.Is<IReadOnlyDictionary<string, string>>(t => t.ContainsKey("confirmUrl")),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_UnsubscribedAddress_ShouldReissueConfirmationWithAFreshToken()
    {
        var existing = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");
        existing.Confirm(DateTime.UtcNow);
        existing.Unsubscribe(DateTime.UtcNow);
        string oldToken = existing.ConfirmationToken;

        _repository
            .Setup(r => r.GetByEmailAsync("fan@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await Handler.Handle(new PublicSubscribeNewsletterCommand("fan@example.com"), CancellationToken.None);

        existing.ConfirmationToken.Should().NotBe(oldToken);
        _repository.Verify(
            r => r.AddAsync(It.IsAny<NewsletterSubscriberEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _mailer.Verify(
            m =>
                m.EnqueueAsync(
                    EnumEmailTemplate.NewsletterConfirm,
                    It.IsAny<EmailRecipient>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_AlreadySubscribed_ShouldChangeNothingAndStillSucceed()
    {
        var existing = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");
        existing.Confirm(DateTime.UtcNow);

        _repository
            .Setup(r => r.GetByEmailAsync("fan@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await Handler.Handle(new PublicSubscribeNewsletterCommand("fan@example.com"), CancellationToken.None);

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mailer.VerifyNoOtherCalls();
    }
}
