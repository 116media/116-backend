# Implementation Specs — Article Comment Authors

Every file, type, property, and test to add or change, with full C# and multiline XML docs,
following `apps/backend/CLAUDE.md` conventions (CQRS vertical slices, one class per file,
`Public`/`Admin` prefixes on use-case files and types, multiline XML doc comments).

| Spec | Phase | Content |
|------|-------|---------|
| [01-dto-and-handler.md](01-dto-and-handler.md) | Phase 1 (must-have) | `IUserLookupService` batch method + impl, `IFileRepository` batch avatars, `ArticleCommentDto.Author`, `ArticleMapper` overload, `PublicGetArticleCommentsHandler` |
| [02-threading.md](02-threading.md) | Phase 2 | `ParentCommentId` on entity/DTO/EF, migration, replies query + add-reply command + endpoints |
| [03-comment-likes.md](03-comment-likes.md) | Phase 3 | `ArticleCommentLikeEntity`, cached `LikeCount`, EF config, migration, `LikeCount`/`IsLiked` on DTO, like/unlike endpoints |
| [04-tests.md](04-tests.md) | All | Unit + integration test bodies mirroring the existing comment tests |

Read [../00-overview.md](../00-overview.md) for the phased plan and decisions before
implementing. Phase 1 is implementation-ready; Phases 2–3 are concrete forward designs.
