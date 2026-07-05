# Spec 02 — Phase 2: Threading / Replies

Forward design, concrete. Adds a single level of replies. Depends on Phase 1 (the author
projection is reused for reply commenters).

---

## 2.1 `ArticleCommentEntity` — parent reference + reply factory

**File:** `src/Modules/Content/Content/Domain/Entities/ArticleCommentEntity.cs`

**Add properties** (after `DeletedAt`):

```csharp
/// <summary>
/// The parent comment this comment replies to, or null for a top-level comment.
/// </summary>
public Guid? ParentCommentId { get; private set; }

/// <summary>
/// Navigation to the parent comment, or null for a top-level comment.
/// </summary>
public ArticleCommentEntity? ParentComment { get; private set; }
```

**Add factory** (after `Create`):

```csharp
/// <summary>
/// Creates a reply to an existing top-level comment.
/// </summary>
/// <param name="id">The unique identifier for the reply.</param>
/// <param name="userId">The user who posted the reply.</param>
/// <param name="articleId">The article being commented on.</param>
/// <param name="parentCommentId">The top-level comment being replied to.</param>
/// <param name="body">The reply text.</param>
/// <returns>A new reply <see cref="ArticleCommentEntity" />.</returns>
public static ArticleCommentEntity CreateReply(
    Guid id,
    Guid userId,
    Guid articleId,
    Guid parentCommentId,
    string body
)
{
    return new ArticleCommentEntity
    {
        Id = id,
        UserId = userId,
        ArticleId = articleId,
        ParentCommentId = parentCommentId,
        Body = body,
        IsDeleted = false,
    };
}
```

Single-level rule is enforced in the add-reply handler (needs the parent loaded), not the
entity.

---

## 2.2 `ArticleCommentConfiguration` — self-FK + index

**File:** `src/Modules/Content/Content/Infrastructure/Persistence/Configurations/ArticleCommentConfiguration.cs`

**Add** inside `Configure`:

```csharp
builder.Property(x => x.ParentCommentId).IsRequired(false);

builder
    .HasOne(x => x.ParentComment)
    .WithMany()
    .HasForeignKey(x => x.ParentCommentId)
    .OnDelete(DeleteBehavior.Restrict);

builder.HasIndex(x => x.ParentCommentId).HasDatabaseName("ix_article_comments_parent");
```

---

## 2.3 Migration

```bash
dotnet ef migrations add AddArticleCommentParent \
  --project src/Modules/Content/Content.Infrastructure \
  --startup-project src/Api \
  --context ContentDbContext
```

Adds `parent_comment_id uuid NULL`, the FK (`RESTRICT`), and the index. No backfill.

---

## 2.4 `ArticleCommentDto` — parent + replies

**File:** `src/Modules/Content/Content/Application/Shared/DTOs/ArticleCommentDto.cs`

**Extend** (append after `Author`):

```csharp
/// <param name="ParentCommentId">
/// The parent comment ID if this is a reply, or null for a top-level comment.
/// </param>
/// <param name="ReplyCount">
/// The number of non-deleted direct replies to this comment.
/// </param>
/// <param name="Replies">
/// A bounded set of embedded replies, or null when replies are fetched lazily.
/// </param>
public record ArticleCommentDto(
    Guid Id,
    Guid UserId,
    string? Body,
    bool IsDeleted,
    AuthorDto? Author = null,
    Guid? ParentCommentId = null,
    int ReplyCount = 0,
    IReadOnlyList<ArticleCommentDto>? Replies = null
) : AuditableDto;
```

---

## 2.5 Repository — top-level filter + replies query

**File:** `src/Modules/Content/Content/Application/Shared/Repositories/IArticleRepository.cs`

`GetCommentsAsync` returns **top-level only** (add a `ParentCommentId == null` filter in the
specification/impl). **Add:**

```csharp
/// <summary>
/// Returns a paginated list of non-deleted replies to a comment, along with total count.
/// </summary>
Task<(List<ArticleCommentEntity> Replies, int TotalCount)> GetRepliesAsync(
    Guid parentCommentId,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default
);
```

Impl in `ArticleRepository` mirrors `GetCommentsAsync`, filtering `ParentCommentId == parentCommentId`,
ordered by `CreatedAt`.

---

## 2.6 Replies query slice

**Folder:** `src/Modules/Content/Content/Application/Interactions/UseCases/Public/Queries/GetCommentReplies/`

```csharp
/// <summary>
/// Query for retrieving a paginated list of replies to a comment.
/// </summary>
/// <param name="CommentId">The parent comment identifier.</param>
/// <param name="PaginatedRequest">Pagination parameters.</param>
public record PublicGetCommentRepliesQuery(Guid CommentId, PaginatedRequest PaginatedRequest)
    : IQuery<PublicGetCommentRepliesResult>;

/// <summary>
/// Result of the <see cref="PublicGetCommentRepliesQuery" /> containing paginated replies.
/// </summary>
/// <param name="Replies">Paginated reply DTOs.</param>
public record PublicGetCommentRepliesResult(PaginatedResult<ArticleCommentDto> Replies);
```

Handler mirrors `PublicGetArticleCommentsHandler` exactly (same `IUserLookupService` +
`IFileRepository` batch author projection), calling `GetRepliesAsync`.

Endpoint `PublicGetCommentRepliesEndpointV1`:
`GET /api/v1/public/articles/comments/{commentId:guid}/replies`, `AllowAnonymous`,
`ContentBrowsing` rate limit, produces `PaginatedResult<ArticleCommentDto>`.

---

## 2.7 Add-reply command slice

**Folder:** `src/Modules/Content/Content/Application/Interactions/UseCases/Public/Commands/AddCommentReply/`

Command `PublicAddCommentReplyCommand(Guid ArticleId, Guid ParentCommentId, Guid UserId, string Body)`.
Handler:
1. Load the article (`GetByIdOrThrowAsync`) and the parent comment (`GetCommentByIdAsync`).
2. **Enforce single level**: throw if the parent is itself a reply (`parent.ParentCommentId != null`) or belongs to a different article.
3. `ArticleCommentEntity.CreateReply(...)`, `AddCommentAsync`, `article.IncrementCommentCount()`, commit.
4. Return the mapped reply DTO with its author (reuse Phase 1 single-author resolution for the caller).

Endpoint `PublicAddCommentReplyEndpointV1`:
`POST /api/v1/public/articles/{id:guid}/comments/{commentId:guid}/replies`,
`RequireAuthorization`, validator (non-empty body, `MaxCommentBodyLength`).

---

## Tasks

- [ ] Add `ParentCommentId`/`ParentComment`/`CreateReply` to `ArticleCommentEntity`.
- [ ] Configure self-FK + index; add nullable column.
- [ ] Create `AddArticleCommentParent` migration.
- [ ] Extend `ArticleCommentDto` with `ParentCommentId`/`ReplyCount`/`Replies`.
- [ ] Filter top-level in `GetCommentsAsync`; add `GetRepliesAsync`.
- [ ] Add the `GetCommentReplies` query slice reusing the Phase 1 author projection.
- [ ] Add the `AddCommentReply` command slice with single-level enforcement.
