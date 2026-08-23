using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Content.Infrastructure.BackgroundJobs;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Quartz;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.BackgroundJobs;

/// <summary>
/// Unit tests for <see cref="AbandonedDraftCleanupJob"/>.
/// </summary>
public class AbandonedDraftCleanupJobTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<AbandonedDraftCleanupJob>> _loggerMock;
    private readonly Mock<IJobExecutionContext> _jobContextMock;
    private readonly AbandonedDraftCleanupJob _job;

    public AbandonedDraftCleanupJobTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _articleRepositoryMock = new Mock<IArticleRepository>();
        _unitOfWorkMock = new Mock<IContentUnitOfWork>();
        _loggerMock = new Mock<ILogger<AbandonedDraftCleanupJob>>();
        _jobContextMock = new Mock<IJobExecutionContext>();

        // Wire up scope factory → scope → service provider → services
        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IArticleRepository)))
            .Returns(_articleRepositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IContentUnitOfWork))).Returns(_unitOfWorkMock.Object);

        _jobContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _job = new AbandonedDraftCleanupJob(_scopeFactoryMock.Object, _loggerMock.Object);
    }

    #region No Drafts

    [Fact]
    public async Task Execute_WithNoDrafts_ShouldNotRemoveAnyArticle()
    {
        // Arrange
        _articleRepositoryMock
            .Setup(x => x.GetAbandonedDraftsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArticleEntity>().AsReadOnly());

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        _articleRepositoryMock.Verify(x => x.Remove(It.IsAny<ArticleEntity>()), Times.Never);
    }

    #endregion

    #region Draft Without Images

    [Fact]
    public async Task Execute_WithDraftWithoutImages_ShouldRemoveArticleAndRaiseDeletionEvent()
    {
        // Arrange
        ArticleEntity draft = ArticleFactory.Create(CategoryId);
        // draft.Images is empty by default

        _articleRepositoryMock
            .Setup(x => x.GetAbandonedDraftsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArticleEntity> { draft }.AsReadOnly());

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        draft
            .DomainEvents.OfType<ArticleDeletedEvent>()
            .Should()
            .ContainSingle()
            .Which.BodyImageStorageKeys.Should()
            .BeEmpty();
        _articleRepositoryMock.Verify(x => x.Remove(draft), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Draft With Images

    [Fact]
    public async Task Execute_WithDraftWithImages_ShouldCaptureStorageKeysOnDeletionEvent()
    {
        // Arrange
        ArticleEntity draft = ArticleFactory.Create(CategoryId);
        ArticleImageEntity image = ArticleImageFactory.CreateBody(draft.Id);
        draft.Images.Add(image);

        _articleRepositoryMock
            .Setup(x => x.GetAbandonedDraftsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArticleEntity> { draft }.AsReadOnly());

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert — the shared post-commit cleanup consumer purges the captured keys.
        draft
            .DomainEvents.OfType<ArticleDeletedEvent>()
            .Should()
            .ContainSingle()
            .Which.BodyImageStorageKeys.Should()
            .BeEquivalentTo([image.StorageKey]);
        _articleRepositoryMock.Verify(x => x.Remove(draft), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_WithDraftWithACoverImageRow_ShouldCaptureOnlyTheBodyImageKeys()
    {
        // Arrange
        ArticleEntity draft = ArticleFactory.Create(CategoryId);
        ArticleImageEntity cover = ArticleImageFactory.CreateCover(draft.Id);
        ArticleImageEntity body = ArticleImageFactory.CreateBody(draft.Id);
        draft.Images.Add(cover);
        draft.Images.Add(body);

        _articleRepositoryMock
            .Setup(x => x.GetAbandonedDraftsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArticleEntity> { draft }.AsReadOnly());

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        draft
            .DomainEvents.OfType<ArticleDeletedEvent>()
            .Should()
            .ContainSingle()
            .Which.BodyImageStorageKeys.Should()
            .BeEquivalentTo([body.StorageKey]);
    }

    #endregion

    #region Multiple Drafts

    [Fact]
    public async Task Execute_WithMultipleDrafts_ShouldProcessEachDraftIndependently()
    {
        // Arrange
        ArticleEntity draft1 = ArticleFactory.Create(CategoryId);
        ArticleEntity draft2 = ArticleFactory.Create(CategoryId);

        _articleRepositoryMock
            .Setup(x => x.GetAbandonedDraftsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArticleEntity> { draft1, draft2 }.AsReadOnly());

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        _articleRepositoryMock.Verify(x => x.Remove(draft1), Times.Once);
        _articleRepositoryMock.Verify(x => x.Remove(draft2), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    #endregion
}
