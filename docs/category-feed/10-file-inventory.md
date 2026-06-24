# 10 — File Inventory

Complete list of files to create or modify, grouped by phase. All source paths are under
`src/Modules/Content/Content/` and test paths under `tests/`.

## New Source Files

| # | File | Purpose |
|---|------|---------|
| 1 | `Application/Catalog/Constants/CatalogFeedConstants.cs` | `MaxPinnedCategoriesPerContentType = 5` |
| 2 | `Application/Catalog/UseCases/Admin/Commands/PinCategoryToFeed/AdminPinCategoryToFeedCommand.cs` | Command + result records |
| 3 | `Application/Catalog/UseCases/Admin/Commands/PinCategoryToFeed/AdminPinCategoryToFeedHandler.cs` | Cap + FIFO eviction handler |
| 4 | `Application/Catalog/UseCases/Admin/Commands/PinCategoryToFeed/AdminPinCategoryToFeedMetaField.cs` | Route metadata |
| 5 | `Application/Catalog/UseCases/Admin/Commands/PinCategoryToFeed/V1/AdminPinCategoryToFeedEndpointV1.cs` | Endpoint |
| 6 | `Application/Catalog/UseCases/Admin/Commands/UnpinCategoryFromFeed/AdminUnpinCategoryFromFeedCommand.cs` | Command + result records |
| 7 | `Application/Catalog/UseCases/Admin/Commands/UnpinCategoryFromFeed/AdminUnpinCategoryFromFeedHandler.cs` | Unpin handler |
| 8 | `Application/Catalog/UseCases/Admin/Commands/UnpinCategoryFromFeed/AdminUnpinCategoryFromFeedMetaField.cs` | Route metadata |
| 9 | `Application/Catalog/UseCases/Admin/Commands/UnpinCategoryFromFeed/V1/AdminUnpinCategoryFromFeedEndpointV1.cs` | Endpoint |
| 10 | `Application/Editorial/UseCases/Public/Queries/GetVideoFeed/PublicGetVideoFeedQuery.cs` | Query + result/section DTOs |
| 11 | `Application/Editorial/UseCases/Public/Queries/GetVideoFeed/PublicGetVideoFeedHandler.cs` | Feed assembly handler |
| 12 | `Application/Editorial/UseCases/Public/Queries/GetVideoFeed/PublicGetVideoFeedMetaField.cs` | Route metadata |
| 13 | `Application/Editorial/UseCases/Public/Queries/GetVideoFeed/V1/PublicGetVideoFeedEndpointV1.cs` | Endpoint |

## Modified Source Files

| # | File | Change |
|---|------|--------|
| 1 | `Domain/Entities/CategoryEntity.cs` | Add `PinnedToFeedAt`, `[NotMapped] IsPinnedToFeed`, `PinToFeed()`, `UnpinFromFeed()` |
| 2 | `Infrastructure/Persistence/Configurations/CategoryConfiguration.cs` | `PinnedToFeedAt` property + non-unique partial index |
| 3 | `Application/Catalog/Specifications/CategorySpecifications.cs` | Add `PinnedToFeedCategorySpecification` |
| 4 | `Application/Shared/Repositories/ICategoryRepository.cs` | Add `GetPinnedToFeedCategoriesAsync` |
| 5 | `Infrastructure/Persistence/Repositories/CategoryRepository.cs` | Implement `GetPinnedToFeedCategoriesAsync` |
| 6 | `Application/Shared/Repositories/IVideoRepository.cs` | Add `GetLatestPublishedByCategoryAsync`, `CountPublishedByCategoryAsync` |
| 7 | `Infrastructure/Repositories/VideoRepository.cs` | Implement both new video repo methods |
| 8 | `Application/Shared/DTOs/CategoryDto.cs` | Add `IsPinnedToFeed`, `PinnedToFeedAt` params + docs |
| 9 | `Application/Catalog/Constants/CatalogRouteConstants.cs` | Add `PinToFeed`, `UnpinFromFeed` route segments |
| 10 | `Application/Editorial/Constants/EditorialRouteConstants.cs` | Add `Feed` route segment |
| 11 | `Application/Editorial/Constants/EditorialFeedConstants.cs` | Add `MaxVideosPerFeedSection = 8`, `MinVideosToPinToFeed = 4` |
| 12 | `Application/Shared/Errors/CategoryErrors.cs` | Add `CannotPinInactiveToFeed`, `ContentTypeNotFeedable`, `NotEnoughVideosToPinToFeed` |
| 13 | `Application/Shared/Errors/Messages/CategoryErrorMessage.cs` | Add the three i18n keys |
| 14 | `Application/Shared/Mappers/CategoryMapper.cs` | Add `ToCategoryDto(mapper, files)` no-IO batch overload |
| 15 | `Application/Shared/Mappers/VideoMapper.cs` | Add `ToVideoSummaryDto(mapper, files)` no-IO batch overload |
| 16 | `Modules/Core/.../IFileRepository.cs` | Add `GetByIdsAsync` (batch file fetch) |
| 17 | `Modules/Core/.../FileRepository.cs` | Implement `GetByIdsAsync` |

> The two new `CategoryDto` fields still map by name via the existing async mappers; the new
> mapper overloads are for **batch** mapping in the feed handler (see [06](06-dto-and-mapper.md), [05](05-public-video-feed-query.md)).

## New Test Files

| # | File |
|---|------|
| 1 | `tests/Unit/.../Catalog/UseCases/Admin/Commands/PinCategoryToFeed/AdminPinCategoryToFeedHandlerTests.cs` |
| 2 | `tests/Unit/.../Catalog/UseCases/Admin/Commands/UnpinCategoryFromFeed/AdminUnpinCategoryFromFeedHandlerTests.cs` |
| 3 | `tests/Unit/.../Editorial/UseCases/Public/Queries/GetVideoFeed/PublicGetVideoFeedHandlerTests.cs` |
| 4 | `tests/Integration/.../Catalog/UseCases/Admin/Commands/PinCategoryToFeed/V1/AdminPinCategoryToFeedEndpointV1Tests.cs` |
| 5 | `tests/Integration/.../Catalog/UseCases/Admin/Commands/UnpinCategoryFromFeed/V1/AdminUnpinCategoryFromFeedEndpointV1Tests.cs` |
| 6 | `tests/Integration/.../Editorial/UseCases/Public/Queries/GetVideoFeed/V1/PublicGetVideoFeedEndpointV1Tests.cs` |
| 7 | `tests/Unit/.../Catalog/Specifications/CategorySpecificationTests.cs` (new — no spec test exists yet) |

> See [09-tests.md](09-tests.md) for the full 100%-coverage plan (case lists + code) behind these files.

## Modified Test Files

| # | File | Change |
|---|------|--------|
| 1 | `tests/Unit/.../Domain/Entities/CategoryEntityTests.cs` | `PinToFeed`/`UnpinFromFeed`/`IsPinnedToFeed` tests |
| 2 | `tests/Unit/.../Catalog/Specifications/CategorySpecificationTests.cs` | `PinnedToFeedCategorySpecification` tests |
| 3 | `tests/Fixtures/Builders/Entities/Content/CategoryBuilder.cs` | `WithPinnedToFeedAt` |
| 4 | `tests/Fixtures/Factories/Content/CategoryFactory.cs` | `pinnedToFeedAt` param |
| 5 | `tests/Fixtures/Mocks/Content/MockCategoryRepository.cs` | `GetPinnedToFeedCategoriesAsync` setup |
| 6 | `tests/Fixtures/Mocks/Content/MockVideoRepository.cs` | `GetLatestPublishedByCategoryAsync`, `CountPublishedByCategoryAsync` setup |
| 7 | `tests/Fixtures/Mocks/Core/MockFileRepository.cs` | `GetByIdsAsync` setup |
| 8 | Repository integration tests | `GetPinnedToFeedCategoriesAsync`, `GetLatestPublishedByCategoryAsync`, `CountPublishedByCategoryAsync`, `GetByIdsAsync` |

## i18n Resource Files

| # | File | Change |
|---|------|--------|
| 1 | `src/.../Resources/en/CategoryErrorMessage.en.resx` | `Category.CannotPinInactiveToFeed`, `Category.ContentTypeNotFeedable`, `Category.NotEnoughVideosToPinToFeed` |
| 2 | `src/.../Resources/fr/CategoryErrorMessage.fr.resx` | Same three keys |

## EF Migration

| # | File | Generated by |
|---|------|-------------|
| 1 | `Infrastructure/Persistence/Migrations/*_AddPinnedToFeedToCategory.cs` | `dotnet ef migrations add` (see [08](08-ef-migration.md)) |

## Totals

| Type | Count |
| ---- | ----- |
| New source files | 13 |
| Modified source files | 17 |
| New test files | 6 |
| Modified test files | 8 |
| i18n resource files | 2 |
| EF migration | 1 |
| **Total** | **~47 files** |

## Suggested implementation order

1. Entity + constants ([01](01-domain-entity.md)) → EF config ([02](02-ef-configuration.md)) → migration ([08](08-ef-migration.md))
2. Specification + repositories ([03](03-repository-and-specification.md))
3. DTO ([06](06-dto-and-mapper.md)) + errors/i18n ([07](07-error-messages.md))
4. Admin pin/unpin use cases ([04](04-admin-pin-category.md))
5. Public video feed query ([05](05-public-video-feed-query.md))
6. Tests throughout ([09](09-tests.md))
