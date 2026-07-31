using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;

/// <summary>
/// Unit tests for <see cref="AdminUpdateArticleHandler"/>.
/// </summary>
public class AdminUpdateArticleHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminUpdateArticleHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUpdateArticleHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        FileEntity coverFile = FileFactory.CreateImage();
        _fileRepositoryMock.SetupGetById(coverFile);
        _handler = new AdminUpdateArticleHandler(
            _categoryRepositoryMock.Object,
            _articleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileRepositoryMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    private static AdminUpdateArticleCommand BuildCommand(ArticleEntity article, Guid categoryId) =>
        new(
            Id: article.Id.ToString(),
            CategoryId: categoryId,
            Title: TestConstants.Content.Editorial.Article.ValidTitle,
            Slug: TestConstants.Content.Editorial.Article.ValidSlug,
            Headline: TestConstants.Content.Editorial.Article.ValidHeadline,
            Body: TestConstants.Content.Editorial.Article.ValidBody,
            CustomerId: null,
            OrderItemId: null,
            SocialBoost: false,
            MetaTitle: null,
            MetaDescription: null
        );

    #region Success Cases

    [Fact]
    public async Task Handle_WhenDraftArticle_ShouldUpdateAndReturnArticle()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        AdminUpdateArticleCommand command = BuildCommand(article, category.Id);

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _articleRepositoryMock.SetupGetBySlug(command.Slug, null);
        _articleRepositoryMock.SetupGetImagesByArticleId(article.Id, new List<ArticleImageEntity>());
        _articleRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(article.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(article);

        // Act
        AdminUpdateArticleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        article.DomainEvents.OfType<ArticleBodyImagesOrphanedEvent>().Should().BeEmpty();
        _articleRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenBodyImagesDropOut_ShouldRaiseOrphanedEventWithCapturedKeys()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        AdminUpdateArticleCommand command = BuildCommand(article, category.Id);
        // Body-type images whose URLs do not appear in the command body drop out on update.
        List<ArticleImageEntity> images = ArticleImageFactory.CreateMany(article.Id, 2);

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _articleRepositoryMock.SetupGetBySlug(command.Slug, null);
        _articleRepositoryMock.SetupGetImagesByArticleId(article.Id, images);
        _articleRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(article.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(article);

        // Act
        AdminUpdateArticleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert — the handler defers row removal and asset deletion to the post-commit consumer.
        result.Should().NotBeNull();
        article
            .DomainEvents.OfType<ArticleBodyImagesOrphanedEvent>()
            .Should()
            .ContainSingle()
            .Which.StorageKeys.Should()
            .BeEquivalentTo(images.Select(img => img.StorageKey));
        _articleRepositoryMock.Verify(x => x.RemoveImages(It.IsAny<IEnumerable<ArticleImageEntity>>()), Times.Never);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        ArticleEntity dummy = ArticleFactory.Create(CategoryId);
        AdminUpdateArticleCommand command = BuildCommand(dummy, CategoryId) with { Id = nonExistentId.ToString() };
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenArticleIsApproved_ShouldThrowBadRequestException()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreateApproved(CategoryId);
        AdminUpdateArticleCommand command = BuildCommand(article, CategoryId);
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenSlugConflictsWithAnotherArticle_ShouldThrowConflictException()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        // Article has a different slug so the handler's slug-change check is triggered.
        ArticleEntity article = ArticleFactory.CreateWithSlug(CategoryId, "original-article-slug");
        // Command uses ValidSlug — a different slug that already belongs to another article.
        AdminUpdateArticleCommand command = BuildCommand(article, category.Id);
        ArticleEntity conflicting = ArticleFactory.CreateWithSlug(CategoryId, command.Slug);

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _articleRepositoryMock.SetupGetBySlug(command.Slug, conflicting);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
