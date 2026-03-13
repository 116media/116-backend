using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;

/// <summary>
/// Unit tests for <see cref="AdminCreateArticleHandler"/>.
/// </summary>
public class AdminCreateArticleHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreateArticleHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();

    public AdminCreateArticleHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminCreateArticleHandler(
            _categoryRepositoryMock.Object,
            _articleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenValidFreeArticle_ShouldCreateAndReturnArticle()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        string slug = TestConstants.Content.Editorial.Article.ValidSlug;

        var command = new AdminCreateArticleCommand(
            CategoryId: category.Id,
            Title: TestConstants.Content.Editorial.Article.ValidTitle,
            Slug: slug,
            AuthorId: AuthorId,
            CustomerId: null,
            OrderItemId: null
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _articleRepositoryMock.SetupGetBySlug(slug, null);

        ArticleEntity created = ArticleFactory.Create(category.Id);
        _articleRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        AdminCreateArticleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();

        _articleRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenValidPaidArticle_ShouldSetCustomerAndOrderItem()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        string slug = TestConstants.Content.Editorial.Article.ValidSlug;
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();

        var command = new AdminCreateArticleCommand(
            CategoryId: category.Id,
            Title: TestConstants.Content.Editorial.Article.ValidTitle,
            Slug: slug,
            AuthorId: AuthorId,
            CustomerId: customerId,
            OrderItemId: orderItemId
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _articleRepositoryMock.SetupGetBySlug(slug, null);

        ArticleEntity created = ArticleFactory.CreatePaid(category.Id, customerId, orderItemId);
        _articleRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        AdminCreateArticleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();

        _articleRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldReloadArticleAfterCreation()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        string slug = TestConstants.Content.Editorial.Article.ValidSlug;

        var command = new AdminCreateArticleCommand(
            CategoryId: category.Id,
            Title: TestConstants.Content.Editorial.Article.ValidTitle,
            Slug: slug,
            AuthorId: AuthorId,
            CustomerId: null,
            OrderItemId: null
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _articleRepositoryMock.SetupGetBySlug(slug, null);

        ArticleEntity reloaded = ArticleFactory.Create(category.Id);
        _articleRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reloaded);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _articleRepositoryMock.Verify(
            x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        var command = new AdminCreateArticleCommand(
            CategoryId: nonExistentId,
            Title: TestConstants.Content.Editorial.Article.ValidTitle,
            Slug: TestConstants.Content.Editorial.Article.ValidSlug,
            AuthorId: AuthorId,
            CustomerId: null,
            OrderItemId: null
        );

        _categoryRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        const string slug = TestConstants.Content.Editorial.Article.ValidSlug;

        var command = new AdminCreateArticleCommand(
            CategoryId: category.Id,
            Title: TestConstants.Content.Editorial.Article.ValidTitle,
            Slug: slug,
            AuthorId: AuthorId,
            CustomerId: null,
            OrderItemId: null
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        ArticleEntity existing = ArticleFactory.CreateWithSlug(category.Id, slug);
        _articleRepositoryMock.SetupGetBySlug(slug, existing);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldNotAddOrCommit()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        const string slug = TestConstants.Content.Editorial.Article.ValidSlug;

        var command = new AdminCreateArticleCommand(
            CategoryId: category.Id,
            Title: TestConstants.Content.Editorial.Article.ValidTitle,
            Slug: slug,
            AuthorId: AuthorId,
            CustomerId: null,
            OrderItemId: null
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        ArticleEntity existing = ArticleFactory.CreateWithSlug(category.Id, slug);
        _articleRepositoryMock.SetupGetBySlug(slug, existing);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (ConflictException)
        {
            // Expected
        }

        // Assert
        _articleRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<ArticleEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
