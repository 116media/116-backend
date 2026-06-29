# Current State

Everything that exists today for the article-comment vertical slice and the
author-resolution mechanism it must reuse. File paths are relative to
`apps/backend/` unless noted.

---

## 1. Domain

### `ArticleCommentEntity`

`src/Modules/Content/Content/Domain/Entities/ArticleCommentEntity.cs`

- Inherits `Aggregate<Guid>` (has `CreatedAt/UpdatedAt/CreatedBy/UpdatedBy`).
- Properties: `UserId (Guid)`, `ArticleId (Guid)`, `Body (string)`, `IsDeleted (bool)`,
  `DeletedAt (DateTimeOffset?)`, `Article (ArticleEntity nav)`.
- `UserId` is the identity user UUID; the entity comment explicitly notes **no FK to the
  identity schema by design** — this is why author details are not on the row.
- Factory `Create(id, userId, articleId, body)`; `Edit(body)`; `SoftDelete()` sets
  `IsDeleted = true` and stamps `DeletedAt`.
- **No `ParentCommentId`** (no threading), **no like count** (no per-comment likes).

### `ContentConstants`

`src/Modules/Content/Content/Domain/Constants/ContentConstants.cs`

- `MaxCommentBodyLength = 1000` (line 212).

---

## 2. Application — DTOs

### `ArticleCommentDto`

`src/Modules/Content/Content/Application/Shared/DTOs/ArticleCommentDto.cs`

```csharp
public record ArticleCommentDto(Guid Id, Guid UserId, string? Body, bool IsDeleted) : AuditableDto;
```

The gap: no author, no parent, no likes.

### `AuthorDto` (the projection to reuse)

`src/Modules/Content/Content/Application/Shared/DTOs/AuthorDto.cs`

```csharp
public record AuthorDto(string UserName, string? Email, string? AvatarUrl, string? Role);
```

### `ArticleDetailDto` (already carries the author)

`src/Modules/Content/Content/Application/Shared/DTOs/ArticleDetailDto.cs`

Its last parameter is `AuthorDto? Author = null` — "the resolved author profile with
avatar URL, or null if the author could not be found."

---

## 3. Application — the query slice

Folder: `src/Modules/Content/Content/Application/Interactions/UseCases/Public/Queries/GetArticleComments/`

| File | Role |
|------|------|
| `PublicGetArticleCommentsQuery.cs` | `record PublicGetArticleCommentsQuery(Guid ArticleId, PaginatedRequest PaginatedRequest)` + `PublicGetArticleCommentsResult(PaginatedResult<ArticleCommentDto> Comments)` |
| `PublicGetArticleCommentsHandler.cs` | Depends on `IArticleRepository` and `IMapper`; calls `GetCommentsAsync`, maps via `ToArticleCommentDtos(mapper)`, wraps in `PaginatedResult` |
| `PublicGetArticleCommentsMetaField.cs` | Endpoint name/summary/description |
| `V1/PublicGetArticleCommentsEndpointV1.cs` | `GET /api/v1/public/articles/{id:guid}/comments`, `AllowAnonymous`, `ContentBrowsing` rate limit, produces `PaginatedResult<ArticleCommentDto>` |

The handler today (no author resolution):

```csharp
public class PublicGetArticleCommentsHandler(IArticleRepository articleRepository, IMapper mapper)
    : IQueryHandler<PublicGetArticleCommentsQuery, PublicGetArticleCommentsResult>
{
    public async Task<PublicGetArticleCommentsResult> Handle(
        PublicGetArticleCommentsQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageIndex = query.PaginatedRequest.PageIndex;
        int pageSize = query.PaginatedRequest.PageSize;

        (List<ArticleCommentEntity> comments, int totalCount) = await articleRepository.GetCommentsAsync(
            articleId: query.ArticleId,
            page: pageIndex + 1,
            pageSize: pageSize,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<ArticleCommentDto> dtoList = comments.AsReadOnly().ToArticleCommentDtos(mapper);

        var paginated = new PaginatedResult<ArticleCommentDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new PublicGetArticleCommentsResult(Comments: paginated);
    }
}
```

---

## 4. Application — the command slices

- **Add**: `.../Public/Commands/AddArticleComment/` — `PublicAddArticleCommentHandler`
  creates the entity, `AddCommentAsync`, increments `article.CommentCount`, commits, and
  returns `ToArticleCommentDto(mapper)`.
- **Edit**: `.../Public/Commands/EditArticleComment/` — owner-only body edit.
- **Delete (public)**: `.../Public/Commands/DeleteArticleComment/` —
  `PublicDeleteArticleCommentHandler` enforces `comment.UserId == command.UserId` (throws
  `NotCommentOwner`), soft-deletes, decrements count.
- **Delete (admin)**: `.../Admin/Commands/DeleteArticleComment/`.

---

## 5. Infrastructure — mapper, repository, EF config

### Mapper

`src/Modules/Content/Content/Application/Shared/Mappers/ArticleMapper.cs`

- Mapster registration: `config.NewConfig<ArticleCommentEntity, ArticleCommentDto>().Map(dest => dest.Body, src => src.IsDeleted ? null : src.Body);`
- `ToArticleCommentDto(this ArticleCommentEntity, IMapper)` — maps then re-nulls `Body`
  when deleted.
- `ToArticleCommentDtos(this IReadOnlyList<ArticleCommentEntity>, IMapper)` — maps a list.

### Repository

- Interface: `src/Modules/Content/Content/Application/Shared/Repositories/IArticleRepository.cs`
  — `GetCommentsAsync(articleId, page, pageSize, ct)` returns
  `(List<ArticleCommentEntity> Comments, int TotalCount)`; also `AddCommentAsync`,
  `GetCommentByIdAsync`, `UpdateComment`.
- Impl: `src/Modules/Content/Content/Infrastructure/Repositories/ArticleRepository.cs`
  (line 272) — uses `ArticleCommentByArticleIdSpecification`, orders by `CreatedAt`,
  paginates with `Skip/Take`.

### EF configuration

`src/Modules/Content/Content/Infrastructure/Persistence/Configurations/ArticleCommentConfiguration.cs`

- Key `Id`; `UserId`, `ArticleId` required; `Body` max length `MaxCommentBodyLength`;
  `IsDeleted` default false; `DeletedAt` optional; FK to `Article` (cascade delete);
  index `ix_article_comments_article` on `ArticleId`.

---

## 6. The author-resolution mechanism (reused by Phase 1)

### `IUserLookupService`

`src/Modules/Identity/Identity.Contracts/Application/IUserLookupService.cs`

```csharp
public interface IUserLookupService
{
    Task<string?> GetUserNameByIdAsync(Guid userId, CancellationToken ct = default);

    Task<AuthorInfo?> GetAuthorInfoByIdAsync(Guid userId, CancellationToken ct = default);
}
```

> **Note:** there is currently **no batch method** — Phase 1 adds
> `GetAuthorInfosByIdsAsync` (see [02-comment-author-projection.md](02-comment-author-projection.md)).

### `AuthorInfo`

`src/Modules/Identity/Identity.Contracts/Application/AuthorInfo.cs`

```csharp
public record AuthorInfo(string UserName, string? Email, Guid? AvatarFileId, string? Role);
```

Avatar is a **file ID**, resolved to a URL by the consuming Content module.

### `UserLookupService`

`src/Modules/Identity/Identity/Infrastructure/Services/UserLookupService.cs`

- `GetAuthorInfoByIdAsync` loads the user with `UserRoles.ThenInclude(Role)` and maps to
  `AuthorInfo(UserName, Email, AvatarFileId, first role name)`.
- Registered in `src/Modules/Identity/Identity/IdentityModule.cs` line 140:
  `services.AddScoped<IUserLookupService, UserLookupService>();`

### Reference consumer — `AdminGetArticleByIdHandler`

`src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Queries/GetArticleById/AdminGetArticleByIdHandler.cs`

Depends on `IArticleRepository`, `IUserLookupService`, `IFileRepository`, `IMapper`. After
mapping the article, it:

```csharp
AuthorInfo? authorInfo = await userLookup.GetAuthorInfoByIdAsync(article.AuthorId, cancellationToken);

AuthorDto? author = null;
if (authorInfo is not null)
{
    string? avatarUrl = null;
    if (authorInfo.AvatarFileId.HasValue)
    {
        FileEntity? avatarFile = await fileRepository.GetByIdAsync(authorInfo.AvatarFileId.Value, cancellationToken);
        avatarUrl = avatarFile?.StorageUrl;
    }

    author = new AuthorDto(authorInfo.UserName, authorInfo.Email, avatarUrl, authorInfo.Role);
}

return new AdminGetArticleByIdResult(Article: dto with { Author = author });
```

**This is the pattern Phase 1 reuses**, batched across a page of commenters.

`FileEntity.StorageUrl` lives at `src/Modules/Core/Core/Domain/Entities/FileEntity.cs`
(line 41). `IFileRepository.GetByIdAsync` is already injected into content read handlers.

---

## 7. Existing tests

### Unit

- `tests/Unit/Modules/Content/Application/Interactions/UseCases/Public/Queries/GetArticleComments/PublicGetArticleCommentsHandlerTests.cs`
  — extends `BaseContentHandlerTest` (provides a configured `IMapper`); mocks
  `IArticleRepository` via `MockArticleRepository.Create()` and
  `.SetupGetCommentsAsync(list, totalCount)`; asserts count + empty page. Uses
  `ArticleCommentFactory.Create(articleId, userId)`.
- Add / edit / delete handler + validator tests in sibling folders.

### Integration

- `tests/Integration/Modules/Content/Application/Interactions/UseCases/Public/Queries/GetArticleComments/V1/PublicGetArticleCommentsEndpointV1Tests.cs`
  — `[Collection("Database")]`, extends `BaseApiTest`. Seeds a `ContentType` →
  `Category` → published `Article` → `ArticleComment` (via factories), calls
  `Routes.Public.Articles.Comments(articleId)`, asserts the comment is returned. Does
  **not** currently seed an Identity user for the commenter.

### Fixtures

- `tests/Fixtures/Factories/Content/ArticleCommentFactory.cs`.
- `BaseApiTest` seeds `TestUser.VisitorId` as an Identity `UserEntity`
  (`UserFactory.CreateWithId(User.VisitorId, User.VisitorEmail)` — see
  `tests/Integration/Common/Base/BaseApiTest.cs` line 139), which Phase 1 integration
  tests will lean on to make the commenter resolvable.
