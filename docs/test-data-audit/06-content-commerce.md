# Content — commerce, catalogue, interactions, infrastructure

Area: everything under `Content.Unit.Tests/` and `Content.Integration.Tests/` except
`Application/Editorial`, `Domain` and `Application/Shared`. 370 files (Catalog 45+27, Commerce 49+18,
Interactions 55+37, Lookup 39+23, Infrastructure 34+36, fixtures 5). 22 read in full, ~14 in relevant
part, ~334 covered by greps that were exhaustive per pattern.

## The counting correction this area produced

`Mock*Factory` members are **extension methods**, invoked as `_mock.SetupX(...)`, so a census matching
`Class.Method(` never sees their call sites. Nine were wrongly reported as uncalled:

| Reported as 0 callers | Actual |
| --- | --- |
| `MockOrderPaymentFactory.SetupGetByOrderId` | **6 sites, 3 files** — `AdminGetOrderPaymentHandlerTests.cs:74, 94, 115, 146`, `AdminVerifyPaymentHandlerTests.cs:63`, `AdminAttachPaymentProofHandlerTests.cs:73` |
| `MockOrderPaymentFactory.SetupGetByOrderIdNotFound` | 2 — `AdminVerifyPaymentHandlerTests.cs:129`, `AdminAttachPaymentProofHandlerTests.cs:120` |
| `MockVerifyPaymentFactory.SetupVerifyAsync` | 1 — `AdminVerifyPaymentHandlerTests.cs:64` |
| `MockAddItemTierFactory.SetupAttachTierAsync` / `…Throws` | `AdminAddItemTierHandlerTests.cs:51` / `:75` |
| `MockAddOrderItemFactory.SetupCreateItemAsync` | `AdminAddOrderItemHandlerTests.cs:60` |
| `MockSubmitOrderFactory.SetupSubmitAsync` / `VerifySubmitCalled` | `AdminSubmitOrderHandlerTests.cs:55` / `:63` |

See [01-census.md](01-census.md) for the corrected totals.

## Missing factories — the interaction-entity cluster

Every one of these is `XEntity.Create(Guid.NewGuid(), userId, targetId[, channel])` with a throwaway id,
mirroring the existing `LyricsLikeFactory` / `LyricsShareFactory` / `VideoRatingFactory` shape.

| Proposed | Sites in this area |
| --- | --- |
| `ShortVideoLikeFactory.Create(Guid userId, Guid shortVideoId)` | **14** — unit `ShortVideoRepositoryTests.cs:299, 333, 357, 514, 515, 521, 522`; integration `:168, 219, 223`; `PublicGetOwnShortVideoFavoritesEndpointV1Tests.cs:79, 80, 81, 90` |
| `VideoShareFactory.Create` + `CreateAnonymous` | **14** — `VideoRepositoryTests.cs:796, 920, 926, 937, 938, 939, 963`; `PublicGetOwnVideoFavoritesEndpointV1Tests.cs:125, 129, 130, 131, 132, 171, 172` |
| `ShortVideoShareFactory.Create` + `CreateAnonymous` | **10** — `ShortVideoRepositoryTests.cs:488, 565, 567, 574, 575`; `PublicGetOwnShortVideoFavoritesEndpointV1Tests.cs:177, 178, 182, 183, 184` |
| `ArticleLikeFactory.Create(Guid userId, Guid articleId)` | **13** — `ArticleInteractionRepositoryTests.cs:77, 112, 135, 265, 266, 267, 297, 298, 324`; `PublicGetOwnArticleFavoritesEndpointV1Tests.cs:86, 87, 88, 242` |
| `ArticleBookmarkFactory.Create` | **10** — `ArticleInteractionRepositoryTests.cs:170, 205, 228, 270, 271, 301, 302, 389, 392`; `PublicGetOwnArticleBookmarksEndpointV1Tests.cs:41` |
| `ShortVideoBookmarkFactory.Create` | **9** — `ShortVideoRepositoryTests.cs:393, 427, 451, 540, 541`; integration `:221`; `PublicGetOwnShortVideoFavoritesEndpointV1Tests.cs:130, 137, 138` |
| `ArticleShareFactory.Create` + `CreateAnonymous` | **10** — `ArticleInteractionRepositoryTests.cs:365, 413, 420, 431, 432`; `PublicGetOwnArticleFavoritesEndpointV1Tests.cs:112, 113, 114, 115, 243` |
| `ArticleCommentLikeFactory.Create` | **7** — unit `ArticleCommentRepositoryTests.cs:363, 382, 383, 401`; integration `:141, 142`; `PublicLikeArticleCommentEndpointV1Tests.cs:115` |
| `ArticleTagFactory.Create(Guid articleId, Guid tagId)` | **7** — `AllTagsQueryBuilderTests.cs:62`, `PopularTagsQueryBuilderTests.cs:61`, `TagRepositoryTests.cs:119, 142, 164, 183, 184` |
| `VideoTagFactory.Create(Guid videoId, Guid tagId)` | **4** — `AllTagsQueryBuilderTests.cs:69`, `PopularTagsQueryBuilderTests.cs:68`, `TagRepositoryTests.cs:120, 143` |

## Missing factories — orders and comments

| Proposed | Sites |
| --- | --- |
| `ContentOrderFactory.CreateForCustomer(CustomerEntity)` | **17**, all identical `new ContentOrderBuilder().WithCustomer(customer).Build()` — `ContentOrderSpecificationTests.cs:48`, `ContentPaymentQueryBuilderTests.cs:66`, `ContentOrderQueryBuilderTests.cs:181, 182`, `AdminGetAllOrdersHandlerTests.cs:51`, `AdminGetPendingPaymentOrdersHandlerTests.cs:54`, `AdminGetAllPaymentsHandlerTests.cs:61, 85, 131`, `AdminGetCustomerOrdersHandlerTests.cs:52`, `AdminRemoveOrderItemHandlerTests.cs:58, 132`, `AdminEditOrderHandlerTests.cs:65, 154`, `AdminEditOrderItemHandlerTests.cs:66, 167`. Exactly equivalent to the existing `CreateForCustomer(customer.Id)` — because `WithCustomer` only assigns the FK, see [12-defects-found.md](12-defects-found.md) D2 |
| `ContentOrderFactory.CreateSubmittedForCustomer(CustomerEntity)` | 3 — `AdminEditOrderHandlerTests.cs:123`, `AdminEditOrderItemHandlerTests.cs:136`, `AdminRemoveOrderItemHandlerTests.cs:109` |
| `ContentOrderFactory.CreateManyForCustomer(CustomerEntity, int)` | 3 — `AdminGetAllOrdersHandlerTests.cs:49-52`, `AdminGetCustomerOrdersHandlerTests.cs:50-53`, `AdminGetPendingPaymentOrdersHandlerTests.cs:52-55`. Mirrors `CustomerFactory.CreateMany` |
| `ArticleCommentFactory.CreateReply(Guid articleId, Guid userId, Guid parentCommentId, string? body = null)` | **8+** — `CommentReplyAddedNotificationsHandlerTests.cs:58`, `GetOwnArticleFavoritesHandlersTests.cs:126`, `PublicGetCommentRepliesHandlerTests.cs:61`, `PublicAddCommentReplyHandlerTests.cs:95`, unit `ArticleCommentRepositoryTests.cs:203, 227, 234, 259, 266, 292`, and 4 integration sites. **Integration `ArticleCommentRepositoryTests.cs:156-157` already declares this as a private local `CreateReply`** — the factory exists, in the wrong place |
| `ArticleCommentFactory.Create(Guid articleId, Guid userId, string body)` overload | **15+** — `ArticleCommentRepositoryTests.cs:79, 111, 135, 136, 170, 202, 226, 257, 258, 290, 300, 302, 325, 327, 329, 336, 361, 378, 379, 399`. The existing `Create(articleId, userId)` pins `ValidCommentBody` and cannot serve bodies like `"top"`, `"c1"`, `"deleted"` |

## Missing mock factories

| Proposed | Sites |
| --- | --- |
| `MockArticleInteractionRepository.SetupApplyEngagementDelta` / `VerifyApplyEngagementDeltaCalled`, and the same pair on `MockVideoRepository`, `MockLyricsRepository`, `MockShortVideoRepository` | `ArticleEngagementHandlerTests.cs:63, 83, 110, 129` + `:71, 99`; `VideoEngagementHandlerTests.cs:59, 101, 136` + `:69`; `LyricsEngagementHandlerTests.cs:52, 74` + `:60`; `ShortVideoEngagementHandlerTests.cs:57, 79` + `:65` |
| `MockCommerceCustomerNotifier.Create()` | **8 files** — every `Commerce/EventHandlers/*Tests.cs` declares `private readonly Mock<ICommerceCustomerNotifier> _notifierMock = new();` while every other collaborator in those files goes through a mock class. Only `Create()` is shareable; the `.Verify(...)` shapes are per-handler and correctly direct |
| `MockCustomerRepository.SetupGetByIdAsync` / `…NotFound` | 6 — `AdminCreateOrderHandlerTests.cs:73, 95, 122, 142`; `CommerceCustomerNotifierTests.cs:123, 150`. `MockCategoryRepository.SetupGetByIdAsync` is the precedent; `MockCustomerRepository` has only the `OrThrow` variants |
| `MockCategoryRepository.SetupGetByIdOrThrowAny(CategoryEntity)` | 5 — `AdminCreateCategoryHandlerTests.cs:85, 151, 192, 223, 255`. The existing `SetupGetByIdOrThrow(category)` binds to `category.Id` and **cannot** substitute: the handler reloads by a freshly generated id |

Below the bar but noted: `MockPackageRepository.SetupGetByIdAsync` (2),
`MockVideoRepository.SetupSetRatingAsync` (2), `MockUserLookupService.SetupGetAuthorInfoByIdAsync`
singular (2).

## Duplication that is not factory-shaped

- **The in-memory `ContentDbContext` bootstrap, byte-identical in 25 files** — every
  `Infrastructure/Repositories/*Tests.cs` and `Infrastructure/Builders/*`:
  `new DbContextOptionsBuilder<ContentDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).AddInterceptors(new CreatedAtStampingInterceptor()).Options`
  plus an identical `Dispose()`. Verified identical in six of them. A `BaseContentRepositoryTest` or
  `ContentDbContextFactory.CreateInMemory()` in `Content.TestData` is the fix.
- **`SeedCategoryAsync()`** — identical bodies at `ArticleInteractionRepositoryTests.cs:57`,
  `ArticleCommentRepositoryTests.cs:58`, `VideoRepositoryTests.cs:58`, `ArticleRepositoryTests.cs:57`
  in this area and ~30 more across Catalog integration and Editorial.
- **`new Mapper(ContentMappingRegistration.CreateConfiguration())`** in all 9
  `Infrastructure/Mappers/*MapperTests.cs` in this area. They cannot inherit `BaseContentHandlerTest`
  (they already extend `BaseRepositoryTest`), so a static `ContentMapperFactory.Create()` is the shape.
- **Two copies of the same private `CreateMockFormFile()`** at
  `AdminUploadCategoryPosterValidatorTests.cs:99-106` and `AdminUploadCategoryPosterHandlerTests.cs:133-140`.
  `FileTestHelpers.CreateMockFormFile` covers three of the four setups but not `OpenReadStream` — needs an
  overload. Four more sites share the identical `proof.jpg` / `image/jpeg` pair
  (`AdminAttachPaymentProofHandlerTests.cs:89-91, 122-124, 147-149`, `AdminAttachPaymentProofValidatorTests.cs:35-37`).

## Existing helper hand-rolled — fix regardless of the 3+ rule

| Site | Use |
| --- | --- |
| `AbandonedDraftCleanupJobTests.cs:78, 100, 129, 155, 182` | `MockArticleRepository.SetupGetAbandonedDraftsAsync(articles)` — **byte-for-byte the same matchers**. Also `:50` → `MockArticleRepository.Create()`, `:51` → `MockContentUnitOfWork.Create()` |
| `CommentReplyAddedNotificationsHandlerTests.cs:66-71` | `MockArticleCommentRepository.SetupGetCommentByIdAsync(_parent)` / `(_reply)` |
| `PublicGetArticleCommentsHandlerTests.cs:63-72, 129-133`, `PublicGetCommentRepliesHandlerTests.cs:64-73` | `MockUserLookupService.SetupGetAuthorInfosByIds(dict)` — exists and is used at `AdminGetOrderPaymentHandlerTests.cs:116` |
| 10 raw `new Mock<IUserLookupService>()` / `<IFileStorageService>` / `<ICustomerRepository>` sites | the matching `Mock*.Create()` |
| `AdminAttachPaymentProofHandlerTests.cs:74-87` | `MockFileStorageService.SetupUpload(StoredFileFactory.From(proofFile))` — sets exactly those two members with the same matchers |
| `ContentOrderQueryBuilderTests.cs:219` | `ContentOrderFactory.CreateForCustomer(customerId)` — which the **same file** already uses at `:108, 109, 220` |
| `ContentPaymentQueryBuilderTests.cs:30, 111` | `ContentOrderFactory.Create()` |
| `AdminCreateOrderFactoryTests.cs:53-57, 80-84, 123-127, 145-149, 168-172` | `PackageSlotFactory.Create(package, category.Id, isRequired, quantity)` — already exists |
| `VideoRepositoryTests.cs:868-871, 898`, `PublicGetOwnVideoFavoritesEndpointV1Tests.cs:66, 67, 68, 93, 164, 165` | `VideoRatingFactory.Create(videoId, userId, stars)` — argument order flips |
| `AdminUpdateCategoryHandlerTests.cs:85-88` | delete — a redundant re-setup, see [12-defects-found.md](12-defects-found.md) D5 |
