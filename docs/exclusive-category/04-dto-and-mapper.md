# 04 — DTO and Mapper Changes

## CategoryDto

**File:** `src/Modules/Content/Content/Application/Shared/DTOs/CategoryDto.cs`

Add two new fields:

```csharp
public record CategoryDto(
    Guid Id,
    Guid ContentTypeId,
    string ContentTypeName,
    string Name,
    string Slug,
    string Description,
    bool IsFree,
    bool IsActive,
    bool IsGossip,
    bool IsExclusive,                          // <-- new
    string? PosterUrl,                         // <-- new (resolved from FileEntity)
    IReadOnlyList<CategoryPricingDto> Pricing
);
```

- `IsExclusive` maps directly from the entity property
- `PosterUrl` is resolved at mapping time from `PosterFileId` via `IFileRepository`

## CategoryMapper — Async Rewrite

**File:** `src/Modules/Content/Content/Application/Shared/Mappers/CategoryMapper.cs`

The mapper needs an async rewrite to resolve `PosterFileId` → `PosterUrl` via `IFileRepository`. This follows the same pattern used for `ArticleMapper` (cover image) and `VideoMapper` (thumbnail).

### Mapster Registration

Update the `CategoryEntity → CategoryDto` config to ignore `PosterUrl` (resolved manually):

```csharp
config
    .NewConfig<CategoryEntity, CategoryDto>()
    .Map(dest => dest.ContentTypeName, src => src.ContentType.Name)
    .Map(dest => dest.Pricing, src => src.Pricing)
    .Map(dest => dest.PosterUrl, _ => (string?)null);
```

### New Async Extension Methods

Replace the sync methods with async equivalents:

```csharp
/// <summary>
/// Maps a CategoryEntity to a CategoryDto, resolving PosterUrl from FileEntity.
/// </summary>
public static async Task<CategoryDto> ToCategoryDtoAsync(
    this CategoryEntity entity,
    IMapper mapper,
    IFileRepository fileRepository,
    CancellationToken cancellationToken = default)
{
    var dto = mapper.Map<CategoryDto>(entity)
        with { Pricing = mapper.Map<IReadOnlyList<CategoryPricingDto>>(entity.Pricing) };

    if (entity.PosterFileId.HasValue)
    {
        FileEntity? posterFile = await fileRepository.GetByIdAsync(
            fileId: entity.PosterFileId.Value,
            cancellationToken: cancellationToken);

        if (posterFile is not null)
        {
            dto = dto with { PosterUrl = posterFile.StorageUrl };
        }
    }

    return dto;
}

/// <summary>
/// Maps a collection of CategoryEntity to a list of CategoryDto,
/// resolving PosterUrl from FileEntity for each.
/// </summary>
public static async Task<IReadOnlyList<CategoryDto>> ToCategoryDtosAsync(
    this IReadOnlyList<CategoryEntity> entities,
    IMapper mapper,
    IFileRepository fileRepository,
    CancellationToken cancellationToken = default)
{
    var dtos = new List<CategoryDto>(entities.Count);

    foreach (CategoryEntity entity in entities)
    {
        dtos.Add(await entity.ToCategoryDtoAsync(mapper, fileRepository, cancellationToken));
    }

    return dtos;
}
```

### Keep Sync Methods (Optional)

The sync `ToCategoryDto` and `ToCategoryDtos` can be removed or kept as deprecated. Since all handlers will be updated to use the async variants, removing them is cleaner.

The `ToCategoryPricingDto` method stays sync — no FileEntity resolution needed.
