# Content Module - Integration Test Coverage Gaps

**Current Coverage:** 95.6% (11,515 / 12,039 lines) | Branch: 56%
**Uncovered Lines:** 524
**Target:** 100% of all specifications, validators, query builders, and error messages

## 1. Cloudinary-Blocked Analysis

The handlers `CreateShortVideoHandler`, `UpdateShortVideoHandler`, and `UploadArticleImageHandler` depend on `CloudinaryService` which is stubbed in tests. These handlers (~21 lines) cannot be covered without a real Cloudinary test double.

However, their validators are pure FluentValidation with zero Cloudinary dependency. Sending invalid payloads triggers a 400 response from the validator before the handler runs, covering validator lines without Cloudinary.

### CreateShortVideoValidator

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `CreateShortVideo_EmptyTitle_ReturnsBadRequest` | Admin auth token | `POST /admin/short-videos` with `title: ""` | 400 with title required error | `ValidShortVideoTitle` required branch |
| 2 | `CreateShortVideo_TitleExceedsMaxLength_ReturnsBadRequest` | Admin auth token | `POST /admin/short-videos` with title of 201+ chars | 400 with title max length error | `ValidShortVideoTitle` max length branch |
| 3 | `CreateShortVideo_EmptySlug_ReturnsBadRequest` | Admin auth token | `POST /admin/short-videos` with `slug: ""` | 400 with slug required error | `ValidShortVideoSlug` required branch |
| 4 | `CreateShortVideo_InvalidSlugFormat_ReturnsBadRequest` | Admin auth token | `POST /admin/short-videos` with `slug: "INVALID SLUG!"` | 400 with slug format error | `ValidShortVideoSlug` format branch |
| 5 | `CreateShortVideo_NullVideoFile_ReturnsBadRequest` | Admin auth token | `POST /admin/short-videos` with no file attached | 400 with file required error | `ValidShortVideoFile` null check |
| 6 | `CreateShortVideo_EmptyVideoFile_ReturnsBadRequest` | Admin auth token | `POST /admin/short-videos` with empty (0-byte) file | 400 with file empty error | `ValidShortVideoFile` empty check |
| 7 | `CreateShortVideo_OversizedVideoFile_ReturnsBadRequest` | Admin auth token | `POST /admin/short-videos` with file exceeding max size | 400 with file size error | `ValidShortVideoFile` size check |
| 8 | `CreateShortVideo_WrongFileExtension_ReturnsBadRequest` | Admin auth token | `POST /admin/short-videos` with `.txt` file | 400 with file extension error | `ValidShortVideoFile` extension check |

### UpdateShortVideoValidator

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `UpdateShortVideo_InvalidGuidId_ReturnsBadRequest` | Admin auth token | `PUT /admin/short-videos/not-a-guid` | 400 with invalid GUID error | `IsValidGuid` on Id |
| 2 | `UpdateShortVideo_TitleExceedsMaxLength_ReturnsBadRequest` | Admin auth token, seeded short video | `PUT /admin/short-videos/{id}` with title of 201+ chars | 400 with title max length error | `ValidShortVideoTitle(isRequired=false)` max length branch |
| 3 | `UpdateShortVideo_OversizedVideoFile_ReturnsBadRequest` | Admin auth token, seeded short video | `PUT /admin/short-videos/{id}` with oversized file | 400 with file size error | `ValidShortVideoFile` size check (optional path) |
| 4 | `UpdateShortVideo_WrongFileExtension_ReturnsBadRequest` | Admin auth token, seeded short video | `PUT /admin/short-videos/{id}` with `.txt` file | 400 with file extension error | `ValidShortVideoFile` extension check (optional path) |

### UploadArticleImageValidator

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `UploadArticleImage_InvalidGuidArticleId_ReturnsBadRequest` | Admin auth token | `POST /admin/articles/not-a-guid/image` | 400 with invalid GUID error | `IsValidGuid` on ArticleId |
| 2 | `UploadArticleImage_NoFile_ReturnsBadRequest` | Admin auth token, seeded article | `POST /admin/articles/{id}/image` with no file | 400 with file required error | File not-null validation |
| 3 | `UploadArticleImage_EmptyFile_ReturnsBadRequest` | Admin auth token, seeded article | `POST /admin/articles/{id}/image` with 0-byte file | 400 with file empty error | File empty check |
| 4 | `UploadArticleImage_WrongFileExtension_ReturnsBadRequest` | Admin auth token, seeded article | `POST /admin/articles/{id}/image` with `.exe` file | 400 with file extension error | `ValidArticleImageFile` extension check |

### Structurally Blocked (Skip)

| Component | Coverage | Lines | Reason |
|-----------|----------|-------|--------|
| `CreateShortVideoHandler` | 0% | 7 | Cloudinary dependency |
| `UpdateShortVideoHandler` | 0% | 7 | Cloudinary dependency |
| `UploadArticleImageHandler` | 0% | 7 | Cloudinary dependency |
| `YoutubeThumbnailService` | 0% | ~10 | External HTTP dependency |
| `AbandonedDraftCleanupJob` | 0% | ~10 | Background job, not HTTP-triggered |

## 2. Specifications - Target 100%

### ArticleTagByArticleIdSpecification (0%)

Used by `ArticleRepository.GetTagsByArticleIdAsync`, called from `AdminUpdateArticleTagsHandler`.

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `UpdateArticleTags_ArticleWithExistingTags_ReplacesOldTags` | Admin token, article with 2 existing tags, 2 new tags | `PUT /admin/articles/{id}/tags` with `tagNames: ["new-tag-1", "new-tag-2"]` | 200, article has only new tags | `ArticleTagByArticleIdSpecification` predicate (handler fetches existing tags via this spec before deleting and re-adding) |

### GossipArticleSpecification (0%)

Used by `ArticleRepository.GetGossipFallbackAsync`, called from `PublicGetArticlePromotionFeedHandler`.

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `GetArticlePromotionFeed_FallbackToGossip_ReturnsGossipArticles` | Gossip category (seeded by name/slug matching gossip convention), 3 published articles in gossip category, no promoted articles | `GET /public/articles/promotion-feed` | 200 with gossip articles as fallback | `GossipArticleSpecification` predicate (triggered when no promoted articles exist, handler falls back to gossip) |

### ShortVideoBookmarkByUserAndShortVideoSpecification (0%)

Used by `ShortVideoRepository` bookmark check, called from `PublicBookmarkShortVideoHandler` and `PublicUnbookmarkShortVideoHandler`.

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `BookmarkShortVideo_Success_CreatesBookmark` | User auth token, active published short video | `POST /public/short-videos/{id}/bookmark` | 200/201 | `ShortVideoBookmarkByUserAndShortVideoSpecification` predicate (handler checks existing bookmark via this spec) |
| 2 | `UnbookmarkShortVideo_AfterBookmark_RemovesBookmark` | User auth token, short video bookmarked by user | `DELETE /public/short-videos/{id}/bookmark` | 200/204 | `ShortVideoBookmarkByUserAndShortVideoSpecification` predicate (handler finds existing bookmark via this spec) |

### VideoByOrderItemIdSpecification (0%)

Used by `VideoRepository.GetByOrderItemIdAsync`, called from `AdminVerifyPaymentFactory`.

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `VerifyPayment_OrderWithVideoItem_ActivatesVideo` | Admin token, customer, video, content order with video order item, submitted order with payment | `PATCH /admin/orders/{id}/verify-payment` | 200, payment verified, video activated | `VideoByOrderItemIdSpecification` predicate (factory looks up video by order item ID during payment verification) |

### TagByNameSpecification (0%) - DEAD CODE

Zero callers in the entire codebase. Cannot be covered by integration tests. Recommend removal or exclusion from coverage target.

## 3. Error Classes - Target 100%

### ArticleErrors (28.5% - 12 methods)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `GetArticleBySlug_NonExistent_Returns404` | None | `GET /public/articles/non-existent-slug` | 404 | `ArticleErrors.NotFound(id)` |
| 2 | `CreateArticle_DuplicateSlug_Returns409` | Admin token, existing article with slug "test-slug" | `POST /admin/articles` with `slug: "test-slug"` | 409 | `ArticleErrors.SlugAlreadyExists(slug)` |
| 3 | `UpdateArticle_DuplicateSlug_Returns409` | Admin token, article A with slug "slug-a", article B | `PUT /admin/articles/{B.id}` with `slug: "slug-a"` | 409 | `ArticleErrors.SlugAlreadyExists(slug)` via update path |
| 4 | `CreateArticle_EmptyTitle_Returns400` | Admin token | `POST /admin/articles` with `title: null` (bypassing validator) | 400 | `ArticleErrors.TitleRequired()` |
| 5 | `CreateArticle_EmptySlug_Returns400` | Admin token | `POST /admin/articles` with `slug: null` (bypassing validator) | 400 | `ArticleErrors.SlugRequired()` |
| 6 | `SubmitArticle_AlreadySubmitted_Returns409` | Admin token, article in Submitted status | `PATCH /admin/articles/{id}/submit` | 409 | `ArticleErrors.AlreadySubmitted()` |
| 7 | `SubmitArticle_AlreadyPendingReview_Returns409` | Admin token, article in PendingReview status | `PATCH /admin/articles/{id}/submit` | 409 | `ArticleErrors.AlreadyPendingReview()` |
| 8 | `ApproveArticle_AlreadyApproved_Returns409` | Admin token, article in Approved status | `PATCH /admin/articles/{id}/approve` | 409 | `ArticleErrors.AlreadyApproved()` |
| 9 | `PublishArticle_AlreadyPublished_Returns409` | Admin token, article in Published status | `PATCH /admin/articles/{id}/publish` | 409 | `ArticleErrors.AlreadyPublished()` |
| 10 | `RejectArticle_AlreadyRejected_Returns409` | Admin token, article in Rejected status | `PATCH /admin/articles/{id}/reject` with reason | 409 | `ArticleErrors.AlreadyRejected()` |
| 11 | `ArchiveArticle_AlreadyArchived_Returns409` | Admin token, article in Archived status | `PATCH /admin/articles/{id}/archive` | 409 | `ArticleErrors.AlreadyArchived()` |
| 12 | `PublishArticle_FromDraft_Returns400` | Admin token, article in Draft status (not submitted/approved) | `PATCH /admin/articles/{id}/publish` | 400 | `ArticleErrors.InvalidStatusTransition(from, to)` |
| 13 | `DeleteArticle_Published_Returns400` | Admin token, published article | `DELETE /admin/articles/{id}` | 400 | `ArticleErrors.CannotDeletePublishedArticle()` |

### ArticleInteractionErrors (62.5% - 6 methods)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `LikeArticle_AlreadyLiked_Returns409` | User token, published article already liked by user | `POST /public/articles/{id}/like` | 409 | `ArticleInteractionErrors.AlreadyLiked()` |
| 2 | `UnlikeArticle_NeverLiked_Returns400` | User token, published article not liked by user | `DELETE /public/articles/{id}/like` | 400 | `ArticleInteractionErrors.LikeNotFound()` |
| 3 | `BookmarkArticle_AlreadyBookmarked_Returns409` | User token, published article already bookmarked by user | `POST /public/articles/{id}/bookmark` | 409 | `ArticleInteractionErrors.AlreadyBookmarked()` |
| 4 | `UnbookmarkArticle_NeverBookmarked_Returns400` | User token, published article not bookmarked by user | `DELETE /public/articles/{id}/bookmark` | 400 | `ArticleInteractionErrors.BookmarkNotFound()` |
| 5 | `EditArticleComment_NonExistent_Returns404` | User token, published article | `PUT /public/articles/{articleId}/comments/{nonExistentId}` | 404 | `ArticleInteractionErrors.CommentNotFound(id)` |
| 6 | `DeleteArticleComment_NotOwner_Returns400` | User A token, published article, comment created by User B | `DELETE /public/articles/{articleId}/comments/{commentId}` as User A | 400 | `ArticleInteractionErrors.NotCommentOwner()` |
| 7 | `EditArticleComment_NotOwner_Returns400` | User A token, published article, comment created by User B | `PUT /public/articles/{articleId}/comments/{commentId}` as User A with new text | 400 | `ArticleInteractionErrors.NotCommentOwner()` via edit path |

### CategoryErrors (35.7% - 12 methods)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `CreateCategory_DuplicateSlug_Returns409` | Admin token, existing category with slug "music" | `POST /admin/categories` with `slug: "music"` | 409 | `CategoryErrors.AlreadyExists(slug)` |
| 2 | `AddPackageSlot_NonExistentCategory_Returns404` | Admin token, package | `POST /admin/packages/{id}/slots` with non-existent categoryId | 404 | `CategoryErrors.NotFound(id)` |
| 3 | `ActivateCategory_AlreadyActive_Returns409` | Admin token, active category | `PATCH /admin/categories/{id}/activate` | 409 | `CategoryErrors.AlreadyActive()` |
| 4 | `DeactivateCategory_AlreadyInactive_Returns409` | Admin token, inactive category | `PATCH /admin/categories/{id}/deactivate` | 409 | `CategoryErrors.AlreadyInactive()` |
| 5 | `CreateCategory_EmptyName_Returns400` | Admin token | `POST /admin/categories` with `name: ""` (bypassing validator) | 400 | `CategoryErrors.NameRequired()` |
| 6 | `CreateCategory_EmptySlug_Returns400` | Admin token | `POST /admin/categories` with `slug: ""` (bypassing validator) | 400 | `CategoryErrors.SlugRequired()` |
| 7 | `AddCategoryPricing_DuplicateTier_Returns409` | Admin token, category with existing pricing for tier X | `POST /admin/categories/{id}/pricing` with same tier | 409 | `CategoryErrors.PricingAlreadyExists()` |
| 8 | `RemoveCategoryPricing_NonExistent_Returns404` | Admin token, category without pricing for tier X | `DELETE /admin/categories/{catId}/pricing/{tierId}` | 404 | `CategoryErrors.PricingNotFound(catId, tierId)` |
| 9 | `AddCategoryPricing_NegativePrice_Returns400` | Admin token, category, active pricing tier | `POST /admin/categories/{id}/pricing` with `priceUsd: -1` | 400 | `CategoryErrors.PriceMustBeNonNegative()` |
| 10 | `SetExclusiveCategory_InactiveCategory_Returns400` | Admin token, inactive video category | `PATCH /admin/categories/{id}/exclusive` | 400 | `CategoryErrors.CannotMakeInactiveExclusive()` |
| 11 | `SetExclusiveCategory_NonVideoCategory_Returns400` | Admin token, active non-video category | `PATCH /admin/categories/{id}/exclusive` | 400 | `CategoryErrors.OnlyVideoCategoryCanBeExclusive()` |
| 12 | `GetExclusiveCategory_NoneSet_Returns404` | No exclusive category configured | `GET /public/categories/exclusive` | 404 | `CategoryErrors.NoExclusiveCategoryFound()` |

### ContentOrderErrors (46.6% - 13 methods)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `GetOrder_NonExistent_Returns404` | Admin token | `GET /admin/orders/{nonExistentId}` | 404 | `ContentOrderErrors.NotFound(id)` |
| 2 | `AddItemTier_NonExistentItem_Returns404` | Admin token, draft order with no matching item | `POST /admin/orders/{orderId}/items/{nonExistentItemId}/tiers` | 404 | `ContentOrderErrors.ItemNotFound(itemId)` |
| 3 | `AddItemTier_DuplicateTier_Returns409` | Admin token, draft order with item already having tier X | `POST /admin/orders/{orderId}/items/{itemId}/tiers` with same tier | 409 | `ContentOrderErrors.TierAlreadyAttached()` |
| 4 | `VerifyPayment_OrderWithoutPayment_Returns404` | Admin token, submitted order with no payment record | `PATCH /admin/orders/{id}/verify-payment` | 404 | `ContentOrderErrors.PaymentNotFound(orderId)` |
| 5 | `SubmitOrder_AlreadySubmitted_Returns409` | Admin token, already-submitted order | `PATCH /admin/orders/{id}/submit` | 409 | `ContentOrderErrors.AlreadySubmitted()` |
| 6 | `VerifyPayment_AlreadyPaid_Returns409` | Admin token, order already marked as paid | `PATCH /admin/orders/{id}/verify-payment` | 409 | `ContentOrderErrors.AlreadyPaid()` |
| 7 | `CancelOrder_AlreadyCancelled_Returns409` | Admin token, already-cancelled order | `PATCH /admin/orders/{id}/cancel` | 409 | `ContentOrderErrors.AlreadyCancelled()` |
| 8 | `CancelOrder_PaidOrder_Returns400` | Admin token, paid order | `PATCH /admin/orders/{id}/cancel` | 400 | `ContentOrderErrors.CannotCancelPaidOrder()` |
| 9 | `AddOrderItem_SubmittedOrder_Returns400` | Admin token, submitted order | `POST /admin/orders/{id}/items` | 400 | `ContentOrderErrors.CannotAddItemToNonDraftOrder()` |
| 10 | `SubmitOrder_NoItemsOrTiers_Returns400` | Admin token, draft order with no items | `PATCH /admin/orders/{id}/submit` | 400 | `ContentOrderErrors.MustHaveAtLeastOneItemWithTier()` |
| 11 | `AddItemTier_NonExistentTier_Returns404` | Admin token, draft order with item | `POST /admin/orders/{orderId}/items/{itemId}/tiers` with non-existent tierId | 404 | `ContentOrderErrors.ItemTierNotFound(tierId)` |
| 12 | `VerifyPayment_AlreadyVerified_Returns409` | Admin token, order with already-verified payment | `PATCH /admin/orders/{id}/verify-payment` | 409 | `ContentOrderErrors.PaymentAlreadyVerified()` |
| 13 | `RejectPayment_AlreadyRejected_Returns409` | Admin token, order with already-rejected payment | `PATCH /admin/orders/{id}/reject-payment` | 409 | `ContentOrderErrors.PaymentAlreadyRejected()` |

### ContentTypeErrors (57.1% - 5 methods)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `CreateContentType_DuplicateName_Returns409` | Admin token, existing content type "Video" | `POST /admin/content-types` with `name: "Video"` | 409 | `ContentTypeErrors.AlreadyExists(name)` |
| 2 | `ActivateContentType_AlreadyActive_Returns409` | Admin token, active content type | `PATCH /admin/content-types/{id}/activate` | 409 | `ContentTypeErrors.AlreadyActive()` |
| 3 | `DeactivateContentType_AlreadyInactive_Returns409` | Admin token, inactive content type | `PATCH /admin/content-types/{id}/deactivate` | 409 | `ContentTypeErrors.AlreadyInactive()` |
| 4 | `CreateContentType_EmptyName_Returns400` | Admin token | `POST /admin/content-types` with `name: ""` (bypassing validator) | 400 | `ContentTypeErrors.NameRequired()` |

**Note:** `ContentTypeErrors.NotFound(id)` has no handler caller. Classified as dead code (Section 9).

### CustomerErrors (66.6% - 4 methods)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `CreateCustomer_DuplicateEmail_Returns409` | Admin token, existing customer with email "test@example.com" | `POST /admin/customers` with `email: "test@example.com"` | 409 | `CustomerErrors.AlreadyExists(email)` |
| 2 | `CreateOrder_NonExistentCustomer_Returns404` | Admin token | `POST /admin/orders` with non-existent customerId | 404 | `CustomerErrors.NotFound(id)` |
| 3 | `CreateCustomer_EmptyFullName_Returns400` | Admin token | `POST /admin/customers` with `fullName: ""` (bypassing validator) | 400 | `CustomerErrors.FullNameRequired()` |
| 4 | `CreateCustomer_EmptyEmail_Returns400` | Admin token | `POST /admin/customers` with `email: ""` (bypassing validator) | 400 | `CustomerErrors.EmailRequired()` |

### LyricsErrors (42.8% - 5 methods)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `GetLyricsByVideoId_NoLyrics_Returns404` | Published video without lyrics | `GET /public/videos/{videoId}/lyrics` | 404 | `LyricsErrors.NotFound(id)` |
| 2 | `CreateLyrics_DuplicateSongAndArtist_Returns409` | Admin token, existing lyrics with song "Title" by "Artist" | `POST /admin/lyrics` with same `songTitle: "Title"` and `artistName: "Artist"` | 409 | `LyricsErrors.AlreadyExists(song, artist)` |
| 3 | `CreateLyrics_EmptySongTitle_Returns400` | Admin token | `POST /admin/lyrics` with `songTitle: ""` (bypassing validator) | 400 | `LyricsErrors.SongTitleRequired()` |
| 4 | `CreateLyrics_EmptyArtistName_Returns400` | Admin token | `POST /admin/lyrics` with `artistName: ""` (bypassing validator) | 400 | `LyricsErrors.ArtistNameRequired()` |
| 5 | `CreateLyrics_EmptyLyricsText_Returns400` | Admin token | `POST /admin/lyrics` with `lyricsText: ""` (bypassing validator) | 400 | `LyricsErrors.LyricsTextRequired()` |

### PackageErrors (44.4% - 7 methods)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `CreateOrder_NonExistentPackage_Returns404` | Admin token, existing customer | `POST /admin/orders` with non-existent packageId | 404 | `PackageErrors.NotFound(id)` |
| 2 | `ActivatePackage_AlreadyActive_Returns409` | Admin token, active package | `PATCH /admin/packages/{id}/activate` | 409 | `PackageErrors.AlreadyActive()` |
| 3 | `DeactivatePackage_AlreadyInactive_Returns409` | Admin token, inactive package | `PATCH /admin/packages/{id}/deactivate` | 409 | `PackageErrors.AlreadyInactive()` |
| 4 | `CreatePackage_EmptyName_Returns400` | Admin token | `POST /admin/packages` with `name: ""` (bypassing validator) | 400 | `PackageErrors.NameRequired()` |
| 5 | `CreatePackage_NegativePrice_Returns400` | Admin token | `POST /admin/packages` with `price: -1` | 400 | `PackageErrors.PriceMustBeNonNegative()` |
| 6 | `AddPackageSlot_ZeroQuantity_Returns400` | Admin token, active package, active category | `POST /admin/packages/{id}/slots` with `quantity: 0` | 400 | `PackageErrors.SlotQuantityMustBePositive()` |
| 7 | `RemovePackageSlot_NonExistent_Returns404` | Admin token, package with no matching slot | `DELETE /admin/packages/{packageId}/slots/{nonExistentSlotId}` | 404 | `PackageErrors.SlotNotFound(slotId)` |

### PlaylistErrors (60% - 3 methods)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `GetPlaylist_NonExistent_Returns404` | User token | `GET /public/playlists/{nonExistentId}` | 404 | `PlaylistErrors.NotFound(id)` |
| 2 | `DeletePlaylist_NotOwner_Returns400` | User A token, playlist owned by User B | `DELETE /public/playlists/{id}` as User A | 400 | `PlaylistErrors.NotOwner()` |
| 3 | `RenamePlaylist_NotOwner_Returns400` | User A token, playlist owned by User B | `PATCH /public/playlists/{id}/rename` as User A | 400 | `PlaylistErrors.NotOwner()` via rename path |
| 4 | `AddVideoToPlaylist_AlreadyInPlaylist_Returns409` | User token, playlist with video already added | `POST /public/playlists/{id}/videos` with same videoId | 409 | `PlaylistErrors.VideoAlreadyInPlaylist()` |

### PricingTierErrors (25% - 6 methods)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `CreatePricingTier_DuplicateName_Returns409` | Admin token, existing tier "Gold" | `POST /admin/pricing-tiers` with `name: "Gold"` | 409 | `PricingTierErrors.AlreadyExists(name)` |
| 2 | `AddItemTier_NonExistentPricingTier_Returns404` | Admin token, draft order with item | `POST /admin/orders/{orderId}/items/{itemId}/tiers` with non-existent pricingTierId | 404 | `PricingTierErrors.NotFound(id)` |
| 3 | `ActivatePricingTier_AlreadyActive_Returns409` | Admin token, active pricing tier | `PATCH /admin/pricing-tiers/{id}/activate` | 409 | `PricingTierErrors.AlreadyActive()` |
| 4 | `DeactivatePricingTier_AlreadyInactive_Returns409` | Admin token, inactive pricing tier | `PATCH /admin/pricing-tiers/{id}/deactivate` | 409 | `PricingTierErrors.AlreadyInactive()` |
| 5 | `AddCategoryPricing_InactiveTier_Returns400` | Admin token, active category, inactive pricing tier | `POST /admin/categories/{id}/pricing` with inactive tierId | 400 | `PricingTierErrors.IsInactive()` |
| 6 | `CreatePricingTier_EmptyName_Returns400` | Admin token | `POST /admin/pricing-tiers` with `name: ""` (bypassing validator) | 400 | `PricingTierErrors.NameRequired()` |

### PromotionLevelErrors (20% - 8 methods)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `CreatePromotionLevel_DuplicateName_Returns409` | Admin token, existing level "Premium" | `POST /admin/promotion-levels` with `name: "Premium"` | 409 | `PromotionLevelErrors.AlreadyExists(name)` |
| 2 | `AddOrderItem_NonExistentPromotion_Returns404` | Admin token, draft order | `POST /admin/orders/{id}/items` with non-existent promotionLevelId | 404 | `PromotionLevelErrors.NotFound(id)` |
| 3 | `ActivatePromotionLevel_AlreadyActive_Returns409` | Admin token, active promotion level | `PATCH /admin/promotion-levels/{id}/activate` | 409 | `PromotionLevelErrors.AlreadyActive()` |
| 4 | `DeactivatePromotionLevel_AlreadyInactive_Returns409` | Admin token, inactive promotion level | `PATCH /admin/promotion-levels/{id}/deactivate` | 409 | `PromotionLevelErrors.AlreadyInactive()` |
| 5 | `CreatePromotionLevel_EmptyName_Returns400` | Admin token | `POST /admin/promotion-levels` with `name: ""` (bypassing validator) | 400 | `PromotionLevelErrors.NameRequired()` |
| 6 | `CreatePromotionLevel_ZeroDuration_Returns400` | Admin token | `POST /admin/promotion-levels` with `durationDays: 0` | 400 | `PromotionLevelErrors.DurationMustBePositive()` |
| 7 | `CreatePromotionLevel_NegativePrice_Returns400` | Admin token | `POST /admin/promotion-levels` with `priceUsd: -1` | 400 | `PromotionLevelErrors.PriceMustBeNonNegative()` |
| 8 | `CreatePromotionLevel_InvalidSpotPriority_Returns400` | Admin token | `POST /admin/promotion-levels` with `spotPriority: 0` (or 4) | 400 | `PromotionLevelErrors.InvalidSpotPriority()` |

### ShortVideoErrors (71.4% - 5 methods)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `GetShortVideoBySlug_NonExistent_Returns404` | None | `GET /public/short-videos/non-existent-slug` | 404 | `ShortVideoErrors.NotFound(id)` |
| 2 | `ActivateShortVideo_AlreadyActive_Returns409` | Admin token, active short video | `PATCH /admin/short-videos/{id}/activate` | 409 | `ShortVideoErrors.AlreadyActive()` |
| 3 | `DeactivateShortVideo_AlreadyInactive_Returns409` | Admin token, inactive short video | `PATCH /admin/short-videos/{id}/deactivate` | 409 | `ShortVideoErrors.AlreadyInactive()` |

**Note:** `ShortVideoErrors.TitleRequired()` is covered transitively via `CreateShortVideoValidator` tests (Section 1). `ShortVideoErrors.SlugAlreadyExists(slug)` is blocked by Cloudinary handler dependency.

### ShortVideoInteractionErrors (16.6% - 4 methods)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `LikeShortVideo_AlreadyLiked_Returns409` | User token, active short video already liked by user | `POST /public/short-videos/{id}/like` | 409 | `ShortVideoInteractionErrors.AlreadyLiked()` |
| 2 | `UnlikeShortVideo_NeverLiked_Returns400` | User token, active short video not liked by user | `DELETE /public/short-videos/{id}/like` | 400 | `ShortVideoInteractionErrors.LikeNotFound()` |
| 3 | `BookmarkShortVideo_AlreadyBookmarked_Returns409` | User token, active short video already bookmarked by user | `POST /public/short-videos/{id}/bookmark` | 409 | `ShortVideoInteractionErrors.AlreadyBookmarked()` |
| 4 | `UnbookmarkShortVideo_NeverBookmarked_Returns400` | User token, active short video not bookmarked by user | `DELETE /public/short-videos/{id}/bookmark` | 400 | `ShortVideoInteractionErrors.BookmarkNotFound()` |

### TagErrors (50% - 4 methods)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `CreateTag_DuplicateSlug_Returns409` | Admin token, existing tag with slug "music" | `POST /admin/tags` with `slug: "music"` | 409 | `TagErrors.SlugAlreadyExists(slug)` |
| 2 | `CreateTag_EmptyName_Returns400` | Admin token | `POST /admin/tags` with `name: ""` (bypassing validator) | 400 | `TagErrors.NameRequired()` |
| 3 | `CreateTag_EmptySlug_Returns400` | Admin token | `POST /admin/tags` with `slug: ""` (bypassing validator) | 400 | `TagErrors.SlugRequired()` |

**Note:** `TagErrors.NotFound(id)` has no handler caller. Classified as dead code (Section 9).

### VideoErrors (66.6% - 14 methods)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `GetVideoBySlug_NonExistent_Returns404` | None | `GET /public/videos/non-existent-slug` | 404 | `VideoErrors.NotFound(id)` |
| 2 | `ForceUnpromoteVideo_NonExistent_Returns404` | Admin token | `PATCH /admin/videos/{nonExistentId}/force-unpromote` | 404 | `VideoErrors.NotFound(id)` via admin path |
| 3 | `CreateVideo_DuplicateSlug_Returns409` | Admin token, existing video with slug "test-video" | `POST /admin/videos` with `slug: "test-video"` | 409 | `VideoErrors.SlugAlreadyExists(slug)` |
| 4 | `CreateVideo_EmptyTitle_Returns400` | Admin token | `POST /admin/videos` with `title: ""` (bypassing validator) | 400 | `VideoErrors.TitleRequired()` |
| 5 | `CreateVideo_EmptySlug_Returns400` | Admin token | `POST /admin/videos` with `slug: ""` (bypassing validator) | 400 | `VideoErrors.SlugRequired()` |
| 6 | `PublishVideo_NoYoutubeUrl_Returns400` | Admin token, approved video without YouTube URL | `PATCH /admin/videos/{id}/publish` | 400 | `VideoErrors.CannotPublishWithoutYoutubeUrl()` |
| 7 | `DeleteVideo_Published_Returns400` | Admin token, published video | `DELETE /admin/videos/{id}` | 400 | `VideoErrors.CannotDeletePublishedVideo()` |
| 8 | `SubmitVideo_AlreadySubmitted_Returns409` | Admin token, video in Submitted status | `PATCH /admin/videos/{id}/submit` | 409 | `VideoErrors.AlreadySubmitted()` |
| 9 | `SubmitVideo_AlreadyPendingReview_Returns409` | Admin token, video in PendingReview status | `PATCH /admin/videos/{id}/submit` | 409 | `VideoErrors.AlreadyPendingReview()` |
| 10 | `ApproveVideo_AlreadyApproved_Returns409` | Admin token, video in Approved status | `PATCH /admin/videos/{id}/approve` | 409 | `VideoErrors.AlreadyApproved()` |
| 11 | `PublishVideo_AlreadyPublished_Returns409` | Admin token, published video with YouTube URL | `PATCH /admin/videos/{id}/publish` | 409 | `VideoErrors.AlreadyPublished()` |
| 12 | `RejectVideo_AlreadyRejected_Returns409` | Admin token, video in Rejected status | `PATCH /admin/videos/{id}/reject` with reason | 409 | `VideoErrors.AlreadyRejected()` |
| 13 | `ArchiveVideo_AlreadyArchived_Returns409` | Admin token, video in Archived status | `PATCH /admin/videos/{id}/archive` | 409 | `VideoErrors.AlreadyArchived()` |
| 14 | `PublishVideo_FromDraft_Returns400` | Admin token, video in Draft status | `PATCH /admin/videos/{id}/publish` | 400 | `VideoErrors.InvalidStatusTransition(from, to)` |
| 15 | `AttachYoutubeUrl_BeforeShootDate_Returns400` | Admin token, video with `shootingScheduledAt` in future | `PATCH /admin/videos/{id}/youtube` with valid YouTube URL | 400 | `VideoErrors.CannotAttachYoutubeUrlBeforeShoot()` |

## 4. Error Messages - Target 100% (Transitively Covered)

Each error method in Section 3 calls its corresponding error message method internally. Covering every error method in Section 3 transitively covers every error message method. No separate tests are needed.

| Error Message Class | Covered By |
|---------------------|------------|
| `ArticleErrorMessage` | ArticleErrors tests (Section 3, 13 tests) |
| `ArticleInteractionErrorMessage` | ArticleInteractionErrors tests (Section 3, 7 tests) |
| `CategoryErrorMessage` | CategoryErrors tests (Section 3, 12 tests) |
| `ContentOrderErrorMessage` | ContentOrderErrors tests (Section 3, 13 tests) |
| `ContentTypeErrorMessage` | ContentTypeErrors tests (Section 3, 4 tests) |
| `CustomerErrorMessage` | CustomerErrors tests (Section 3, 4 tests) |
| `LyricsErrorMessage` | LyricsErrors tests (Section 3, 5 tests) |
| `PackageErrorMessage` | PackageErrors tests (Section 3, 7 tests) |
| `PlaylistErrorMessage` | PlaylistErrors tests (Section 3, 4 tests) |
| `PricingTierErrorMessage` | PricingTierErrors tests (Section 3, 6 tests) |
| `PromotionLevelErrorMessage` | PromotionLevelErrors tests (Section 3, 8 tests) |
| `ShortVideoErrorMessage` | ShortVideoErrors tests (Section 3, 3 tests) + Cloudinary validator tests (Section 1) |
| `ShortVideoInteractionErrorMessage` | ShortVideoInteractionErrors tests (Section 3, 4 tests) |
| `TagErrorMessage` | TagErrors tests (Section 3, 3 tests) |
| `VideoErrorMessage` | VideoErrors tests (Section 3, 15 tests) |

## 5. Shared Validators - Target 100%

### CategoryValidation (65.7%)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `CreateCategory_EmptyName_Returns400` | Admin token | `POST /admin/categories` with `name: ""` | 400 | `ValidCategoryName(isRequired=true)` required branch |
| 2 | `CreateCategory_NameExceedsMaxLength_Returns400` | Admin token | `POST /admin/categories` with name of 101+ chars | 400 | `ValidCategoryName(isRequired=true)` max length branch |
| 3 | `UpdateCategory_NameExceedsMaxLength_Returns400` | Admin token, seeded category | `PUT /admin/categories/{id}` with name of 101+ chars | 400 | `ValidCategoryName(isRequired=false)` max length branch |
| 4 | `CreateCategory_EmptySlug_Returns400` | Admin token | `POST /admin/categories` with `slug: ""` | 400 | `ValidCategorySlug(isRequired=true)` required branch |
| 5 | `CreateCategory_InvalidSlugFormat_Returns400` | Admin token | `POST /admin/categories` with `slug: "INVALID SLUG!"` | 400 | `ValidCategorySlug(isRequired=true)` format branch |
| 6 | `UpdateCategory_SlugExceedsMaxLength_Returns400` | Admin token, seeded category | `PUT /admin/categories/{id}` with slug of 101+ chars | 400 | `ValidCategorySlug(isRequired=false)` max length branch |
| 7 | `CreateCategory_DescriptionExceedsMaxLength_Returns400` | Admin token | `POST /admin/categories` with description exceeding max | 400 | `ValidCategoryDescription` max length branch |
| 8 | `AddCategoryPricing_NegativePriceUsd_Returns400` | Admin token, category, tier | `POST /admin/categories/{id}/pricing` with `priceUsd: -1` | 400 | `ValidCategoryPriceUsd` negative check |

### ContentTypeValidation (58.3%)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `CreateContentType_EmptyName_Returns400` | Admin token | `POST /admin/content-types` with `name: ""` | 400 | `ValidContentTypeName(isRequired=true)` required branch |
| 2 | `CreateContentType_NameExceedsMaxLength_Returns400` | Admin token | `POST /admin/content-types` with name of 101+ chars | 400 | `ValidContentTypeName(isRequired=true)` max length branch |
| 3 | `UpdateContentType_NameExceedsMaxLength_Returns400` | Admin token, seeded content type | `PUT /admin/content-types/{id}` with name of 101+ chars | 400 | `ValidContentTypeName(isRequired=false)` max length branch |

### EditorialValidation (62.7%)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `CreateArticle_EmptyCategoryId_Returns400` | Admin token | `POST /admin/articles` with `categoryId: "00000000-0000-0000-0000-000000000000"` | 400 | `ValidArticleCategoryId` empty GUID check |
| 2 | `CreateArticle_EmptyTitle_Returns400` | Admin token | `POST /admin/articles` with `title: ""` | 400 | `ValidArticleTitle(isRequired=true)` required branch |
| 3 | `CreateArticle_TitleExceedsMaxLength_Returns400` | Admin token | `POST /admin/articles` with title of 201+ chars | 400 | `ValidArticleTitle(isRequired=true)` max length branch |
| 4 | `UpdateArticle_TitleExceedsMaxLength_Returns400` | Admin token, seeded article | `PUT /admin/articles/{id}` with title of 201+ chars | 400 | `ValidArticleTitle(isRequired=false)` max length branch |
| 5 | `CreateArticle_EmptySlug_Returns400` | Admin token | `POST /admin/articles` with `slug: ""` | 400 | `ValidArticleSlug(isRequired=true)` required branch |
| 6 | `CreateArticle_InvalidSlugFormat_Returns400` | Admin token | `POST /admin/articles` with `slug: "INVALID SLUG!"` | 400 | `ValidArticleSlug(isRequired=true)` format branch |
| 7 | `UpdateArticle_SlugExceedsMaxLength_Returns400` | Admin token, seeded article | `PUT /admin/articles/{id}` with slug of 201+ chars | 400 | `ValidArticleSlug(isRequired=false)` max length branch |
| 8 | `CreateArticle_HeadlineTooShort_Returns400` | Admin token | `POST /admin/articles` with `headline: "ab"` | 400 | `ValidArticleHeadline` min length branch |
| 9 | `CreateArticle_HeadlineExceedsMaxLength_Returns400` | Admin token | `POST /admin/articles` with headline exceeding max | 400 | `ValidArticleHeadline` max length branch |
| 10 | `CreateArticle_EmptyBody_Returns400` | Admin token | `POST /admin/articles` with `body: ""` | 400 | `ValidArticleBody` required branch |
| 11 | `CreateVideo_EmptyTitle_Returns400` | Admin token | `POST /admin/videos` with `title: ""` | 400 | `ValidVideoTitle(isRequired=true)` required branch |
| 12 | `CreateVideo_TitleExceedsMaxLength_Returns400` | Admin token | `POST /admin/videos` with title of 201+ chars | 400 | `ValidVideoTitle(isRequired=true)` max length branch |
| 13 | `UpdateVideo_TitleExceedsMaxLength_Returns400` | Admin token, seeded video | `PUT /admin/videos/{id}` with title of 201+ chars | 400 | `ValidVideoTitle(isRequired=false)` max length branch |
| 14 | `CreateVideo_EmptySlug_Returns400` | Admin token | `POST /admin/videos` with `slug: ""` | 400 | `ValidVideoSlug(isRequired=true)` required branch |
| 15 | `CreateVideo_InvalidSlugFormat_Returns400` | Admin token | `POST /admin/videos` with `slug: "INVALID!"` | 400 | `ValidVideoSlug(isRequired=true)` format branch |
| 16 | `UpdateVideo_SlugExceedsMaxLength_Returns400` | Admin token, seeded video | `PUT /admin/videos/{id}` with slug of 201+ chars | 400 | `ValidVideoSlug(isRequired=false)` max length branch |
| 17 | `CreateVideo_EmptyDescription_Returns400` | Admin token | `POST /admin/videos` with `description: ""` | 400 | `ValidVideoDescription` required branch |
| 18 | `AttachYoutubeUrl_EmptyUrl_Returns400` | Admin token, seeded video | `PATCH /admin/videos/{id}/youtube` with `url: ""` | 400 | `ValidYoutubeVideoUrl` required branch |
| 19 | `AttachYoutubeUrl_NonYoutubeUrl_Returns400` | Admin token, seeded video | `PATCH /admin/videos/{id}/youtube` with `url: "https://vimeo.com/123"` | 400 | `ValidYoutubeVideoUrl` format branch |
| 20 | `AttachYoutubeUrl_UrlExceedsMaxLength_Returns400` | Admin token, seeded video | `PATCH /admin/videos/{id}/youtube` with URL exceeding max | 400 | `ValidYoutubeVideoUrl` max length branch |
| 21 | `CreateShortVideo_EmptyTitle_Returns400` | Admin token | `POST /admin/short-videos` with `title: ""` | 400 | `ValidShortVideoTitle` required branch |
| 22 | `CreateShortVideo_TitleExceedsMaxLength_Returns400` | Admin token | `POST /admin/short-videos` with title of 201+ chars | 400 | `ValidShortVideoTitle` max length branch |
| 23 | `CreateShortVideo_EmptySlug_Returns400` | Admin token | `POST /admin/short-videos` with `slug: ""` | 400 | `ValidShortVideoSlug` required branch |
| 24 | `CreateShortVideo_InvalidSlugFormat_Returns400` | Admin token | `POST /admin/short-videos` with `slug: "INVALID!"` | 400 | `ValidShortVideoSlug` format branch |
| 25 | `CreateShortVideo_NullVideoFile_Returns400` | Admin token | `POST /admin/short-videos` with no file | 400 | `ValidShortVideoFile` null check |
| 26 | `CreateShortVideo_EmptyVideoFile_Returns400` | Admin token | `POST /admin/short-videos` with 0-byte file | 400 | `ValidShortVideoFile` empty check |
| 27 | `CreateShortVideo_OversizedVideoFile_Returns400` | Admin token | `POST /admin/short-videos` with file exceeding max size | 400 | `ValidShortVideoFile` size check |
| 28 | `CreateShortVideo_WrongFileExtension_Returns400` | Admin token | `POST /admin/short-videos` with `.txt` file | 400 | `ValidShortVideoFile` extension check |
| 29 | `CreateVideo_OrderItemIdWithoutCustomerId_Returns400` | Admin token | `POST /admin/videos` with `orderItemId: "{guid}"` but `customerId: null` | 400 | `ValidCustomerId` conditional required branch |
| 30 | `CreateVideo_CustomerIdWithoutOrderItemId_Returns400` | Admin token | `POST /admin/videos` with `customerId: "{guid}"` but `orderItemId: null` | 400 | `ValidOrderItemId` conditional required branch |
| 31 | `ForceUnpromoteArticle_EmptyReason_Returns400` | Admin token, promoted article | `PATCH /admin/articles/{id}/force-unpromote` with `reason: ""` | 400 | `ValidUnpromoteReason` required branch |
| 32 | `ForceUnpromoteArticle_ReasonExceedsMaxLength_Returns400` | Admin token, promoted article | `PATCH /admin/articles/{id}/force-unpromote` with reason > 500 chars | 400 | `ValidUnpromoteReason` max length branch |
| 33 | `CreateArticle_MetaTitleTooShort_Returns400` | Admin token | `POST /admin/articles` with `metaTitle: "ab"` | 400 | `ValidMetaTitle` min length branch |
| 34 | `CreateArticle_MetaTitleExceedsMaxLength_Returns400` | Admin token | `POST /admin/articles` with metaTitle exceeding max | 400 | `ValidMetaTitle` max length branch |
| 35 | `CreateArticle_MetaDescriptionTooShort_Returns400` | Admin token | `POST /admin/articles` with `metaDescription: "ab"` | 400 | `ValidMetaDescription` min length branch |
| 36 | `CreateArticle_MetaDescriptionExceedsMaxLength_Returns400` | Admin token | `POST /admin/articles` with metaDescription exceeding max | 400 | `ValidMetaDescription` max length branch |
| 37 | `RejectArticle_EmptyReason_Returns400` | Admin token, pending-review article | `PATCH /admin/articles/{id}/reject` with `reason: ""` | 400 | `ValidRejectionReason` required branch |
| 38 | `RejectArticle_ReasonExceedsMaxLength_Returns400` | Admin token, pending-review article | `PATCH /admin/articles/{id}/reject` with reason exceeding max | 400 | `ValidRejectionReason` max length branch |
| 39 | `CreateLyrics_EmptySongTitle_Returns400` | Admin token | `POST /admin/lyrics` with `songTitle: ""` | 400 | `ValidSongTitle` required branch |
| 40 | `CreateLyrics_SongTitleExceedsMaxLength_Returns400` | Admin token | `POST /admin/lyrics` with songTitle exceeding max | 400 | `ValidSongTitle` max length branch |
| 41 | `CreateLyrics_EmptyArtistName_Returns400` | Admin token | `POST /admin/lyrics` with `artistName: ""` | 400 | `ValidArtistName` required branch |
| 42 | `CreateLyrics_ArtistNameExceedsMaxLength_Returns400` | Admin token | `POST /admin/lyrics` with artistName exceeding max | 400 | `ValidArtistName` max length branch |
| 43 | `CreateLyrics_EmptyLyricsText_Returns400` | Admin token | `POST /admin/lyrics` with `lyricsText: ""` | 400 | `ValidLyricsText` required branch |
| 44 | `CreateLyrics_EmptyLanguage_Returns400` | Admin token | `POST /admin/lyrics` with `language: ""` | 400 | `ValidLyricsLanguage` required branch |
| 45 | `CreateLyrics_LanguageExceedsMaxLength_Returns400` | Admin token | `POST /admin/lyrics` with language exceeding max | 400 | `ValidLyricsLanguage` max length branch |
| 46 | `CreateVideo_ShootingScheduledAtInPast_Returns400` | Admin token | `POST /admin/videos` with `shootingScheduledAt` set to yesterday | 400 | `ValidShootingScheduledAt` past date check |
| 47 | `UploadArticleImage_NullFile_Returns400` | Admin token | `POST /admin/articles/{id}/image` with no file | 400 | `ValidArticleImageFile` null check |

**Note:** `ValidArticleId`, `ValidVideoId`, and `ValidLyricsId` in `EditorialValidation` have zero callers. Classified as dead code (Section 9).

### PricingTierValidation (75%)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `CreatePricingTier_EmptyId_Returns400` | Admin token | Request with `id: "00000000-0000-0000-0000-000000000000"` | 400 | `ValidPricingTierId` empty GUID check |
| 2 | `CreatePricingTier_EmptyName_Returns400` | Admin token | `POST /admin/pricing-tiers` with `name: ""` | 400 | `ValidPricingTierName(isRequired=true)` required branch |
| 3 | `CreatePricingTier_NameExceedsMaxLength_Returns400` | Admin token | `POST /admin/pricing-tiers` with name of 101+ chars | 400 | `ValidPricingTierName(isRequired=true)` max length branch |
| 4 | `UpdatePricingTier_NameExceedsMaxLength_Returns400` | Admin token, seeded tier | `PUT /admin/pricing-tiers/{id}` with name of 101+ chars | 400 | `ValidPricingTierName(isRequired=false)` max length branch |
| 5 | `CreatePricingTier_EmptyDescription_Returns400` | Admin token | `POST /admin/pricing-tiers` with `description: ""` | 400 | `ValidPricingTierDescription` required branch |
| 6 | `CreatePricingTier_DescriptionExceedsMaxLength_Returns400` | Admin token | `POST /admin/pricing-tiers` with description exceeding max | 400 | `ValidPricingTierDescription` max length branch |

### PromotionLevelValidation (66.6%)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `CreatePromotionLevel_EmptyName_Returns400` | Admin token | `POST /admin/promotion-levels` with `name: ""` | 400 | `ValidPromotionLevelName(isRequired=true)` required branch |
| 2 | `CreatePromotionLevel_NameExceedsMaxLength_Returns400` | Admin token | `POST /admin/promotion-levels` with name of 101+ chars | 400 | `ValidPromotionLevelName(isRequired=true)` max length branch |
| 3 | `UpdatePromotionLevel_NameExceedsMaxLength_Returns400` | Admin token, seeded promotion level | `PUT /admin/promotion-levels/{id}` with name of 101+ chars | 400 | `ValidPromotionLevelName(isRequired=false)` max length branch |
| 4 | `CreatePromotionLevel_ZeroDurationDays_Returns400` | Admin token | `POST /admin/promotion-levels` with `durationDays: 0` | 400 | `ValidDurationDays` zero check |
| 5 | `CreatePromotionLevel_NegativeDurationDays_Returns400` | Admin token | `POST /admin/promotion-levels` with `durationDays: -1` | 400 | `ValidDurationDays` negative check |
| 6 | `CreatePromotionLevel_NegativePriceUsd_Returns400` | Admin token | `POST /admin/promotion-levels` with `priceUsd: -1` | 400 | `ValidPriceUsd` negative check |
| 7 | `CreatePromotionLevel_SpotPriorityZero_Returns400` | Admin token | `POST /admin/promotion-levels` with `spotPriority: 0` | 400 | `ValidSpotPriority` below-range check |
| 8 | `CreatePromotionLevel_SpotPriorityFour_Returns400` | Admin token | `POST /admin/promotion-levels` with `spotPriority: 4` | 400 | `ValidSpotPriority` above-range check |

### TagValidation (63.6%)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `CreateTag_EmptyName_Returns400` | Admin token | `POST /admin/tags` with `name: ""` | 400 | `ValidTagName(isRequired=true)` required branch |
| 2 | `CreateTag_NameExceedsMaxLength_Returns400` | Admin token | `POST /admin/tags` with name of 101+ chars | 400 | `ValidTagName(isRequired=true)` max length branch |
| 3 | `UpdateTag_NameExceedsMaxLength_Returns400` | Admin token, seeded tag | `PUT /admin/tags/{id}` with name of 101+ chars | 400 | `ValidTagName(isRequired=false)` max length branch |
| 4 | `CreateTag_EmptySlug_Returns400` | Admin token | `POST /admin/tags` with `slug: ""` | 400 | `ValidTagSlug(isRequired=true)` required branch |
| 5 | `CreateTag_InvalidSlugFormat_Returns400` | Admin token | `POST /admin/tags` with `slug: "INVALID SLUG!"` | 400 | `ValidTagSlug(isRequired=true)` format branch |
| 6 | `UpdateTag_SlugExceedsMaxLength_Returns400` | Admin token, seeded tag | `PUT /admin/tags/{id}` with slug of 101+ chars | 400 | `ValidTagSlug(isRequired=false)` max length branch |
| 7 | `UpdateArticleTags_EmptyTagNameItem_Returns400` | Admin token, seeded article | `PUT /admin/articles/{id}/tags` with `tagNames: [""]` | 400 | `ValidTagNameItem` empty item check |
| 8 | `UpdateVideoTags_EmptyTagNameItem_Returns400` | Admin token, seeded video | `PUT /admin/videos/{id}/tags` with `tagNames: [""]` | 400 | `ValidTagNameItem` empty item check (video path) |

## 6. Entity Domain Methods

Entities are covered indirectly when handler tests call their domain methods. The guard clause branches (e.g., `Activate()` returns false when already active) are covered by the error-path tests in Section 3.

### Entities at 0% (Cover via endpoint tests)

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `BookmarkShortVideo_CreatesBookmarkEntity` | User token, active short video | `POST /public/short-videos/{id}/bookmark` | 200/201 | `ShortVideoBookmarkEntity` Create method |
| 2 | `ShareShortVideo_CreatesShareEntity` | User token, active short video | `POST /public/short-videos/{id}/share` | 200/201 | `ShortVideoShareEntity` Create method |
| 3 | `ShareVideo_CreatesShareEntity` | User token, published video | `POST /public/videos/{id}/share` | 200/201 | `VideoShareEntity` Create method |
| 4 | `UpdateArticleTags_WithExistingTags_CoversArticleTagEntity` | Admin token, article with existing tags | `PUT /admin/articles/{id}/tags` with new tags | 200 | `ArticleTagEntity` Create method (covered jointly with `ArticleTagByArticleIdSpecification` test in Section 2) |

### Partially Covered Entities

These entity methods are covered transitively when the error-path and specification tests from Sections 2 and 3 are added. No separate tests are needed beyond what is already specified. The following summarizes the coverage mapping:

| Entity | Uncovered Methods | Covered By Tests In |
|--------|-------------------|---------------------|
| `ShortVideoEntity` | Activate, Deactivate guard clauses | Section 3 ShortVideoErrors tests |
| `VideoEntity` | State transition guards, Publish without URL | Section 3 VideoErrors tests |
| `ArticleEntity` | State transition guards, SoftDelete | Section 3 ArticleErrors tests |
| `ArticleCommentEntity` | Edit, SoftDelete by non-owner | Section 3 ArticleInteractionErrors tests |
| `PromotionLevelEntity` | Activate/Deactivate guards, Create validation | Section 3 PromotionLevelErrors tests |
| `CategoryEntity` | SetExclusive, Activate/Deactivate guards | Section 3 CategoryErrors tests |
| `PricingTierEntity` | Activate/Deactivate guards | Section 3 PricingTierErrors tests |
| `ContentTypeEntity` | Activate/Deactivate guards | Section 3 ContentTypeErrors tests |
| `PackageEntity` | Activate/Deactivate guards | Section 3 PackageErrors tests |
| `ContentOrderEntity` | Submit/Cancel/MarkPaid guards | Section 3 ContentOrderErrors tests |
| `ContentPaymentEntity` | Verify/Reject guards | Section 3 ContentOrderErrors tests (12, 13) |
| `PlaylistEntity` | Rename ownership check | Section 3 PlaylistErrors tests |
| `LyricsEntity` | ValidateRequiredFields | Section 3 LyricsErrors tests |
| `TagEntity` | Create validation | Section 3 TagErrors tests |
| `CustomerEntity` | Create validation | Section 3 CustomerErrors tests |
| `PackageSlotEntity` | Create with zero quantity | Section 3 PackageErrors test 6 |
| `CategoryPricingEntity` | Create with negative price | Section 3 CategoryErrors test 9 |

## 7. Repositories

Repository query methods are covered when handler tests exercise specific filter, search, and pagination paths.

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `ListArticles_WithSearchQuery_FiltersResults` | Admin token, 3 articles with distinct titles | `GET /admin/articles?search=specific-keyword` | 200 with filtered results | `ArticleRepository` search query builder |
| 2 | `ListArticles_WithCategoryFilter_FiltersResults` | Admin token, articles in different categories | `GET /admin/articles?categoryId={id}` | 200 with filtered results | `ArticleRepository` category filter path |
| 3 | `ListArticles_WithStatusFilter_FiltersResults` | Admin token, articles in different statuses | `GET /admin/articles?status=Published` | 200 with filtered results | `ArticleRepository` status filter path |
| 4 | `ListVideos_WithSearchQuery_FiltersResults` | Admin token, 3 videos with distinct titles | `GET /admin/videos?search=specific-keyword` | 200 with filtered results | `VideoRepository` search query builder |
| 5 | `ListVideos_WithCategoryFilter_FiltersResults` | Admin token, videos in different categories | `GET /admin/videos?categoryId={id}` | 200 with filtered results | `VideoRepository` category filter path |
| 6 | `ListVideos_WithStatusFilter_FiltersResults` | Admin token, videos in different statuses | `GET /admin/videos?status=Published` | 200 with filtered results | `VideoRepository` status filter path |
| 7 | `ListShortVideos_WithSearchQuery_FiltersResults` | Admin token, 3 short videos with distinct titles | `GET /admin/short-videos?search=specific-keyword` | 200 with filtered results | `ShortVideoRepository` search query builder |
| 8 | `ListShortVideos_WithStatusFilter_FiltersResults` | Admin token, short videos in active/inactive states | `GET /admin/short-videos?isActive=true` | 200 with filtered results | `ShortVideoRepository` status filter path |
| 9 | `ListLyrics_WithSearchQuery_FiltersResults` | Admin token, 3 lyrics with distinct song titles | `GET /admin/lyrics?search=specific-keyword` | 200 with filtered results | `LyricsRepository` search query builder |
| 10 | `ListMyPlaylists_ReturnsOnlyOwnPlaylists` | User A token, User A's playlists + User B's playlists | `GET /public/me/playlists` as User A | 200 with only User A's playlists | `PlaylistRepository` user filter path |
| 11 | `ListArticles_Paginated_ReturnsCorrectPage` | Admin token, 15 articles | `GET /admin/articles?page=2&pageSize=5` | 200 with 5 results, correct total | `ArticleRepository` pagination path |
| 12 | `ListVideos_Paginated_ReturnsCorrectPage` | Admin token, 15 videos | `GET /admin/videos?page=2&pageSize=5` | 200 with 5 results, correct total | `VideoRepository` pagination path |

## 8. Mappers

| # | Test Name | Seed Data | Request | Expected | Covers |
|---|-----------|-----------|---------|----------|--------|
| 1 | `GetShortVideo_WithAllFields_MapsCorrectly` | Admin token, short video with all optional fields populated | `GET /admin/short-videos/{id}` | 200 with all fields mapped | `ShortVideoMapper` full mapping path |
| 2 | `GetShortVideo_WithNullOptionalFields_MapsCorrectly` | Admin token, short video with null optional fields | `GET /admin/short-videos/{id}` | 200 with null fields handled | `ShortVideoMapper` null-handling branches |
| 3 | `GetPackage_WithSlots_MapsCorrectly` | Admin token, package with slots | `GET /admin/packages/{id}` | 200 with slots mapped | `PackageMapper` slot mapping path |

## 9. Dead Code

The following components have zero callers in the codebase. They cannot be covered by integration tests. Recommend removal or exclusion from the coverage target.

| Component | Type | Reason |
|-----------|------|--------|
| `TagByNameSpecification` | Specification | Zero callers across all handlers and repositories |
| `ContentTypeErrors.NotFound(id)` | Error method | No handler throws this exception |
| `TagErrors.NotFound(id)` | Error method | No handler throws this exception |
| `EditorialValidation.ValidArticleId` | Validator rule | Zero callers across all command validators |
| `EditorialValidation.ValidVideoId` | Validator rule | Zero callers across all command validators |
| `EditorialValidation.ValidLyricsId` | Validator rule | Zero callers across all command validators |

## 10. Summary

### Test Count by Section

| Section | Category | Test Count |
|---------|----------|------------|
| 1 | Cloudinary-blocked validators (coverable) | 16 |
| 2 | Specifications | 5 |
| 3 | Error classes | 99 |
| 4 | Error messages (transitive, no new tests) | 0 |
| 5 | Shared validators | 72 |
| 6 | Entity domain methods | 4 |
| 7 | Repositories | 12 |
| 8 | Mappers | 3 |
| **Total new tests** | | **211** |

### Coverage Projection

| Category | Lines Recovered |
|----------|----------------|
| Error-path negative tests (Section 3) | ~150 |
| Shared validator edge-case payloads (Section 5) | ~50 |
| Specification seed data (Section 2) | ~10 |
| Repository query-path variations (Section 7) | ~50 |
| Cloudinary validator tests (Section 1) | ~20 |
| Entity domain methods (Section 6) | ~10 |
| Mapper branches (Section 8) | ~10 |
| Error messages (transitive from Section 3) | ~100 |
| **Subtotal recoverable** | **~400** |

### Structurally Blocked (Cannot Cover)

| Category | Lines |
|----------|-------|
| Cloudinary-blocked handlers (3 handlers) | ~21 |
| `YoutubeThumbnailService` | ~10 |
| `AbandonedDraftCleanupJob` | ~10 |
| **Subtotal blocked** | **~41** |

### Dead Code (Exclude from Target)

| Category | Lines |
|----------|-------|
| `TagByNameSpecification` | ~2 |
| `ContentTypeErrors.NotFound` | ~2 |
| `TagErrors.NotFound` | ~2 |
| `EditorialValidation.ValidArticleId/ValidVideoId/ValidLyricsId` | ~6 |
| **Subtotal dead code** | **~12** |

### Projected Coverage After All Tests

- **Current:** 95.6% (11,515 / 12,039 lines)
- **After all tests:** ~98.9% (11,915 / 12,039 lines)
- **Theoretical max (excluding blocked + dead):** 99.6% (11,986 / 12,039 lines)
