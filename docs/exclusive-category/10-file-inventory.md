# 10 — File Inventory

Complete list of files to create or modify, grouped by phase.

## New Files (7)

| # | File | Purpose |
|---|------|---------|
| 1 | `src/.../Catalog/UseCases/Admin/Commands/UploadCategoryPoster/AdminUploadCategoryPosterCommand.cs` | Command record |
| 2 | `src/.../Catalog/UseCases/Admin/Commands/UploadCategoryPoster/AdminUploadCategoryPosterHandler.cs` | Poster upload handler |
| 3 | `src/.../Catalog/UseCases/Admin/Commands/UploadCategoryPoster/AdminUploadCategoryPosterValidator.cs` | Validator |
| 4 | `src/.../Catalog/UseCases/Admin/Commands/UploadCategoryPoster/AdminUploadCategoryPosterMetaField.cs` | MetaField enum |
| 5 | `src/.../Catalog/UseCases/Admin/Commands/UploadCategoryPoster/V1/AdminUploadCategoryPosterEndpointV1.cs` | Endpoint |
| 6 | `src/.../Catalog/UseCases/Admin/Commands/SetExclusiveCategory/AdminSetExclusiveCategoryCommand.cs` | Command record |
| 7 | `src/.../Catalog/UseCases/Admin/Commands/SetExclusiveCategory/AdminSetExclusiveCategoryHandler.cs` | Set exclusive handler |
| 8 | `src/.../Catalog/UseCases/Admin/Commands/SetExclusiveCategory/AdminSetExclusiveCategoryMetaField.cs` | MetaField enum |
| 9 | `src/.../Catalog/UseCases/Admin/Commands/SetExclusiveCategory/V1/AdminSetExclusiveCategoryEndpointV1.cs` | Endpoint |
| 10 | `tests/.../Catalog/Admin/Commands/UploadCategoryPoster/AdminUploadCategoryPosterHandlerTests.cs` | Handler tests |
| 11 | `tests/.../Catalog/Admin/Commands/UploadCategoryPoster/V1/AdminUploadCategoryPosterEndpointV1Tests.cs` | Endpoint tests |
| 12 | `tests/.../Catalog/Admin/Commands/SetExclusiveCategory/AdminSetExclusiveCategoryHandlerTests.cs` | Handler tests |
| 13 | `tests/.../Catalog/Admin/Commands/SetExclusiveCategory/V1/AdminSetExclusiveCategoryEndpointV1Tests.cs` | Endpoint tests |

> All paths under `src/Modules/Content/Content/Application/` and `tests/Unit/Content/Application/`.

## Modified Source Files (13)

| # | File | Change |
|---|------|--------|
| 1 | `src/.../Domain/Entities/CategoryEntity.cs` | Add `PosterFileId`, `IsExclusive`, `SetPosterFileId()`, `ClearExclusive()` |
| 2 | `src/.../Infrastructure/Persistence/Configurations/CategoryConfiguration.cs` | Add property config + unique filtered index |
| 3 | `src/.../Application/Shared/DTOs/CategoryDto.cs` | Add `IsExclusive`, `PosterUrl` params |
| 4 | `src/.../Application/Shared/Mappers/CategoryMapper.cs` | Async rewrite for poster URL resolution |
| 5 | `src/.../Application/Shared/Repositories/ICategoryRepository.cs` | Add `GetExclusiveCategoryAsync` |
| 6 | `src/.../Infrastructure/Persistence/Repositories/CategoryRepository.cs` | Implement `GetExclusiveCategoryAsync` |
| 7 | `src/.../Application/Catalog/Specifications/CategorySpecifications.cs` | Add `ExclusiveCategorySpecification` |
| 8 | `src/.../Application/Shared/Errors/CategoryErrors.cs` | Add `CannotMakeInactiveExclusive` |
| 9 | `src/.../Application/Shared/Errors/Messages/CategoryErrorMessage.cs` | Add i18n key |
| 10 | `src/.../Catalog/UseCases/Admin/Commands/CreateCategory/AdminCreateCategoryCommand.cs` | Add `IsExclusive` param |
| 11 | `src/.../Catalog/UseCases/Admin/Commands/CreateCategory/AdminCreateCategoryHandler.cs` | Mutex logic + async mapper + `IFileRepository` |
| 12 | `src/.../Catalog/UseCases/Admin/Commands/CreateCategory/V1/AdminCreateCategoryEndpointV1.cs` | Add `IsExclusive` to request |
| 13 | `src/.../Catalog/UseCases/Admin/Commands/UpdateCategory/AdminUpdateCategoryCommand.cs` | Add `IsExclusive` param |
| 14 | `src/.../Catalog/UseCases/Admin/Commands/UpdateCategory/AdminUpdateCategoryHandler.cs` | Mutex logic + async mapper + `IFileRepository` |
| 15 | `src/.../Catalog/UseCases/Admin/Commands/UpdateCategory/V1/AdminUpdateCategoryEndpointV1.cs` | Add `IsExclusive` to request |
| 16 | `src/.../Catalog/UseCases/Admin/Queries/GetAllCategories/AdminGetAllCategoriesHandler.cs` | `IFileRepository` + async mapper |
| 17 | `src/.../Catalog/UseCases/Admin/Queries/GetCategoryById/AdminGetCategoryByIdHandler.cs` | `IFileRepository` + async mapper |
| 18 | `src/.../Catalog/UseCases/Public/Queries/GetActiveCategories/PublicGetActiveCategoriesHandler.cs` | `IFileRepository` + async mapper |

## Modified Test Files (12+)

| # | File | Change |
|---|------|--------|
| 1 | `tests/.../Domain/Entities/CategoryEntityTests.cs` | New tests for `IsExclusive`, `SetPosterFileId`, `ClearExclusive` |
| 2 | `tests/.../Catalog/Admin/Commands/CreateCategory/AdminCreateCategoryHandlerTests.cs` | `IFileRepository` mock, `IsExclusive`, mutex tests |
| 3 | `tests/.../Catalog/Admin/Commands/CreateCategory/AdminCreateCategoryValidatorTests.cs` | `IsExclusive` in commands |
| 4 | `tests/.../Catalog/Admin/Commands/CreateCategory/V1/AdminCreateCategoryEndpointV1Tests.cs` | `IsExclusive` in requests |
| 5 | `tests/.../Catalog/Admin/Commands/UpdateCategory/AdminUpdateCategoryHandlerTests.cs` | `IFileRepository` mock, `IsExclusive`, mutex tests |
| 6 | `tests/.../Catalog/Admin/Commands/UpdateCategory/AdminUpdateCategoryValidatorTests.cs` | `IsExclusive` in commands |
| 7 | `tests/.../Catalog/Admin/Commands/UpdateCategory/V1/AdminUpdateCategoryEndpointV1Tests.cs` | `IsExclusive` in requests |
| 8 | `tests/.../Catalog/Admin/Queries/GetAllCategories/AdminGetAllCategoriesHandlerTests.cs` | `IFileRepository` mock |
| 9 | `tests/.../Catalog/Admin/Queries/GetCategoryById/AdminGetCategoryByIdHandlerTests.cs` | `IFileRepository` mock |
| 10 | `tests/.../Catalog/Public/Queries/GetActiveCategories/PublicGetActiveCategoriesHandlerTests.cs` | `IFileRepository` mock |
| 11 | `tests/.../Catalog/Specifications/CategorySpecificationTests.cs` | `ExclusiveCategorySpecification` tests |
| 12 | `tests/.../Shared/Mappers/CategoryMapperTests.cs` | Async rewrite, poster URL resolution tests |
| 13 | `tests/Fixtures/Builders/Entities/Content/CategoryBuilder.cs` | `WithIsExclusive`, `WithPosterFileId` |
| 14 | `tests/Fixtures/Factories/Content/CategoryFactory.cs` | `isExclusive` param |
| 15 | `tests/Fixtures/Mocks/Content/MockCategoryRepository.cs` | `GetExclusiveCategoryAsync` setup |

## i18n Resource Files (2+)

| # | File | Change |
|---|------|--------|
| 1 | `src/.../Resources/en/CategoryErrorMessage.en.resx` | Add `Category.CannotMakeInactiveExclusive` |
| 2 | `src/.../Resources/fr/CategoryErrorMessage.fr.resx` | Add `Category.CannotMakeInactiveExclusive` |

## EF Migration (1)

| # | File | Generated by |
|---|------|-------------|
| 1 | `src/.../Infrastructure/Persistence/Migrations/*_AddPosterAndExclusiveToCategory.cs` | `dotnet ef migrations add` |

## Totals

| Type | Count |
| ---- | ----- |
| New source files | 9 |
| New test files | 4 |
| Modified source files | 18 |
| Modified test files | 15 |
| i18n resource files | 2+ |
| EF migration | 1 |
| **Total** | **~49 files** |
