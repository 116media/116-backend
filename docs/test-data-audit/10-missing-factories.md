# Missing factories and helpers

Every shape below is hand-rolled three or more times with nothing naming it, which is precisely the bar
the layer sets for itself. Ordered by sites, largest first. Sites are listed in the area documents where
they are too many to repeat here.

## The top of the list

| Sites | Add | Where it lives |
| --- | --- | --- |
| **72** | `PermissionFactory.CreateDefault()` | Identity. `PermissionFactory.Create(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)` across 15 files — `AdminAssignPermissionToRoleHandlerTests` (9), `AdminGetPermissionByIdHandlerTests` (8), `AdminRestorePermissionHandlerTests` (7), `AdminUpdatePermissionValidatorTests` (7), and 11 more. Precedent: `CategoryFactory.CreateDefault` |
| **22** | `LyricsFactory.CreatePublishedWithSlug(Guid categoryId, string slug)` | Content. `CreateWithSlug(...)` + `MarkPendingReview(); Approve(); Publish(...)` — exactly `LyricsBuilder.AsPublished()`. 12 sites in `PublicGetLyricsBySlugHandlerTests`, 8 in `PublicGetLyricsBySlugEndpointV1Tests` |
| **21** | `RoleFactory.CreateDefault()` | Identity. `Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription)` across 10 files |
| **21** | `AuthTestHelpers.CreateAuthenticationDto` / `CreateUserResponseDto` | Identity. Two duplicated private helpers — `TokenDeliveryServiceTests.cs:728-736` (`CreateAuthResult()`, 21 calls) and the integration twin at `:16-41`, which also hand-rolls a 14-argument `UserResponseDto` |
| **17** | `ContentOrderFactory.CreateForCustomer(CustomerEntity)` | Content commerce — see [06](06-content-commerce.md) |
| **14** | `SessionExportDtoFactory` (`Create`, `CreateWithIpAddress`, `CreateWithCreatedAt`, `CreateMobile`, `CreateMany`) | Identity. 14 hand-rolled 11-argument constructions across 5 files, three of them near-identical private helpers — and the `Csv`/`Xlsx` strategy helpers are **byte-identical** (md5-verified). Precedent: `Storage.TestData/Factories/FileReferenceDtoFactory` |
| **14** | `ShortVideoLikeFactory.Create` | Content interactions |
| **14** | `VideoShareFactory.Create` + `CreateAnonymous` | Content interactions |
| **13** | `ArticleLikeFactory.Create` | Content, split across Editorial (13) and interactions (13) |
| **11** | `NewsletterSubscriberFactory.Create(string email)` | Mailer — see [04](04-storage-mailer-shared-e2e.md) |
| **10** | `ArticleShareFactory.Create` + `CreateAnonymous`, `ArticleBookmarkFactory.Create`, `ShortVideoShareFactory` | Content interactions |
| **9** | `NotificationFactory.CreatePasswordChanged(Guid userId)` | Mailer. **Six byte-identical private helpers** across six files, plus two adoptable sites and one needing a title overload |
| **9** | `LyricsFactory.CreatePublishedForVideo(Guid, Guid)` overload | Content |
| **9** | `ShortVideoBookmarkFactory.Create` | Content interactions |
| **9** | `MockSessionRepository.SetupGetSessionByUserIdAndDeviceId{,ReturnsNull}` | Identity. Targets `GetSessionByUserIdAndDeviceIdAsync`, which **no** helper covers — the existing pair targets the distinct `GetActiveSession…` member |
| **8** | `MockAuthRepository.SetupGetOrCreateExternalUser` | Identity. Eight identical 9-line setups in `PublicSocialLoginAuthFactoryTests.cs` |
| **8** | `ArticleCommentFactory.CreateReply(...)` | Content. Already exists as a **private local helper** at integration `ArticleCommentRepositoryTests.cs:156-157` — the factory in the wrong place |
| **8** | `MockCommerceCustomerNotifier.Create()` | Content. No mock class exists for `ICommerceCustomerNotifier`; 8 files declare a raw `new()` |
| **7** | `PromotionLevelFactory.CreateWithSpotPriority(int)`, `ArticleCommentLikeFactory.Create`, `ArticleTagFactory.Create`, `ShortVideoViewEventFactory.CreateCounted/Uncounted` | Content |
| **6** | `MockSessionRepository.SetupUpdateRefreshToken` + `VerifyUpdateRefreshTokenCalled` | Identity. Six identical 6-line setups in `PublicRefreshTokenFactoryTests.cs` |
| **6** | `AuthTestHelpers.CreatePublicSignUpAuthData(UserEntity)` | Identity. `AuthDataBuilder` builds the Login/SocialLogin/AdminLogin trio but not SignUp |
| **6** | `MockCustomerRepository.SetupGetByIdAsync` / `…NotFound`, `LyricsFactory.CreatePublishedWithTags` | Content |
| **5** | `UserFactory.CreateExternalWithSubject(provider, subjectId, userName, email)`, `MockCategoryRepository.SetupGetByIdOrThrowAny`, `MockAccountLockoutRepository` (new class) | Identity / Content |
| **4** | `MockOtpRepository.SetupValidateUsedOtpNotFound`, `NewsletterSubscriberFactory.CreateUnsubscribed`, `OutboxEmailFactory.CreatePendingDueAt`, `VideoTagFactory.Create`, `VideoFactory.CreateWithShootingScheduledAt` | — |
| **3** | `OtpFactory.CreateConsumed()` ×2 overloads, `UserFactory.CreateVerifiedInactive()`, `ContentOrderFactory.CreateSubmittedForCustomer`, `CreateManyForCustomer`, `ShortVideoFactory.CreateWithTitle`, `VideoFactory.CreateWithTitleAndDescription`, `LyricsFactory.CreateWithLanguage`, `ArticleFactory.CreateWithCreatedAt`, `VideoTagFactory.Attach` | — |

## Not factory-shaped, but the same duplication

| Sites | What | Fix |
| --- | --- | --- |
| **25** | The in-memory `ContentDbContext` bootstrap, byte-identical (options + interceptor + `Dispose`) | `BaseContentRepositoryTest` or `ContentDbContextFactory.CreateInMemory()` in `Content.TestData` |
| **~34** | `SeedCategoryAsync()` with identical bodies | A seeding helper in `Content.TestData` |
| **12** | The Identity admin fixture: `RoleFactory.CreateAdmin()` + verified user + `UserRoleFactory.Create` + seed | Either `UserFactory.CreateAdminWithRole(email)` or a helper on `BaseApiTest` — the duplication is certain, the right home is not |
| **9** | `new Mapper(ContentMappingRegistration.CreateConfiguration())` | `ContentMapperFactory.Create()` |
| **6** | Two copies of a private `CreateMockFormFile`, plus four `proof.jpg` sites | An overload on the existing `FileTestHelpers.CreateMockFormFile` that also stubs `OpenReadStream` |
| **~20** | `SessionBuilder` has no `CreatedAt` default, so one file bolts `.WithCreatedAt(DateTime.UtcNow)` onto every session and can use no factory at all | Default `_createdAt` in the `SessionBuilder` constructor — but it changes what the builder produces for every consumer, so check the Application session tests first |

## Request builders that do not exist but should

Per-module convention: Content and Identity both use `*RequestBuilder` widely, so a missing one is a gap
rather than a style choice.

- **`AdminRejectLyricsRequest`** — newed inline 3× plus 3 anonymous objects, while `AdminRejectArticleRequestBuilder` and `AdminRejectVideoRequestBuilder` exist for the identical `record X(string Reason)` shape.
- **`AdminForceUnpromoteLyricsRequest`** — 7 inline sites, while the Article and Video twins exist.
- With no builder and 3+ sites each: `AdminCreateArtistRequest` (7 parameters, 11 sites), `AdminUpdateArtistRequest` (6, 6), `AdminCreateAlbumRequest` (5, 6), `AdminUpdateAlbumRequest` (4, 6), `AdminUpdateLyricsMetadataRequest` (5, 6), `PublicSubmitLyricsRequest` (5, 7).
- **The Artist/Album/Streaming-link slice of Editorial has none at all** — roughly 30 request types constructed inline. Measured by grep across those files rather than by reading each, so the count is structural.
- Identity: extend `AdminUpdateOwnProfileRequestBuilder` with `WithUserName`, `WithCountryName`, `WithCountryIsoCode`, `WithPartialPhoneNumber` — three tests new the record inline today *because* the builder cannot express their shape.
