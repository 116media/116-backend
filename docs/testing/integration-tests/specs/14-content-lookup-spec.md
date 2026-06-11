# Phase 12: Content Module — Lookup API Tests Spec

## Tasks

### Admin ContentType Commands
- [ ] `AdminCreateContentTypeEndpointTests.cs`
  - [ ] Post_AsSuperAdmin_WithValidData_ShouldReturn201
  - [ ] Post_AsAdmin_ShouldReturn403
  - [ ] Post_WithDuplicateName_ShouldReturn409
  - [ ] Post_WithInvalidData_ShouldReturn422
- [ ] `AdminUpdateContentTypeEndpointTests.cs`
  - [ ] Put_AsSuperAdmin_ShouldReturn200
- [ ] `AdminActivateContentTypeEndpointTests.cs`
- [ ] `AdminDeactivateContentTypeEndpointTests.cs`

### Admin PricingTier Commands
- [ ] `AdminCreatePricingTierEndpointTests.cs`
  - [ ] Post_AsAdmin_WithValidData_ShouldReturn201
  - [ ] Post_WithDuplicateName_ShouldReturn409
- [ ] `AdminUpdatePricingTierEndpointTests.cs`
- [ ] `AdminActivatePricingTierEndpointTests.cs`
- [ ] `AdminDeactivatePricingTierEndpointTests.cs`

### Admin PromotionLevel Commands
- [ ] `AdminCreatePromotionLevelEndpointTests.cs`
  - [ ] Post_AsAdmin_WithValidData_ShouldReturn201
- [ ] `AdminUpdatePromotionLevelEndpointTests.cs`
- [ ] `AdminActivatePromotionLevelEndpointTests.cs`
- [ ] `AdminDeactivatePromotionLevelEndpointTests.cs`

### Admin Tag Commands
- [ ] `AdminCreateTagEndpointTests.cs`
  - [ ] Post_AsAdmin_WithValidData_ShouldReturn201
  - [ ] Post_WithDuplicateName_ShouldReturn409
- [ ] `AdminUpdateTagEndpointTests.cs`
- [ ] `AdminDeleteTagEndpointTests.cs`

### Admin Lookup Queries
- [ ] `AdminGetAllContentTypesEndpointTests.cs`
  - [ ] Get_AsAdmin_ShouldReturn200WithContentTypes
- [ ] `AdminGetAllPricingTiersEndpointTests.cs`
- [ ] `AdminGetAllPromotionLevelsEndpointTests.cs`
- [ ] `AdminGetAllTagsEndpointTests.cs`
  - [ ] Get_AsAdmin_ShouldReturn200WithPaginatedTags

### Public Lookup Queries
- [ ] `PublicGetAllContentTypesEndpointTests.cs`
  - [ ] Get_Anonymous_ShouldReturn200
- [ ] `PublicGetAllTagsEndpointTests.cs`
  - [ ] Get_Anonymous_ShouldReturn200
- [ ] `PublicGetPopularTagsEndpointTests.cs`
  - [ ] Get_Anonymous_ShouldReturn200OrderedByUsage
- [ ] `PublicGetActivePromotionLevelsEndpointTests.cs`
  - [ ] Get_Anonymous_ShouldReturn200WithActiveOnly

## File Locations

```
tests/_116.Integration.Tests/Content/Api/Lookup/
├── ContentTypes/
│   ├── AdminCreateContentTypeEndpointTests.cs
│   ├── AdminUpdateContentTypeEndpointTests.cs
│   ├── AdminActivateContentTypeEndpointTests.cs
│   ├── AdminDeactivateContentTypeEndpointTests.cs
│   ├── AdminGetAllContentTypesEndpointTests.cs
│   └── PublicGetAllContentTypesEndpointTests.cs
├── PricingTiers/
│   ├── AdminCreatePricingTierEndpointTests.cs
│   ├── AdminUpdatePricingTierEndpointTests.cs
│   ├── AdminActivatePricingTierEndpointTests.cs
│   ├── AdminDeactivatePricingTierEndpointTests.cs
│   └── AdminGetAllPricingTiersEndpointTests.cs
├── PromotionLevels/
│   ├── AdminCreatePromotionLevelEndpointTests.cs
│   ├── AdminUpdatePromotionLevelEndpointTests.cs
│   ├── AdminActivatePromotionLevelEndpointTests.cs
│   ├── AdminDeactivatePromotionLevelEndpointTests.cs
│   ├── AdminGetAllPromotionLevelsEndpointTests.cs
│   └── PublicGetActivePromotionLevelsEndpointTests.cs
└── Tags/
    ├── AdminCreateTagEndpointTests.cs
    ├── AdminUpdateTagEndpointTests.cs
    ├── AdminDeleteTagEndpointTests.cs
    ├── AdminGetAllTagsEndpointTests.cs
    ├── PublicGetAllTagsEndpointTests.cs
    └── PublicGetPopularTagsEndpointTests.cs
```

## Acceptance Criteria

1. Every lookup endpoint has integration tests
2. CRUD lifecycle verified for each lookup entity
3. Activate/deactivate state transitions verified
4. Public endpoints return only active entities
5. `./scripts/run-tests-with-coverage.sh integration` passes
