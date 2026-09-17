using _116.Content.Application.Interactions.UseCases.Public.Queries.GetArticleComments;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Contracts.Application.Services;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetArticleComments;

/// <summary>
/// Unit tests for <see cref="PublicGetArticleCommentsHandler" />.
/// </summary>
public class PublicGetArticleCommentsHandlerTests : BaseContentHandlerTest
{
    private static readonly Guid ArticleId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid AvatarFileId = Guid.NewGuid();

    private readonly Mock<IArticleCommentRepository> _articleCommentRepositoryMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly PublicGetArticleCommentsHandler _handler;

    public PublicGetArticleCommentsHandlerTests()
    {
        _articleCommentRepositoryMock = MockArticleCommentRepository.Create();
        _userLookupMock = new Mock<IUserLookupService>();
        _fileStorageMock = new Mock<IFileStorageService>();
        _handler = new PublicGetArticleCommentsHandler(
            _articleCommentRepositoryMock.Object,
            _userLookupMock.Object,
            _fileStorageMock.Object
        );
    }

    private void SetupAuthor(Guid userId, string userName, string? email, Guid? avatarFileId, string? role)
    {
        _userLookupMock
            .Setup(x =>
                x.GetAuthorInfosByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new Dictionary<Guid, AuthorDto> { [userId] = new AuthorDto(userName, email, avatarFileId, role) }
            );
    }

    private static PublicGetArticleCommentsQuery Query(Guid? viewerUserId = null) =>
        new(ArticleId, new PaginatedRequest(0, 10), viewerUserId);

    [Fact]
    public async Task Handle_WhenCommentHasResolvableUser_MapsAuthor()
    {
        // Arrange
        ArticleCommentEntity comment = ArticleCommentFactory.Create(ArticleId, UserId);
        _articleCommentRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity> { comment }, totalCount: 1);
        SetupAuthor(UserId, "jane", email: "jane@example.com", avatarFileId: null, role: "Visitor");

        // Act
        PublicGetArticleCommentsResult result = await _handler.Handle(Query(), CancellationToken.None);

        // Assert
        PublicArticleCommentDto dto = result.Comments.Items.Single();
        dto.Author.Should().NotBeNull();
        dto.Author!.UserName.Should().Be("jane");
    }

    [Fact]
    public async Task Handle_DoesNotExposeEmail()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(ArticleId, UserId);
        _articleCommentRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity> { comment }, totalCount: 1);
        SetupAuthor(UserId, "jane", email: "jane@example.com", avatarFileId: null, role: "Visitor");

        PublicGetArticleCommentsResult result = await _handler.Handle(Query(), CancellationToken.None);

        // The public author shape carries no email member; the type system enforces the absence.
        result.Comments.Items.Single().Author.Should().BeOfType<PublicAuthorDto>();
    }

    [Fact]
    public async Task Handle_ResolvesAllCommentersInOneBatchCall()
    {
        ArticleCommentEntity c1 = ArticleCommentFactory.Create(ArticleId, UserId);
        ArticleCommentEntity c2 = ArticleCommentFactory.Create(ArticleId, UserId);
        _articleCommentRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity> { c1, c2 }, totalCount: 2);
        SetupAuthor(UserId, "jane", null, null, "Visitor");

        await _handler.Handle(Query(), CancellationToken.None);

        _userLookupMock.Verify(
            x => x.GetAuthorInfosByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenUserNotResolvable_LeavesAuthorNull()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(ArticleId, UserId);
        _articleCommentRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity> { comment }, totalCount: 1);
        _userLookupMock
            .Setup(x =>
                x.GetAuthorInfosByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new Dictionary<Guid, AuthorDto>());

        PublicGetArticleCommentsResult result = await _handler.Handle(Query(), CancellationToken.None);

        result.Comments.Items.Single().Author.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenCommentDeleted_AuthorAndBodyAreNull()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(ArticleId, UserId);
        comment.SoftDelete();
        _articleCommentRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity> { comment }, totalCount: 1);

        PublicGetArticleCommentsResult result = await _handler.Handle(Query(), CancellationToken.None);

        PublicArticleCommentDto dto = result.Comments.Items.Single();
        dto.Body.Should().BeNull();
        dto.Author.Should().BeNull();
        _userLookupMock.Verify(
            x => x.GetAuthorInfosByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WithAvatar_ResolvesAvatarUrl()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(ArticleId, UserId);
        _articleCommentRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity> { comment }, totalCount: 1);
        SetupAuthor(UserId, "jane", null, AvatarFileId, "Visitor");
        _fileStorageMock
            .Setup(x => x.ResolveUrlsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string> { [AvatarFileId] = "https://cdn/avatar.jpg" });

        PublicGetArticleCommentsResult result = await _handler.Handle(Query(), CancellationToken.None);

        result.Comments.Items.Single().Author!.AvatarUrl.Should().Be("https://cdn/avatar.jpg");
    }

    [Fact]
    public async Task Handle_WhenNoComments_ReturnsEmptyPage()
    {
        _articleCommentRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity>(), totalCount: 0);

        PublicGetArticleCommentsResult result = await _handler.Handle(Query(), CancellationToken.None);

        result.Comments.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_StampsReplyCountForTopLevelComment()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(ArticleId, UserId);
        _articleCommentRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity> { comment }, totalCount: 1);
        SetupAuthor(UserId, "jane", null, null, "Visitor");
        _articleCommentRepositoryMock.SetupGetReplyCounts(new Dictionary<Guid, int> { [comment.Id] = 3 });

        PublicGetArticleCommentsResult result = await _handler.Handle(Query(), CancellationToken.None);

        result.Comments.Items.Single().ReplyCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WhenViewerLikedComment_StampsIsLikedTrue()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(ArticleId, UserId);
        _articleCommentRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity> { comment }, totalCount: 1);
        SetupAuthor(UserId, "jane", null, null, "Visitor");
        _articleCommentRepositoryMock.SetupGetLikedCommentIds(new HashSet<Guid> { comment.Id });

        PublicGetArticleCommentsResult result = await _handler.Handle(Query(Guid.NewGuid()), CancellationToken.None);

        result.Comments.Items.Single().IsLiked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAnonymousViewer_SkipsLikeLookupAndLeavesIsLikedFalse()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(ArticleId, UserId);
        _articleCommentRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity> { comment }, totalCount: 1);
        SetupAuthor(UserId, "jane", null, null, "Visitor");

        PublicGetArticleCommentsResult result = await _handler.Handle(
            Query(viewerUserId: null),
            CancellationToken.None
        );

        result.Comments.Items.Single().IsLiked.Should().BeFalse();
        _articleCommentRepositoryMock.Verify(
            x =>
                x.GetLikedCommentIdsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }
}
