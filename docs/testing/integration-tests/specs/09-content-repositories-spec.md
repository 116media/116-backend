# Phase 7: Content Module — Repository Tests Spec

## Tasks

### ArticleRepository
- [ ] `ArticleRepositoryTests.cs`
  - [ ] CreateAsync_ValidArticle_ShouldPersist
  - [ ] GetByIdOrThrowAsync_Existing_ShouldReturn
  - [ ] GetByIdOrThrowAsync_NonExistent_ShouldThrow
  - [ ] GetAllAsync_ShouldReturnPaginated
  - [ ] GetAllAsync_WithStatusFilter_ShouldFilterCorrectly
  - [ ] GetAllAsync_WithCategoryFilter_ShouldFilterCorrectly
  - [ ] GetAllAsync_WithSearchTerm_ShouldMatchTitle
  - [ ] GetBySlugAsync_ShouldReturnCorrectArticle
  - [ ] UpdateAsync_ShouldUpdateFields
  - [ ] SoftDeleteAsync_ShouldSetDeletedAt
  - [ ] GetWithTagsAsync_ShouldIncludeTags

### CategoryRepository
- [ ] `CategoryRepositoryTests.cs`
  - [ ] CreateAsync_ValidCategory_ShouldPersist
  - [ ] GetByIdOrThrowAsync_Existing_ShouldReturnWithContentType
  - [ ] GetByIdOrThrowAsync_NonExistent_ShouldThrow
  - [ ] GetAllAsync_ShouldReturnPaginated
  - [ ] GetActiveCategoriesAsync_ShouldReturnOnlyActive
  - [ ] GetExclusiveCategoryAsync_ShouldReturnExclusiveCategory
  - [ ] GetExclusiveCategoryAsync_NoExclusive_ShouldReturnNull
  - [ ] ExistsBySlugAsync_ShouldReturnCorrectResult
  - [ ] UpdateAsync_ShouldUpdateFields
  - [ ] GetWithPricingsAsync_ShouldIncludePricings

### ContentOrderRepository
- [ ] `ContentOrderRepositoryTests.cs`
  - [ ] CreateAsync_ValidOrder_ShouldPersist
  - [ ] GetByIdOrThrowAsync_Existing_ShouldReturn
  - [ ] GetAllAsync_ShouldReturnPaginated
  - [ ] GetCustomerOrdersAsync_ShouldFilterByCustomer
  - [ ] GetPendingPaymentOrdersAsync_ShouldReturnCorrectOrders
  - [ ] GetWithItemsAsync_ShouldIncludeOrderItems
  - [ ] GetOrderPaymentAsync_ShouldReturnPayment

### CustomerRepository
- [ ] `CustomerRepositoryTests.cs`
  - [ ] CreateAsync_ValidCustomer_ShouldPersist
  - [ ] GetByIdOrThrowAsync_Existing_ShouldReturn
  - [ ] GetByIdOrThrowAsync_NonExistent_ShouldThrow
  - [ ] GetAllAsync_ShouldReturnPaginated
  - [ ] GetByUserIdAsync_ShouldReturnCustomer
  - [ ] UpdateAsync_ShouldUpdateFields

### LookupRepository
- [ ] `LookupRepositoryTests.cs`
  - [ ] GetContentTypeByIdOrThrowAsync_Existing_ShouldReturn
  - [ ] GetContentTypeByIdOrThrowAsync_NonExistent_ShouldThrow
  - [ ] GetAllContentTypesAsync_ShouldReturnAll
  - [ ] GetPricingTierByIdOrThrowAsync_ShouldReturn
  - [ ] GetPromotionLevelByIdOrThrowAsync_ShouldReturn
  - [ ] GetTagByIdOrThrowAsync_ShouldReturn
  - [ ] GetAllTagsAsync_ShouldReturnPaginated
  - [ ] GetPopularTagsAsync_ShouldReturnByUsageCount

### LyricsRepository
- [ ] `LyricsRepositoryTests.cs`
  - [ ] CreateAsync_ValidLyrics_ShouldPersist
  - [ ] GetByVideoIdAsync_ShouldReturnLyrics
  - [ ] GetByVideoIdAsync_NoLyrics_ShouldReturnNull
  - [ ] UpdateAsync_ShouldUpdateFields
  - [ ] DeleteAsync_ShouldRemoveFromDatabase

### PackageRepository
- [ ] `PackageRepositoryTests.cs`
  - [ ] CreateAsync_ValidPackage_ShouldPersist
  - [ ] GetByIdOrThrowAsync_Existing_ShouldReturn
  - [ ] GetAllAsync_ShouldReturnPaginated
  - [ ] GetWithSlotsAsync_ShouldIncludeSlots
  - [ ] ActivateAsync_ShouldSetIsActiveTrue
  - [ ] DeactivateAsync_ShouldSetIsActiveFalse

### PlaylistRepository
- [ ] `PlaylistRepositoryTests.cs`
  - [ ] CreateAsync_ValidPlaylist_ShouldPersist
  - [ ] GetByIdOrThrowAsync_Existing_ShouldReturn
  - [ ] GetByUserIdAsync_ShouldReturnUserPlaylists
  - [ ] AddVideoAsync_ShouldCreateJunctionRecord
  - [ ] RemoveVideoAsync_ShouldDeleteJunctionRecord
  - [ ] RenameAsync_ShouldUpdateName
  - [ ] DeleteAsync_ShouldRemovePlaylistAndVideos

### ShortVideoRepository
- [ ] `ShortVideoRepositoryTests.cs`
  - [ ] CreateAsync_ValidShortVideo_ShouldPersist
  - [ ] GetByIdOrThrowAsync_Existing_ShouldReturn
  - [ ] GetAllAsync_ShouldReturnPaginated
  - [ ] GetAllAsync_WithStatusFilter_ShouldFilter
  - [ ] UpdateAsync_ShouldUpdateFields
  - [ ] ActivateAsync_ShouldSetIsActiveTrue
  - [ ] DeactivateAsync_ShouldSetIsActiveFalse
  - [ ] IncrementViewCountAsync_ShouldIncrement

### VideoRepository
- [ ] `VideoRepositoryTests.cs`
  - [ ] CreateAsync_ValidVideo_ShouldPersist
  - [ ] GetByIdOrThrowAsync_Existing_ShouldReturn
  - [ ] GetByIdOrThrowAsync_NonExistent_ShouldThrow
  - [ ] GetAllAsync_ShouldReturnPaginated
  - [ ] GetAllAsync_WithStatusFilter_ShouldFilterCorrectly
  - [ ] GetAllAsync_WithCategoryFilter_ShouldFilterCorrectly
  - [ ] GetAllAsync_WithSearchTerm_ShouldMatchTitle
  - [ ] GetBySlugAsync_ShouldReturnCorrectVideo
  - [ ] UpdateAsync_ShouldUpdateFields
  - [ ] GetWithTagsAsync_ShouldIncludeTags
  - [ ] GetWithRatingsAsync_ShouldIncludeRatings

## FK Seeding Order

```
1. ContentTypes (no FK)
2. PricingTiers (no FK)
3. PromotionLevels (no FK)
4. Tags (no FK)
5. Categories (FK → ContentTypes)
6. CategoryPricings (FK → Categories, PricingTiers)
7. Videos (FK → Categories)
8. Articles (FK → Categories)
9. ShortVideos (FK → Categories)
10. Lyrics (FK → Videos)
11. Playlists (FK → Users)
12. PlaylistVideos (FK → Playlists, Videos)
13. Customers (FK → Users from Identity)
14. ContentOrders (FK → Customers)
15. ContentOrderItems (FK → Orders, Articles/Videos)
```

## File Locations

```
tests/_116.Integration.Tests/Content/Repositories/
├── ArticleRepositoryTests.cs
├── CategoryRepositoryTests.cs
├── ContentOrderRepositoryTests.cs
├── CustomerRepositoryTests.cs
├── LookupRepositoryTests.cs
├── LyricsRepositoryTests.cs
├── PackageRepositoryTests.cs
├── PlaylistRepositoryTests.cs
├── ShortVideoRepositoryTests.cs
└── VideoRepositoryTests.cs
```

## Acceptance Criteria

1. Every public repository method has at least one integration test
2. FK dependencies are seeded in correct order
3. Pagination tests verify both items and total count
4. `./scripts/run-tests-with-coverage.sh integration` passes
