using _116.BuildingBlocks.Constants;
using _116.Content.Application.Interactions.UseCases.Public.Queries.GetCommentReplies;
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

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetCommentReplies;

/// <summary>
/// Unit tests for <see cref="PublicGetCommentRepliesHandler" />.
/// </summary>
public class PublicGetCommentRepliesHandlerTests : BaseContentHandlerTest
{
    private static readonly Guid ArticleId = Guid.NewGuid();
    private static readonly Guid ParentId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly Mock<IArticleCommentRepository> _articleCommentRepositoryMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly PublicGetCommentRepliesHandler _handler;

    public PublicGetCommentRepliesHandlerTests()
    {
        _articleCommentRepositoryMock = MockArticleCommentRepository.Create();
        _userLookupMock = new Mock<IUserLookupService>();
        _fileStorageMock = new Mock<IFileStorageService>();
        _handler = new PublicGetCommentRepliesHandler(
            _articleCommentRepositoryMock.Object,
            _userLookupMock.Object,
            _fileStorageMock.Object
        );
    }

    private static ArticleCommentEntity Reply() =>
        ArticleCommentEntity.CreateReply(Guid.NewGuid(), UserId, ArticleId, ParentId, "a reply");

    private void SetupAuthor() =>
        _userLookupMock
            .Setup(x =>
                x.GetAuthorInfosByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                new Dictionary<Guid, AuthorDto>
                {
                    [UserId] = new AuthorDto("jane", null, null, "Visitor", UserConstants.DefaultLocale),
                }
            );

    private static PublicGetCommentRepliesQuery Query(Guid? viewerUserId = null) =>
        new(ParentId, new PaginatedRequest(0, 10), viewerUserId);

    [Fact]
    public async Task Handle_WhenRepliesExist_ReturnsMappedRepliesWithAuthors()
    {
        ArticleCommentEntity reply = Reply();
        _articleCommentRepositoryMock.SetupGetRepliesAsync(new List<ArticleCommentEntity> { reply }, totalCount: 1);
        SetupAuthor();

        PublicGetCommentRepliesResult result = await _handler.Handle(Query(), CancellationToken.None);

        PublicArticleCommentDto dto = result.Replies.Items.Single();
        dto.ParentCommentId.Should().Be(ParentId);
        dto.Author!.UserName.Should().Be("jane");
    }

    [Fact]
    public async Task Handle_WhenViewerLikedReply_StampsIsLiked()
    {
        ArticleCommentEntity reply = Reply();
        _articleCommentRepositoryMock.SetupGetRepliesAsync(new List<ArticleCommentEntity> { reply }, totalCount: 1);
        SetupAuthor();
        _articleCommentRepositoryMock.SetupGetLikedCommentIds(new HashSet<Guid> { reply.Id });

        PublicGetCommentRepliesResult result = await _handler.Handle(Query(Guid.NewGuid()), CancellationToken.None);

        result.Replies.Items.Single().IsLiked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAnonymousViewer_SkipsLikeLookup()
    {
        ArticleCommentEntity reply = Reply();
        _articleCommentRepositoryMock.SetupGetRepliesAsync(new List<ArticleCommentEntity> { reply }, totalCount: 1);
        SetupAuthor();

        await _handler.Handle(Query(viewerUserId: null), CancellationToken.None);

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

    [Fact]
    public async Task Handle_WhenNoReplies_ReturnsEmptyPage()
    {
        _articleCommentRepositoryMock.SetupGetRepliesAsync(new List<ArticleCommentEntity>(), totalCount: 0);

        PublicGetCommentRepliesResult result = await _handler.Handle(Query(), CancellationToken.None);

        result.Replies.Items.Should().BeEmpty();
    }
}
