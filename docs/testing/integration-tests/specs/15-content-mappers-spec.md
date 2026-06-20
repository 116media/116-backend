# Phase 13: Content Module — Mapper Tests Spec

## Tasks

### ArticleMapper
- [ ] `ArticleMapperTests.cs`
  - [ ] ToArticleDtoAsync_ShouldMapAllFields
  - [ ] ToArticleDtoAsync_WithTags_ShouldIncludeTags
  - [ ] ToArticleDtoAsync_WithImages_ShouldIncludeImageUrls
  - [ ] ToArticleSummaryDtosAsync_ShouldMapCollection

### CategoryMapper
- [ ] `CategoryMapperTests.cs`
  - [ ] ToCategoryDtoAsync_ShouldMapAllFields
  - [ ] ToCategoryDtoAsync_WithPoster_ShouldIncludePosterUrl
  - [ ] ToCategoryDtoAsync_WithPricings_ShouldIncludePricings

### ContentOrderMapper
- [ ] `ContentOrderMapperTests.cs`
  - [ ] ToOrderDtoAsync_ShouldMapAllFields
  - [ ] ToOrderDtoAsync_WithItems_ShouldIncludeItemDetails
  - [ ] ToOrderDtoAsync_WithPayment_ShouldIncludePaymentInfo

### ContentTypeMapper
- [ ] `ContentTypeMapperTests.cs`
  - [ ] ToContentTypeDtoAsync_ShouldMapAllFields

### CustomerMapper
- [ ] `CustomerMapperTests.cs`
  - [ ] ToCustomerDtoAsync_ShouldMapAllFields

### LyricsMapper
- [ ] `LyricsMapperTests.cs`
  - [ ] ToLyricsDtoAsync_ShouldMapAllFields

### PackageMapper
- [ ] `PackageMapperTests.cs`
  - [ ] ToPackageDtoAsync_ShouldMapAllFields
  - [ ] ToPackageDtoAsync_WithSlots_ShouldIncludeSlots

### PlaylistMapper
- [ ] `PlaylistMapperTests.cs`
  - [ ] ToPlaylistDtoAsync_ShouldMapAllFields
  - [ ] ToPlaylistDtoAsync_WithVideos_ShouldIncludeVideoSummaries

### PricingTierMapper
- [ ] `PricingTierMapperTests.cs`
  - [ ] ToPricingTierDtoAsync_ShouldMapAllFields

### PromotionLevelMapper
- [ ] `PromotionLevelMapperTests.cs`
  - [ ] ToPromotionLevelDtoAsync_ShouldMapAllFields

### ShortVideoMapper
- [ ] `ShortVideoMapperTests.cs`
  - [ ] ToShortVideoDtoAsync_ShouldMapAllFields
  - [ ] ToShortVideoDtoAsync_WithThumbnail_ShouldIncludeUrl

### TagMapper
- [ ] `TagMapperTests.cs`
  - [ ] ToTagDtoAsync_ShouldMapAllFields

### VideoMapper
- [ ] `VideoMapperTests.cs`
  - [ ] ToVideoDtoAsync_ShouldMapAllFields
  - [ ] ToVideoDtoAsync_WithTags_ShouldIncludeTags
  - [ ] ToVideoDtoAsync_WithThumbnail_ShouldIncludeUrl
  - [ ] ToVideoSummaryDtosAsync_ShouldMapCollection

## Test Approach

Mapper tests use `BaseApiTest` because mappers resolve `IFileRepository` to build file URLs.

```csharp
[Collection("Database")]
public class CategoryMapperTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ToCategoryDtoAsync_ShouldMapAllFields()
    {
        // Arrange — seed category with content type
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeEntity.Create(/* ... */);
        context.ContentTypes.Add(contentType);
        var category = CategoryEntity.Create(/* ... */);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        // Act — resolve mapper and map
        using var scope = Api.Services.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<CategoryMapper>();
        var dto = await mapper.ToCategoryDtoAsync(category);

        // Assert
        dto.Id.Should().Be(category.Id);
        dto.Name.Should().Be(category.Name);
        dto.Slug.Should().Be(category.Slug);
        dto.ContentType.Should().NotBeNull();
    }
}
```

## File Locations

```
tests/_116.Integration.Tests/Content/Mappers/
├── ArticleMapperTests.cs
├── CategoryMapperTests.cs
├── ContentOrderMapperTests.cs
├── ContentTypeMapperTests.cs
├── CustomerMapperTests.cs
├── LyricsMapperTests.cs
├── PackageMapperTests.cs
├── PlaylistMapperTests.cs
├── PricingTierMapperTests.cs
├── PromotionLevelMapperTests.cs
├── ShortVideoMapperTests.cs
├── TagMapperTests.cs
└── VideoMapperTests.cs
```

## Acceptance Criteria

1. Every mapper method has at least one integration test
2. All DTO fields verified with exact values
3. File URL resolution verified (via stub Cloudinary)
4. Navigation property mapping verified (e.g., tags, pricings)
5. `./scripts/run-tests-with-coverage.sh integration` passes
