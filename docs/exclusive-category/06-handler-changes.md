# 06 — Handler Changes

## AdminCreateCategoryHandler

**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/CreateCategory/AdminCreateCategoryHandler.cs`

### Changes

1. Add `IFileRepository fileRepository` to the primary constructor
2. Add exclusive mutex enforcement before `AddAsync`
3. Pass `isExclusive: command.IsExclusive` to `CategoryEntity.Create()`
4. Handle optional poster upload via `command.Poster`
5. Switch mapper call from sync `ToCategoryDto` to async `ToCategoryDtoAsync`

```csharp
public class AdminCreateCategoryHandler(
    ILookupRepository lookupRepository,
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,        // <-- new
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminCreateCategoryCommand, AdminCreateCategoryResult>
{
    public async Task<AdminCreateCategoryResult> Handle(
        AdminCreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        // ... existing slug uniqueness check ...

        // Mutex: unset current exclusive if setting this one
        if (command.IsExclusive)
        {
            CategoryEntity? currentExclusive = await categoryRepository.GetExclusiveCategoryAsync(
                cancellationToken: cancellationToken);

            if (currentExclusive is not null)
            {
                currentExclusive.ClearExclusive();
            }
        }

        var category = CategoryEntity.Create(
            // ... existing params ...
            isGossip: command.IsGossip,
            isExclusive: command.IsExclusive    // <-- new
        );

        // Optional poster upload at creation time
        if (command.Poster is not null)
        {
            FileEntity posterFile = await fileRepository.UploadAndStoreImageFileAsync(
                file: command.Poster,
                publicId: category.Id.ToString(),
                folder: "content/category-posters",
                originalFileName: command.Poster.FileName,
                mimeType: command.Poster.ContentType,
                cancellationToken: cancellationToken);

            category.SetPosterFileId(posterFileId: posterFile.Id);
        }

        await categoryRepository.AddAsync(category: category, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity created = await categoryRepository.GetByIdOrThrowAsync(
            id: category.Id, cancellationToken: cancellationToken);

        var dto = await created.ToCategoryDtoAsync(mapper, fileRepository, cancellationToken);
        return new AdminCreateCategoryResult(Category: dto);
    }
}
```

### AdminCreateCategoryCommand

**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/CreateCategory/AdminCreateCategoryCommand.cs`

Add `IsExclusive` and optional `Poster`:

```csharp
public record AdminCreateCategoryCommand(
    string ContentTypeId,
    string Name,
    string Slug,
    string Description,
    bool IsFree,
    bool IsGossip,
    bool IsExclusive,         // <-- new
    IFormFile? Poster          // <-- new (optional poster upload at creation)
) : ICommand<AdminCreateCategoryResult>;
```

### AdminCreateCategoryEndpointV1

**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/CreateCategory/V1/AdminCreateCategoryEndpointV1.cs`

Add `IsExclusive` and `Poster` to request DTO. The endpoint changes from `JSON` to `multipart/form-data` to support the optional file upload:

```csharp
public record AdminCreateCategoryRequest(
    string Name,
    string Slug,
    string Description,
    bool IsFree,
    bool IsGossip,
    bool IsExclusive,         // <-- new
    IFormFile? Poster          // <-- new (optional)
);
```

---

## AdminUpdateCategoryHandler

**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategory/AdminUpdateCategoryHandler.cs`

### Changes

1. Add `IFileRepository fileRepository` to the primary constructor
2. Add exclusive mutex enforcement before `Update()`
3. Pass `isExclusive: command.IsExclusive` to `category.Update()`
4. Handle optional poster replacement via `command.Poster`
5. Switch mapper call from sync `ToCategoryDto` to async `ToCategoryDtoAsync`

```csharp
public class AdminUpdateCategoryHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,        // <-- new
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminUpdateCategoryCommand, AdminUpdateCategoryResult>
{
    public async Task<AdminUpdateCategoryResult> Handle(
        AdminUpdateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        // ... existing slug conflict check ...

        // Mutex: unset current exclusive if setting this one
        if (command.IsExclusive)
        {
            CategoryEntity? currentExclusive = await categoryRepository.GetExclusiveCategoryAsync(
                cancellationToken: cancellationToken);

            if (currentExclusive is not null && currentExclusive.Id != category.Id)
            {
                currentExclusive.ClearExclusive();
            }
        }

        // Optional poster replacement
        if (command.Poster is not null)
        {
            FileEntity posterFile = await fileRepository.ReplaceImageFileAsync(
                currentFileId: category.PosterFileId,
                file: command.Poster,
                publicId: category.Id.ToString(),
                folder: "content/category-posters",
                originalFileName: command.Poster.FileName,
                mimeType: command.Poster.ContentType,
                cancellationToken: cancellationToken);

            category.SetPosterFileId(posterFileId: posterFile.Id);
        }

        category.Update(
            name: command.Name,
            slug: command.Slug,
            description: command.Description,
            isGossip: command.IsGossip,
            isExclusive: command.IsExclusive,    // <-- new
            errors: i18n.Category
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id, cancellationToken: cancellationToken);

        var dto = await updated.ToCategoryDtoAsync(mapper, fileRepository, cancellationToken);
        return new AdminUpdateCategoryResult(Category: dto);
    }
}
```

### AdminUpdateCategoryCommand

**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategory/AdminUpdateCategoryCommand.cs`

```csharp
public record AdminUpdateCategoryCommand(
    string Id,
    string Name,
    string Slug,
    string Description,
    bool IsGossip,
    bool IsExclusive,         // <-- new
    IFormFile? Poster          // <-- new (optional poster replacement)
) : ICommand<AdminUpdateCategoryResult>;
```

### AdminUpdateCategoryEndpointV1

**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategory/V1/AdminUpdateCategoryEndpointV1.cs`

The endpoint changes from `JSON` to `multipart/form-data` to support the optional file upload:

```csharp
public record AdminUpdateCategoryRequest(
    string Name,
    string Slug,
    string Description,
    bool IsGossip,
    bool IsExclusive,         // <-- new
    IFormFile? Poster          // <-- new (optional)
);
```

---

## AdminSetExclusiveCategoryHandler (New Use Case)

New use case at:
```
src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/SetExclusiveCategory/
├── AdminSetExclusiveCategoryCommand.cs
├── AdminSetExclusiveCategoryHandler.cs
├── AdminSetExclusiveCategoryMetaField.cs
└── V1/
    └── AdminSetExclusiveCategoryEndpointV1.cs
```

### Endpoint

```
PATCH /api/v1/admin/categories/{id}/set-exclusive
```

| Field | Value |
|-------|-------|
| Method | `PATCH` |
| Auth | `RequireAuthorization` (admin) |
| Rate limit | `ContentBrowsing` |
| Request body | None |
| Response | `AdminSetExclusiveCategoryResult` containing the updated `CategoryDto` |

### Command

```csharp
public record AdminSetExclusiveCategoryCommand(string Id)
    : ICommand<AdminSetExclusiveCategoryResult>;

public record AdminSetExclusiveCategoryResult(CategoryDto Category);
```

### Handler

```csharp
public class AdminSetExclusiveCategoryHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminSetExclusiveCategoryCommand, AdminSetExclusiveCategoryResult>
{
    public async Task<AdminSetExclusiveCategoryResult> Handle(
        AdminSetExclusiveCategoryCommand command,
        CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(command.Id);

        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: id, cancellationToken: cancellationToken);

        // Mutex: unset current exclusive
        CategoryEntity? currentExclusive = await categoryRepository.GetExclusiveCategoryAsync(
            cancellationToken: cancellationToken);

        if (currentExclusive is not null && currentExclusive.Id != id)
        {
            currentExclusive.ClearExclusive();
        }

        category.SetExclusive();

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id, cancellationToken: cancellationToken);

        var dto = await updated.ToCategoryDtoAsync(mapper, fileRepository, cancellationToken);
        return new AdminSetExclusiveCategoryResult(Category: dto);
    }
}
```

No validator needed — `Id` comes from the route parameter.

---

## Query Handlers (Async Mapper Migration)

All query handlers that call `ToCategoryDto` or `ToCategoryDtos` need:

1. `IFileRepository fileRepository` added to the primary constructor
2. Mapper calls updated to async variants

| Handler | File | Change |
|---------|------|--------|
| `AdminGetAllCategoriesHandler` | `Catalog/UseCases/Admin/Queries/GetAllCategories/` | `ToCategoryDtos` → `ToCategoryDtosAsync` |
| `AdminGetCategoryByIdHandler` | `Catalog/UseCases/Admin/Queries/GetCategoryById/` | `ToCategoryDto` → `ToCategoryDtoAsync` |
| `PublicGetActiveCategoriesHandler` | `Catalog/UseCases/Public/Queries/GetActiveCategories/` | `ToCategoryDtos` → `ToCategoryDtosAsync` |

Pattern for each:

```csharp
// Before
var dto = category.ToCategoryDto(mapper);

// After
var dto = await category.ToCategoryDtoAsync(mapper, fileRepository, cancellationToken);
```

```csharp
// Before
var dtos = categories.ToCategoryDtos(mapper);

// After
var dtos = await categories.ToCategoryDtosAsync(mapper, fileRepository, cancellationToken);
```
