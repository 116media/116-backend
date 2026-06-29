# Spec 04 — Tests

Mirrors the existing comment-slice tests. Phase 1 test bodies are implementation-ready;
Phase 2/3 are outlined. The user runs `dotnet test` — do not run it here.

---

## Phase 1 — unit tests

**File:** `tests/Unit/Modules/Content/Application/Interactions/UseCases/Public/Queries/GetArticleComments/PublicGetArticleCommentsHandlerTests.cs`

The handler now depends on `IUserLookupService` and `IFileRepository` in addition to
`IArticleRepository` + `IMapper`. Extend the existing test class:

```csharp
using _116.Content.Application.Interactions.UseCases.Public.Queries.GetArticleComments;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
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

    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IUserLookupService> _userLookupMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicGetArticleCommentsHandler _handler;

    public PublicGetArticleCommentsHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _userLookupMock = new Mock<IUserLookupService>();
        _fileRepositoryMock = new Mock<IFileRepository>();
        _handler = new PublicGetArticleCommentsHandler(
            _articleRepositoryMock.Object,
            _userLookupMock.Object,
            _fileRepositoryMock.Object,
            Mapper
        );
    }

    private void SetupAuthor(Guid userId, string userName, string? email, Guid? avatarFileId, string? role)
    {
        _userLookupMock
            .Setup(x => x.GetAuthorInfosByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, AuthorInfo>
            {
                [userId] = new AuthorInfo(userName, email, avatarFileId, role),
            });
    }

    [Fact]
    public async Task Handle_WhenCommentHasResolvableUser_MapsAuthor()
    {
        // Arrange
        ArticleCommentEntity comment = ArticleCommentFactory.Create(ArticleId, UserId);
        _articleRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity> { comment }, totalCount: 1);
        SetupAuthor(UserId, "jane", email: "jane@example.com", avatarFileId: null, role: "Visitor");

        var query = new PublicGetArticleCommentsQuery(ArticleId, new PaginatedRequest(0, 10));

        // Act
        PublicGetArticleCommentsResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        ArticleCommentDto dto = result.Comments.Items.Single();
        dto.Author.Should().NotBeNull();
        dto.Author!.UserName.Should().Be("jane");
        dto.Author.Role.Should().Be("Visitor");
    }

    [Fact]
    public async Task Handle_DoesNotExposeEmail()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(ArticleId, UserId);
        _articleRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity> { comment }, totalCount: 1);
        SetupAuthor(UserId, "jane", email: "jane@example.com", avatarFileId: null, role: "Visitor");

        var query = new PublicGetArticleCommentsQuery(ArticleId, new PaginatedRequest(0, 10));

        PublicGetArticleCommentsResult result = await _handler.Handle(query, CancellationToken.None);

        result.Comments.Items.Single().Author!.Email.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ResolvesAllCommentersInOneBatchCall()
    {
        ArticleCommentEntity c1 = ArticleCommentFactory.Create(ArticleId, UserId);
        ArticleCommentEntity c2 = ArticleCommentFactory.Create(ArticleId, UserId);
        _articleRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity> { c1, c2 }, totalCount: 2);
        SetupAuthor(UserId, "jane", null, null, "Visitor");

        var query = new PublicGetArticleCommentsQuery(ArticleId, new PaginatedRequest(0, 10));

        await _handler.Handle(query, CancellationToken.None);

        _userLookupMock.Verify(
            x => x.GetAuthorInfosByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenUserNotResolvable_LeavesAuthorNull()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(ArticleId, UserId);
        _articleRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity> { comment }, totalCount: 1);
        _userLookupMock
            .Setup(x => x.GetAuthorInfosByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, AuthorInfo>());

        var query = new PublicGetArticleCommentsQuery(ArticleId, new PaginatedRequest(0, 10));

        PublicGetArticleCommentsResult result = await _handler.Handle(query, CancellationToken.None);

        result.Comments.Items.Single().Author.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenCommentDeleted_AuthorAndBodyAreNull()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(ArticleId, UserId);
        comment.SoftDelete();
        _articleRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity> { comment }, totalCount: 1);

        var query = new PublicGetArticleCommentsQuery(ArticleId, new PaginatedRequest(0, 10));

        PublicGetArticleCommentsResult result = await _handler.Handle(query, CancellationToken.None);

        ArticleCommentDto dto = result.Comments.Items.Single();
        dto.Body.Should().BeNull();
        dto.Author.Should().BeNull();
        _userLookupMock.Verify(
            x => x.GetAuthorInfosByIdsAsync(It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 0), It.IsAny<CancellationToken>()),
            Times.AtMostOnce
        );
    }

    [Fact]
    public async Task Handle_WithAvatar_ResolvesAvatarUrl()
    {
        ArticleCommentEntity comment = ArticleCommentFactory.Create(ArticleId, UserId);
        _articleRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity> { comment }, totalCount: 1);
        SetupAuthor(UserId, "jane", null, AvatarFileId, "Visitor");
        _fileRepositoryMock
            .Setup(x => x.GetStorageUrlsByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string> { [AvatarFileId] = "https://cdn/avatar.jpg" });

        var query = new PublicGetArticleCommentsQuery(ArticleId, new PaginatedRequest(0, 10));

        PublicGetArticleCommentsResult result = await _handler.Handle(query, CancellationToken.None);

        result.Comments.Items.Single().Author!.AvatarUrl.Should().Be("https://cdn/avatar.jpg");
    }

    [Fact]
    public async Task Handle_WhenNoComments_ReturnsEmptyPage()
    {
        _articleRepositoryMock.SetupGetCommentsAsync(new List<ArticleCommentEntity>(), totalCount: 0);

        var query = new PublicGetArticleCommentsQuery(ArticleId, new PaginatedRequest(0, 10));

        PublicGetArticleCommentsResult result = await _handler.Handle(query, CancellationToken.None);

        result.Comments.Items.Should().BeEmpty();
    }
}
```

> If the avatar-loop fallback is chosen over `GetStorageUrlsByIdsAsync`, swap the
> `_fileRepositoryMock` setup to `GetByIdAsync` returning a `FileEntity` with the expected
> `StorageUrl`.

---

## Phase 1 — integration tests

**File:** `tests/Integration/Modules/Content/Application/Interactions/UseCases/Public/Queries/GetArticleComments/V1/PublicGetArticleCommentsEndpointV1Tests.cs`

Extend the existing class. `BaseApiTest` already seeds `TestUser.VisitorId` as an Identity
`UserEntity`, so the seeded comment's author resolves without extra setup.

```csharp
[Fact]
public async Task GetArticleComments_WithSeededComment_ReturnsAuthor()
{
    (ArticleEntity article, ArticleCommentEntity comment) = await SeedArticleWithCommentAsync();
    Client.ClearAuthentication();

    var response = await Client.GetAsync(Routes.Public.Articles.Comments(article.Id));

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    PaginatedResult<ArticleCommentDto> body = await response.ReadAsAsync<PaginatedResult<ArticleCommentDto>>();
    ArticleCommentDto dto = body.Items.Single(c => c.Id == comment.Id);
    dto.Author.Should().NotBeNull();
    dto.Author!.UserName.Should().NotBeNullOrEmpty();
}

[Fact]
public async Task GetArticleComments_AuthorEmail_IsNotExposed()
{
    (ArticleEntity article, ArticleCommentEntity comment) = await SeedArticleWithCommentAsync();
    Client.ClearAuthentication();

    var response = await Client.GetAsync(Routes.Public.Articles.Comments(article.Id));

    PaginatedResult<ArticleCommentDto> body = await response.ReadAsAsync<PaginatedResult<ArticleCommentDto>>();
    body.Items.Single(c => c.Id == comment.Id).Author!.Email.Should().BeNull();
}

[Fact]
public async Task GetArticleComments_DeletedComment_HasNoAuthor()
{
    ArticleEntity article = await SeedArticleWithCommentAsync().ContinueWith(t => t.Result.Article);

    ArticleCommentEntity deleted = await SeedAsync<ContentDbContext, ArticleCommentEntity>(ctx =>
    {
        ArticleCommentEntity c = ArticleCommentFactory.Create(article.Id, TestUser.VisitorId);
        c.SoftDelete();
        ctx.ArticleComments.Add(c);
        return c;
    });

    Client.ClearAuthentication();
    var response = await Client.GetAsync(Routes.Public.Articles.Comments(article.Id));

    PaginatedResult<ArticleCommentDto> body = await response.ReadAsAsync<PaginatedResult<ArticleCommentDto>>();
    ArticleCommentDto dto = body.Items.Single(c => c.Id == deleted.Id);
    dto.Body.Should().BeNull();
    dto.Author.Should().BeNull();
}
```

- **Avatar test** (`GetArticleComments_WithAvatar_ResolvesAvatarUrl`): seed a `FileEntity`
  in the Core context, set the seeded user's `AvatarFileId` to it, assert
  `author.avatarUrl` equals the file's `StorageUrl`.
- **Multiple commenters** (`GetArticleComments_WithMultipleCommenters_ReturnsEachAuthor`):
  seed a second Identity `UserEntity` via `SeedAsync<IdentityDbContext, UserEntity>(...)`
  and a second comment; assert both authors resolve.

---

## Phase 2 / Phase 3 — outline

**Phase 2 (replies)** — unit: single-level enforcement rejects reply-to-reply; reply
pagination; deleted parent keeps replies visible; author projection covers reply commenters.
Integration: `/replies` returns paged replies with authors; posting a reply bumps the
article comment count; top-level list excludes replies.

**Phase 3 (likes)** — unit: like idempotency (double-like no-ops); unlike never below zero;
`IsLiked` reflects viewer; anonymous → all `false`. Integration: like→unlike round-trips
`likeCount`/`isLiked`; another user's like does not set the first user's `isLiked`.

---

## Tasks

- [ ] Extend `PublicGetArticleCommentsHandlerTests` with the six Phase 1 unit tests.
- [ ] Add mocks/setup for `IUserLookupService.GetAuthorInfosByIdsAsync` and `IFileRepository.GetStorageUrlsByIdsAsync`.
- [ ] Extend `PublicGetArticleCommentsEndpointV1Tests` with author, email-privacy, deleted, avatar, and multi-commenter integration tests.
- [ ] (Phase 2/3) Add reply and like tests when those phases are implemented.
