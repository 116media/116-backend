using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.BackgroundJobs;
using _116.Core.Application.Shared.Services;
using _116.Tests.Fixtures.Factories.Content;
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
    private readonly Mock<ICloudinaryService> _cloudinaryMock;
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
        _cloudinaryMock = new Mock<ICloudinaryService>();
        _unitOfWorkMock = new Mock<IContentUnitOfWork>();
        _loggerMock = new Mock<ILogger<AbandonedDraftCleanupJob>>();
        _jobContextMock = new Mock<IJobExecutionContext>();

        // Wire up scope factory → scope → service provider → services
        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IArticleRepository)))
            .Returns(_articleRepositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(ICloudinaryService))).Returns(_cloudinaryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IContentUnitOfWork))).Returns(_unitOfWorkMock.Object);

        _jobContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _cloudinaryMock
            .Setup(x => x.DeleteImagesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

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
        _cloudinaryMock.Verify(
            x => x.DeleteImagesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion

    #region Draft Without Images

    [Fact]
    public async Task Execute_WithDraftWithoutImages_ShouldRemoveArticleWithoutCallingCloudinary()
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
        _cloudinaryMock.Verify(
            x => x.DeleteImagesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _articleRepositoryMock.Verify(x => x.Remove(draft), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Draft With Images

    [Fact]
    public async Task Execute_WithDraftWithImages_ShouldDeleteCloudinaryImagesBeforeRemoving()
    {
        // Arrange
        ArticleEntity draft = ArticleFactory.Create(CategoryId);
        ArticleImageEntity image = ArticleImageFactory.CreateCover(draft.Id);
        draft.Images.Add(image);

        _articleRepositoryMock
            .Setup(x => x.GetAbandonedDraftsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArticleEntity> { draft }.AsReadOnly());

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        _cloudinaryMock.Verify(
            x =>
                x.DeleteImagesAsync(
                    It.Is<IEnumerable<string>>(keys => keys.Contains(image.StorageKey)),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _articleRepositoryMock.Verify(x => x.Remove(draft), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenCloudinaryDeleteReturnsFalse_ShouldStillRemoveArticle()
    {
        // Arrange
        ArticleEntity draft = ArticleFactory.Create(CategoryId);
        ArticleImageEntity image = ArticleImageFactory.CreateCover(draft.Id);
        draft.Images.Add(image);

        _articleRepositoryMock
            .Setup(x => x.GetAbandonedDraftsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArticleEntity> { draft }.AsReadOnly());

        _cloudinaryMock
            .Setup(x => x.DeleteImagesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // simulate partial Cloudinary failure

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert — job continues and removes the article despite the Cloudinary warning
        _articleRepositoryMock.Verify(x => x.Remove(draft), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
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
