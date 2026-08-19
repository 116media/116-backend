using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Domain.Exceptions;
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
            Title: TestConstants.Article.ValidTitle,
            Slug: TestConstants.Article.ValidSlug,
            Headline: TestConstants.Article.ValidHeadline,
            Body: TestConstants.Article.ValidBody,
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
        article.CategoryId.Should().Be(command.CategoryId);
        article.Title.Should().Be(command.Title);
        article.Slug.Should().Be(command.Slug);
        article.Headline.Should().Be(command.Headline);
        article.Body.Should().Be(command.Body);
        article.CustomerId.Should().BeNull();
        article.OrderItemId.Should().BeNull();
        article.SocialBoost.Should().BeFalse();
        article.MetaTitle.Should().BeNull();
        article.MetaDescription.Should().BeNull();
        result.Article.Id.Should().Be(article.Id);
        result.Article.Title.Should().Be(command.Title);
        result.Article.Slug.Should().Be(command.Slug);
        article.DomainEvents.OfType<ArticleBodyImagesOrphanedEvent>().Should().BeEmpty();
        _articleRepositoryMock.VerifyUpdateCalled(article);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenBodyImagesDropOut_ShouldRaiseOrphanedEventWithCapturedKeys()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        AdminUpdateArticleCommand command = BuildCommand(article, category.Id);
        List<ArticleImageEntity> images = ArticleImageFactory.CreateMany(article.Id, 2);
        article.ClearDomainEvents();

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _articleRepositoryMock.SetupGetBySlug(command.Slug, null);
        _articleRepositoryMock.SetupGetImagesByArticleId(article.Id, images);
        _articleRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(article.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(article);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        ArticleBodyImagesOrphanedEvent orphanedEvent = article
            .DomainEvents.OfType<ArticleBodyImagesOrphanedEvent>()
            .Should()
            .ContainSingle()
            .Subject;
        orphanedEvent.ArticleId.Should().Be(article.Id);
        orphanedEvent.StorageKeys.Should().BeEquivalentTo(images.Select(img => img.StorageKey));
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
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenArticleIsApproved_ShouldThrowDomainRuleException()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreateApproved(CategoryId);
        article.ClearDomainEvents();
        string originalTitle = article.Title;
        AdminUpdateArticleCommand command = BuildCommand(article, CategoryId);
        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainRuleException>();
        article.Status.Should().Be(EnumContentStatus.Approved);
        article.Title.Should().Be(originalTitle);
        article.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenSlugConflictsWithAnotherArticle_ShouldThrowConflictException()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        ArticleEntity article = ArticleFactory.CreateWithSlug(CategoryId, "original-article-slug");
        article.ClearDomainEvents();
        AdminUpdateArticleCommand command = BuildCommand(article, category.Id);
        ArticleEntity conflicting = ArticleFactory.CreateWithSlug(CategoryId, command.Slug);

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _articleRepositoryMock.SetupGetBySlug(command.Slug, conflicting);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        article.Slug.Should().Be("original-article-slug");
        article.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
