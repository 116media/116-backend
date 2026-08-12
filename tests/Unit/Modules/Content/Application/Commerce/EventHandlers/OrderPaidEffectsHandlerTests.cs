using _116.Content.Application.Commerce.EventHandlers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.EventHandlers;

/// <summary>
/// Unit tests for <see cref="OrderPaidEffectsHandler"/>.
/// </summary>
public class OrderPaidEffectsHandlerTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly OrderPaidEffectsHandler _handler;

    public OrderPaidEffectsHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new OrderPaidEffectsHandler(
            _articleRepositoryMock.Object,
            _videoRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            NullLogger<OrderPaidEffectsHandler>.Instance
        );
    }

    /// <summary>
    /// The instant every event built by <see cref="PaidEvent"/> was paid at.
    /// </summary>
    private static readonly DateTimeOffset PaidAt = DateTimeOffset.UtcNow.AddMinutes(-5);

    /// <summary>
    /// Builds an <see cref="OrderPaidEvent"/> around a single item effect.
    /// </summary>
    private static OrderPaidEvent PaidEvent(params PaidItemEffect[] effects)
    {
        return PaidEventAt(PaidAt, effects);
    }

    /// <summary>
    /// Builds an <see cref="OrderPaidEvent"/> paid at a specific instant.
    /// </summary>
    private static OrderPaidEvent PaidEventAt(DateTimeOffset paidAt, params PaidItemEffect[] effects)
    {
        return new OrderPaidEvent(OrderId: Guid.NewGuid(), PaymentId: Guid.NewGuid(), PaidAt: paidAt, Items: effects);
    }

    [Fact]
    public async Task Handle_WhenArticleFulfilsItem_ShouldStampEffectsAndCommit()
    {
        // Arrange
        Guid orderItemId = Guid.NewGuid();
        Guid promotionLevelId = Guid.NewGuid();
        DateTimeOffset promotionUntil = DateTimeOffset.UtcNow.AddDays(14);
        ArticleEntity article = ArticleFactory.Create(Guid.NewGuid());
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(orderItemId, article);

        // Act
        await _handler.Handle(
            PaidEvent(new PaidItemEffect(orderItemId, promotionLevelId, promotionUntil, SocialBoost: true)),
            CancellationToken.None
        );

        // Assert
        article.SocialBoost.Should().BeTrue();
        article.IsPromoted.Should().BeTrue();
        article.PromotionLevelId.Should().Be(promotionLevelId);
        article.Status.Should().Be(EnumContentStatus.PendingReview);
        _articleRepositoryMock.VerifyUpdateCalled(article);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldApplyPromotionWindowFromPayloadVerbatim()
    {
        // Arrange
        Guid orderItemId = Guid.NewGuid();
        DateTimeOffset promotionUntil = DateTimeOffset.UtcNow.AddDays(30);
        ArticleEntity article = ArticleFactory.Create(Guid.NewGuid());
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(orderItemId, article);

        // Act
        await _handler.Handle(
            PaidEvent(new PaidItemEffect(orderItemId, Guid.NewGuid(), promotionUntil, SocialBoost: false)),
            CancellationToken.None
        );

        // Assert
        article.PromotedUntil.Should().Be(promotionUntil);
    }

    [Fact]
    public async Task Handle_WhenArticleFound_ShouldNotQueryVideoOrLyrics()
    {
        // Arrange
        Guid orderItemId = Guid.NewGuid();
        ArticleEntity article = ArticleFactory.Create(Guid.NewGuid());
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(orderItemId, article);

        // Act
        await _handler.Handle(
            PaidEvent(new PaidItemEffect(orderItemId, null, null, SocialBoost: false)),
            CancellationToken.None
        );

        // Assert
        _videoRepositoryMock.Verify(
            x => x.GetByOrderItemIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _lyricsRepositoryMock.Verify(
            x => x.GetByOrderItemIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenVideoFulfilsItem_ShouldStampEffectsAndCommit()
    {
        // Arrange
        Guid orderItemId = Guid.NewGuid();
        VideoEntity video = VideoFactory.Create(Guid.NewGuid());
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(orderItemId, null);
        _videoRepositoryMock.SetupGetByOrderItemIdAsync(orderItemId, video);

        // Act
        await _handler.Handle(
            PaidEvent(new PaidItemEffect(orderItemId, null, null, SocialBoost: true)),
            CancellationToken.None
        );

        // Assert
        video.SocialBoost.Should().BeTrue();
        video.Status.Should().Be(EnumContentStatus.PendingReview);
        _videoRepositoryMock.VerifyUpdateCalled(video);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsFulfilItem_ShouldStampPromotionButNeverSocialBoost()
    {
        // Arrange
        Guid orderItemId = Guid.NewGuid();
        DateTimeOffset promotionUntil = DateTimeOffset.UtcNow.AddDays(7);
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(orderItemId, null);
        _videoRepositoryMock.SetupGetByOrderItemIdAsync(orderItemId, null);
        _lyricsRepositoryMock.SetupGetByOrderItemIdAsync(orderItemId, lyrics);

        // Act
        await _handler.Handle(
            PaidEvent(new PaidItemEffect(orderItemId, Guid.NewGuid(), promotionUntil, SocialBoost: true)),
            CancellationToken.None
        );

        // Assert
        lyrics.IsPromoted.Should().BeTrue();
        lyrics.PromotedUntil.Should().Be(promotionUntil);
        lyrics.Status.Should().Be(EnumContentStatus.PendingReview);
        _lyricsRepositoryMock.VerifyUpdateCalled(lyrics);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNoContentFulfilsItem_ShouldNotCommit()
    {
        // Arrange

        // Act
        await _handler.Handle(
            PaidEvent(new PaidItemEffect(Guid.NewGuid(), null, null, SocialBoost: false)),
            CancellationToken.None
        );

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenItemAlreadyStamped_ShouldBeANoOp()
    {
        // Arrange
        Guid orderItemId = Guid.NewGuid();
        Guid promotionLevelId = Guid.NewGuid();
        DateTimeOffset promotionUntil = DateTimeOffset.UtcNow.AddDays(14);
        ArticleEntity article = ArticleFactory.CreatePublished(Guid.NewGuid());
        article.StampSocialBoost();
        article.StampPromotion(promotionLevelId, promotionUntil);
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(orderItemId, article);

        // Act
        await _handler.Handle(
            PaidEvent(new PaidItemEffect(orderItemId, promotionLevelId, promotionUntil, SocialBoost: true)),
            CancellationToken.None
        );

        // Assert
        _articleRepositoryMock.Verify(x => x.Update(It.IsAny<ArticleEntity>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenArticleAlreadyPublished_ShouldStampPromotionWithoutDisturbingStatus()
    {
        // Arrange
        Guid orderItemId = Guid.NewGuid();
        DateTimeOffset promotionUntil = DateTimeOffset.UtcNow.AddDays(14);
        ArticleEntity article = ArticleFactory.CreatePublished(Guid.NewGuid());
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(orderItemId, article);

        // Act
        await _handler.Handle(
            PaidEvent(new PaidItemEffect(orderItemId, Guid.NewGuid(), promotionUntil, SocialBoost: false)),
            CancellationToken.None
        );

        // Assert
        article.IsPromoted.Should().BeTrue();
        article.Status.Should().Be(EnumContentStatus.Published);
        _articleRepositoryMock.VerifyUpdateCalled(article);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenPromotionForceRemovedAfterPayment_ShouldNotReStampIt()
    {
        // Arrange
        Guid orderItemId = Guid.NewGuid();
        Guid promotionLevelId = Guid.NewGuid();
        ArticleEntity article = ArticleFactory.CreatePublished(Guid.NewGuid());
        article.StampPromotion(promotionLevelId, PaidAt.AddDays(14));
        article.ForceUnpromote(
            unpromotedBy: "super-admin-uuid",
            reason: "policy violation",
            errors: TestErrorsFactory.CreateArticleErrors()
        );
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(orderItemId, article);

        // Act
        await _handler.Handle(
            PaidEvent(new PaidItemEffect(orderItemId, promotionLevelId, PaidAt.AddDays(14), SocialBoost: false)),
            CancellationToken.None
        );

        // Assert
        article.IsPromoted.Should().BeFalse();
        article.PromotedUntil.Should().BeNull();
        article.PromotionLevelId.Should().BeNull();
        article.UnpromotedReason.Should().Be("policy violation");
        _articleRepositoryMock.Verify(x => x.Update(It.IsAny<ArticleEntity>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPromotionForceRemovedBeforeThisPayment_ShouldStampTheNewlyPurchasedPromotion()
    {
        // Arrange
        Guid orderItemId = Guid.NewGuid();
        Guid promotionLevelId = Guid.NewGuid();
        DateTimeOffset laterPaidAt = DateTimeOffset.UtcNow.AddMinutes(5);
        DateTimeOffset promotionUntil = laterPaidAt.AddDays(14);
        ArticleEntity article = ArticleFactory.CreatePublished(Guid.NewGuid());
        article.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(-1));
        article.ForceUnpromote(
            unpromotedBy: "super-admin-uuid",
            reason: "policy violation",
            errors: TestErrorsFactory.CreateArticleErrors()
        );
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(orderItemId, article);

        // Act
        await _handler.Handle(
            PaidEventAt(
                laterPaidAt,
                new PaidItemEffect(orderItemId, promotionLevelId, promotionUntil, SocialBoost: false)
            ),
            CancellationToken.None
        );

        // Assert
        article.IsPromoted.Should().BeTrue();
        article.PromotionLevelId.Should().Be(promotionLevelId);
        article.PromotedUntil.Should().Be(promotionUntil);
        _articleRepositoryMock.VerifyUpdateCalled(article);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsPromotionForceRemovedAfterPayment_ShouldNotReStampIt()
    {
        // Arrange
        Guid orderItemId = Guid.NewGuid();
        DateTimeOffset promotionUntil = PaidAt.AddDays(7);
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        lyrics.StampPromotion(Guid.NewGuid(), promotionUntil);
        lyrics.ForceUnpromote(
            unpromotedBy: "super-admin-uuid",
            reason: "policy violation",
            errors: TestErrorsFactory.CreateLyricsErrors()
        );
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(orderItemId, null);
        _videoRepositoryMock.SetupGetByOrderItemIdAsync(orderItemId, null);
        _lyricsRepositoryMock.SetupGetByOrderItemIdAsync(orderItemId, lyrics);

        // Act
        await _handler.Handle(
            PaidEvent(new PaidItemEffect(orderItemId, Guid.NewGuid(), promotionUntil, SocialBoost: false)),
            CancellationToken.None
        );

        // Assert
        lyrics.IsPromoted.Should().BeFalse();
        lyrics.PromotedUntil.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithMultipleItems_ShouldCommitPerItem()
    {
        // Arrange
        Guid firstItemId = Guid.NewGuid();
        Guid secondItemId = Guid.NewGuid();
        ArticleEntity article = ArticleFactory.Create(Guid.NewGuid());
        VideoEntity video = VideoFactory.Create(Guid.NewGuid());
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(firstItemId, article);
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(secondItemId, null);
        _videoRepositoryMock.SetupGetByOrderItemIdAsync(secondItemId, video);

        // Act
        await _handler.Handle(
            PaidEvent(
                new PaidItemEffect(firstItemId, null, null, SocialBoost: true),
                new PaidItemEffect(secondItemId, null, null, SocialBoost: true)
            ),
            CancellationToken.None
        );

        // Assert
        _unitOfWorkMock.VerifyCommitCalled(times: 2);
    }
}
