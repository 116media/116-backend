using _116.Mailer.Application.Shared.Persistence;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Mailer.Infrastructure.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="OutboxMailer" />: renders, persists a
/// self-contained pending row, and commits exactly once.
/// </summary>
public class OutboxMailerTests
{
    private readonly Mock<IEmailTemplateRenderer> _renderer = new();
    private readonly Mock<IOutboxEmailRepository> _repository = new();
    private readonly Mock<IMailerUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task EnqueueAsync_ShouldPersistTheRenderedEmailAndCommit()
    {
        _renderer
            .Setup(r => r.Render(EnumEmailTemplate.Welcome, It.IsAny<IReadOnlyDictionary<string, string>>(), "fr"))
            .Returns(new RenderedEmail("Bienvenue", "<p>Salut</p>", "Salut"));

        OutboxEmailEntity? captured = null;
        _repository
            .Setup(r => r.AddAsync(It.IsAny<OutboxEmailEntity>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxEmailEntity, CancellationToken>((e, _) => captured = e);

        var mailer = new OutboxMailer(_renderer.Object, _repository.Object, _unitOfWork.Object);

        await mailer.EnqueueAsync(
            template: EnumEmailTemplate.Welcome,
            to: new EmailRecipient("fan@example.com", "Fan"),
            tokens: new Dictionary<string, string> { ["userName"] = "Fan" },
            culture: "fr",
            cancellationToken: CancellationToken.None
        );

        captured.Should().NotBeNull();
        captured!.RecipientAddress.Should().Be("fan@example.com");
        captured.RecipientName.Should().Be("Fan");
        captured.Subject.Should().Be("Bienvenue");
        captured.HtmlBody.Should().Be("<p>Salut</p>");
        captured.TextBody.Should().Be("Salut");
        captured.Template.Should().Be(nameof(EnumEmailTemplate.Welcome));
        captured.Status.Should().Be(EnumOutboxEmailStatus.Pending);

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnqueueAsync_WhenRenderingThrows_ShouldPersistNothing()
    {
        _renderer
            .Setup(r =>
                r.Render(
                    It.IsAny<EnumEmailTemplate>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<string>()
                )
            )
            .Throws(new InvalidOperationException("unresolved placeholder"));

        var mailer = new OutboxMailer(_renderer.Object, _repository.Object, _unitOfWork.Object);

        Func<Task> act = () =>
            mailer.EnqueueAsync(
                EnumEmailTemplate.Welcome,
                new EmailRecipient("fan@example.com"),
                new Dictionary<string, string>(),
                "en",
                CancellationToken.None
            );

        await act.Should().ThrowAsync<InvalidOperationException>();
        _repository.Verify(r => r.AddAsync(It.IsAny<OutboxEmailEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
