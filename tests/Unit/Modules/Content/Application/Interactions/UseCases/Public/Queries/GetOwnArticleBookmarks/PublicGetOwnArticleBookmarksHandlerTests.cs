using _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnArticleBookmarks;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetOwnArticleBookmarks;

/// <summary>
/// Unit tests for <see cref="PublicGetOwnArticleBookmarksHandler"/>.
/// </summary>
public class PublicGetOwnArticleBookmarksHandlerTests : BaseContentHandlerTest
{
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly Mock<IArticleInteractionRepository> _articleInteractionRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly PublicGetOwnArticleBookmarksHandler _handler;

    public PublicGetOwnArticleBookmarksHandlerTests()
    {
        _articleInteractionRepositoryMock = MockArticleInteractionRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        FileReferenceDto coverFile = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupResolve(coverFile);
        _handler = new PublicGetOwnArticleBookmarksHandler(
            _articleInteractionRepositoryMock.Object,
            _fileStorageMock.Object,
            CreateContentLookupFactory()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenUserHasBookmarks_ShouldReturnMappedPaginatedArticles()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        DateTimeOffset bookmarkedAt = DateTimeOffset.UtcNow.AddDays(-2);
        _articleInteractionRepositoryMock.SetupGetBookmarkedArticlesAsync(
            new List<BookmarkedArticleActivity> { new(article, bookmarkedAt) },
            totalCount: 1
        );
        _articleInteractionRepositoryMock.SetupGetLikedAndBookmarkedIds(
            likedIds: new HashSet<Guid>(),
            bookmarkedIds: new HashSet<Guid> { article.Id }
        );

        var query = new PublicGetOwnArticleBookmarksQuery(
            UserId: UserId,
            PaginatedRequest: new PaginatedRequest(0, 10)
        );

        // Act
        PublicGetOwnArticleBookmarksResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Items.Should().ContainSingle();
        result.Articles.Items.Single().BookmarkedAt.Should().Be(bookmarkedAt);
        result.Articles.Items.Single().Article.IsBookmarked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserHasNoBookmarks_ShouldReturnEmptyPage()
    {
        // Arrange
        _articleInteractionRepositoryMock.SetupGetBookmarkedArticlesAsync(
            new List<BookmarkedArticleActivity>(),
            totalCount: 0
        );

        var query = new PublicGetOwnArticleBookmarksQuery(
            UserId: UserId,
            PaginatedRequest: new PaginatedRequest(0, 10)
        );

        // Act
        PublicGetOwnArticleBookmarksResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Articles.Items.Should().BeEmpty();
    }

    #endregion
}
