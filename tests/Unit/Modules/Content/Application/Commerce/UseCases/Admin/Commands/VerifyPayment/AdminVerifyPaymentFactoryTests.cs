using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;

/// <summary>
/// Unit tests for <see cref="AdminVerifyPaymentFactory"/>.
/// </summary>
public class AdminVerifyPaymentFactoryTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentOrderRepository> _orderRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminVerifyPaymentFactory _factory;

    private static readonly Guid AdminUserId = Guid.NewGuid();
    private const string ReceiptUrl = TestConstants.Content.Commerce.ValidReceiptUrl;

    public AdminVerifyPaymentFactoryTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _lookupRepositoryMock = MockLookupRepository.Create();
        _orderRepositoryMock = MockContentOrderRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _factory = new AdminVerifyPaymentFactory(
            _articleRepositoryMock.Object,
            _videoRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            _lookupRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentOrderErrors()
        );
    }

    #region Success Cases

    [Fact]
    public async Task VerifyAsync_WhenOrderHasNoItems_ShouldVerifyAndCommit()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);

        // Act
        await _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        _orderRepositoryMock.VerifyUpdatePaymentCalled();
        _orderRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task VerifyAsync_WithSocialBoostItem_WhenArticleFound_ShouldUpdateArticle()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        Guid categoryId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.CreateSocialBoost(order.Id, categoryId);
        order.Items.Add(item);
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);

        ArticleEntity article = ArticleFactory.Create(categoryId);
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(item.Id, article);

        // Act
        await _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        _articleRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task VerifyAsync_WithPromotionLevelItem_WhenArticleFound_ShouldUpdateArticle()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        Guid categoryId = Guid.NewGuid();
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateDefault();
        ContentOrderItemEntity item = ContentOrderItemFactory.CreateWithPromo(
            order.Id,
            categoryId,
            promoLevel.Id,
            promoLevel.PriceUsd
        );
        order.Items.Add(item);
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);

        ArticleEntity article = ArticleFactory.Create(categoryId);
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(item.Id, article);
        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrow(promoLevel);

        // Act
        await _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        _articleRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task VerifyAsync_WhenNoArticleOrVideoForItem_ShouldStillCommit()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        Guid categoryId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, categoryId);
        order.Items.Add(item);
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);

        // Both repositories return null (default setup)

        // Act
        await _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task VerifyAsync_WithSocialBoostItem_WhenVideoFound_ShouldUpdateVideo()
    {
        // Arrange — article returns null so factory tries video
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        Guid categoryId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.CreateSocialBoost(order.Id, categoryId);
        order.Items.Add(item);
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);

        VideoEntity video = VideoFactory.Create(categoryId);
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(item.Id, null);
        _videoRepositoryMock.SetupGetByOrderItemIdAsync(item.Id, video);

        // Act
        await _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task VerifyAsync_WithPromotionLevelItem_WhenVideoFound_ShouldUpdateVideo()
    {
        // Arrange — article returns null so factory tries video
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        Guid categoryId = Guid.NewGuid();
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateDefault();
        ContentOrderItemEntity item = ContentOrderItemFactory.CreateWithPromo(
            order.Id,
            categoryId,
            promoLevel.Id,
            promoLevel.PriceUsd
        );
        order.Items.Add(item);
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);

        VideoEntity video = VideoFactory.Create(categoryId);
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(item.Id, null);
        _videoRepositoryMock.SetupGetByOrderItemIdAsync(item.Id, video);
        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrow(promoLevel);

        // Act
        await _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task VerifyAsync_WhenArticleInPendingPayment_ShouldTransitionToPendingReview()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        Guid categoryId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, categoryId);
        order.Items.Add(item);
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);

        ArticleEntity article = ArticleFactory.CreatePendingPayment(categoryId, customerId, item.Id);
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(item.Id, article);

        // Act
        await _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        article.Status.Should().Be(EnumContentStatus.PendingReview);
        _articleRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task VerifyAsync_WhenVideoInPendingPayment_ShouldTransitionToPendingReview()
    {
        // Arrange — article returns null so factory tries video
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        Guid categoryId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, categoryId);
        order.Items.Add(item);
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);

        VideoEntity video = VideoFactory.CreatePendingPayment(categoryId, customerId, item.Id);
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(item.Id, null);
        _videoRepositoryMock.SetupGetByOrderItemIdAsync(item.Id, video);

        // Act
        await _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        video.Status.Should().Be(EnumContentStatus.PendingReview);
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    /// <summary>
    /// When neither an article nor a video resolves for an order item's promoted purchase,
    /// the factory falls through to lyrics — stamping promotion (no SocialBoost, per spec 12's
    /// explicit scoping) and moving it to <c>PendingReview</c>.
    /// </summary>
    [Fact]
    public async Task VerifyAsync_WithPromotionLevelItem_WhenLyricsFound_ShouldStampPromotionAndUpdateLyrics()
    {
        // Arrange — article and video both return null so factory falls through to lyrics
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        Guid categoryId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        PromotionLevelEntity promoLevel = PromotionLevelFactory.CreateDefault();
        ContentOrderItemEntity item = ContentOrderItemFactory.CreateWithPromo(
            order.Id,
            categoryId,
            promoLevel.Id,
            promoLevel.PriceUsd
        );
        order.Items.Add(item);
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);

        LyricsEntity lyrics = LyricsFactory.CreatePendingPayment(categoryId, customerId, item.Id);
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(item.Id, null);
        _videoRepositoryMock.SetupGetByOrderItemIdAsync(item.Id, null);
        _lyricsRepositoryMock.SetupGetByOrderItemIdAsync(item.Id, lyrics);
        _lookupRepositoryMock.SetupGetPromotionLevelByIdOrThrow(promoLevel);

        // Act
        await _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        lyrics.IsPromoted.Should().BeTrue();
        lyrics.PromotedUntil.Should().NotBeNull();
        lyrics.Status.Should().Be(EnumContentStatus.PendingReview);
        _lyricsRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    /// <summary>
    /// A lyrics item with no promotion purchased still transitions Draft/PendingPayment →
    /// PendingReview via <c>MarkPendingReview()</c>, mirroring the article/video branches, but
    /// leaves <c>IsPromoted</c> untouched.
    /// </summary>
    [Fact]
    public async Task VerifyAsync_WithoutPromotionLevelItem_WhenLyricsFound_ShouldTransitionToPendingReviewWithoutPromoting()
    {
        // Arrange
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        Guid categoryId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(order.Id, categoryId);
        order.Items.Add(item);
        ContentPaymentEntity payment = ContentPaymentFactory.Create(order.Id);

        LyricsEntity lyrics = LyricsFactory.CreatePendingPayment(categoryId, customerId, item.Id);
        _articleRepositoryMock.SetupGetByOrderItemIdAsync(item.Id, null);
        _videoRepositoryMock.SetupGetByOrderItemIdAsync(item.Id, null);
        _lyricsRepositoryMock.SetupGetByOrderItemIdAsync(item.Id, lyrics);

        // Act
        await _factory.VerifyAsync(order, payment, AdminUserId, ReceiptUrl, CancellationToken.None);

        // Assert
        lyrics.IsPromoted.Should().BeFalse();
        lyrics.Status.Should().Be(EnumContentStatus.PendingReview);
        _lyricsRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion
}
