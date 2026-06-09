# Lookup Sub-Module — Implementation Plan

> Build this first. Every other sub-module depends on at least one lookup table existing.

## Scope

| Entity | SQL Table | Repository |
|---|---|---|
| `ContentTypeEntity` | `content.content_types` | `ILookupRepository` |
| `PricingTierEntity` | `content.pricing_tiers` | `ILookupRepository` |
| `PromotionLevelEntity` | `content.promotion_levels` | `ILookupRepository` |
| `TagEntity` | `content.tags` | `ILookupRepository` |

---

## 🔴 CRUCIAL — Must exist before categories, pricing, or orders can be created

---

### POST /api/v1/admin/content-types

> Defines a new top-level content format (e.g. "Article", "Video"). This is the very first setup
> step the SuperAdmin must complete — categories cannot be created until at least one content type
> exists, which means no orders, no articles, and no videos can flow through the system.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `CreateContentTypeCommand(Name)` |
| **Response** | `201` + `ContentTypeDto(Id, Name, IsActive)` |

**TODOs**
- [x] `ContentTypeEntity.Create(id, name)` — validate name not empty, max 30 chars
- [x] `CreateContentTypeCommand(string Name) : ICommand<ContentTypeDto>`
- [x] `CreateContentTypeCommandValidator` — `Name` required, max `ContentConstants.MaxContentTypeNameLength`
- [x] `CreateContentTypeCommandHandler` — creates entity, calls `ILookupRepository.AddContentTypeAsync()`, commits `IContentUnitOfWork`
- [x] `LookupRepository.AddContentTypeAsync(contentType)` implementation against `ContentDbContext`
- [x] `CreateContentTypeEndpointV1` Carter module

---

### POST /api/v1/admin/pricing-tiers

> Defines a new add-on service fee (e.g. "base_upload", "social_boost"). Pricing tiers are the
> building blocks of every category's price list — without them, no paid category can be configured
> with a price, which means no orders can be submitted and no revenue can flow.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `CreatePricingTierCommand(Name, Description?)` |
| **Response** | `201` + `PricingTierDto(Id, Name, Description, IsActive)` |

**TODOs**
- [x] `PricingTierEntity.Create(id, name, description)` — validate name not empty, max 40 chars
- [x] `CreatePricingTierCommand(string Name, string? Description) : ICommand<PricingTierDto>`
- [x] `CreatePricingTierCommandValidator` — `Name` required, max `ContentConstants.MaxPricingTierNameLength`
- [x] `CreatePricingTierCommandHandler` — creates entity, calls `ILookupRepository.AddPricingTierAsync()`, commits UoW
- [x] `LookupRepository.AddPricingTierAsync(pricingTier)` implementation against `ContentDbContext`
- [x] `CreatePricingTierEndpointV1` Carter module

---

### POST /api/v1/admin/promotion-levels

> Creates a homepage placement upgrade option (e.g. "Featured — 7 days", "À la Une — 14 days").
> These are the upsell options available when a customer commissions content. Without at least one
> promotion level, the platform cannot offer featured placement, which is a core revenue add-on.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `CreatePromotionLevelCommand(Name, DurationDays, PriceUsd)` |
| **Response** | `201` + `PromotionLevelDto(Id, Name, DurationDays, PriceUsd, IsActive)` |

**TODOs**
- [x] `PromotionLevelEntity.Create(id, name, durationDays, priceUsd)` — validate `durationDays > 0`, `priceUsd >= 0`, name max 40 chars
- [x] `CreatePromotionLevelCommand(string Name, int DurationDays, decimal PriceUsd) : ICommand<PromotionLevelDto>`
- [x] `CreatePromotionLevelCommandValidator` — all fields required, `DurationDays > 0`, `PriceUsd >= 0`
- [x] `CreatePromotionLevelCommandHandler` — creates entity, calls `ILookupRepository.AddPromotionLevelAsync()`, commits UoW
- [x] `LookupRepository.AddPromotionLevelAsync(promotionLevel)` implementation against `ContentDbContext`
- [x] `CreatePromotionLevelEndpointV1` Carter module

---

## 🟡 IMPORTANT — Core management, needed for ongoing admin operations

---

### GET /api/v1/admin/content-types

> Returns the complete list of content types so the admin can review what formats currently exist
> before creating or editing categories. Provides the dropdown source on the "Create Category" form
> in the admin dashboard.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetAllContentTypesQuery` |
| **Response** | `200` + `IReadOnlyList<ContentTypeDto>` |

**TODOs**
- [x] `GetAllContentTypesQuery : IQuery<IReadOnlyList<ContentTypeDto>>`
- [x] `GetAllContentTypesQueryHandler` — calls `ILookupRepository.GetAllContentTypesAsync()`
- [x] `LookupRepository.GetAllContentTypesAsync()` — `ContentDbContext.ContentTypes.ToListAsync()`
- [x] `GetAllContentTypesEndpointV1` Carter module

---

### GET /api/v1/admin/pricing-tiers

> Lists all pricing tiers so the admin can see what add-ons are available before configuring
> category pricing. Also used as the data source when building the order form in the admin
> dashboard.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetAllPricingTiersQuery` |
| **Response** | `200` + `IReadOnlyList<PricingTierDto>` |

**TODOs**
- [x] `GetAllPricingTiersQuery : IQuery<IReadOnlyList<PricingTierDto>>`
- [x] `GetAllPricingTiersQueryHandler` — calls `ILookupRepository.GetAllPricingTiersAsync()`
- [x] `LookupRepository.GetAllPricingTiersAsync()` — `ContentDbContext.PricingTiers.ToListAsync()`
- [x] `GetAllPricingTiersEndpointV1` Carter module

---

### GET /api/v1/admin/promotion-levels

> Lists all promotion levels for admin use. Admins consult this list when advising customers on
> available homepage placement options and when building order items that include a promotion
> upgrade.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetAllPromotionLevelsQuery` |
| **Response** | `200` + `IReadOnlyList<PromotionLevelDto>` |

**TODOs**
- [x] `GetAllPromotionLevelsQuery : IQuery<IReadOnlyList<PromotionLevelDto>>`
- [x] `GetAllPromotionLevelsQueryHandler` — calls `ILookupRepository.GetAllPromotionLevelsAsync()`
- [x] `LookupRepository.GetAllPromotionLevelsAsync()` — `ContentDbContext.PromotionLevels.ToListAsync()`
- [x] `GetAllPromotionLevelsEndpointV1` Carter module

---

### PUT /api/v1/admin/pricing-tiers/{id}

> Allows a SuperAdmin to rename or update the description of a pricing tier when the service
> offering evolves (e.g. expanding "social_boost" to cover TikTok in addition to Facebook).
> Existing order price snapshots are unaffected.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `UpdatePricingTierCommand(Id, Name, Description?)` |
| **Response** | `200` + `PricingTierDto` |

**TODOs**
- [x] `UpdatePricingTierCommand(Guid Id, string Name, string? Description) : ICommand<PricingTierDto>`
- [x] `UpdatePricingTierCommandValidator` — `Id` not empty, `Name` required and max length
- [x] `UpdatePricingTierCommandHandler` — fetches entity via `ILookupRepository.GetPricingTierByIdOrThrowAsync()`, calls `PricingTierEntity.Update()`, commits UoW
- [x] `LookupRepository.GetPricingTierByIdOrThrowAsync(id)` — throws NotFoundException if not found
- [x] `UpdatePricingTierEndpointV1` Carter module

---

### PUT /api/v1/admin/promotion-levels/{id}

> Updates the name, duration, or price of a promotion level. Price changes apply only to new orders
> — past order snapshots are frozen. Useful when adjusting the "À la Une" duration from 7 to 14
> days or repricing the featured placement tier.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `UpdatePromotionLevelCommand(Id, Name, DurationDays, PriceUsd)` |
| **Response** | `200` + `PromotionLevelDto` |

**TODOs**
- [x] `UpdatePromotionLevelCommand(Guid Id, string Name, int DurationDays, decimal PriceUsd) : ICommand<PromotionLevelDto>`
- [x] `UpdatePromotionLevelCommandValidator`
- [x] `UpdatePromotionLevelCommandHandler` — fetches entity via `ILookupRepository.GetPromotionLevelByIdOrThrowAsync()`, calls `PromotionLevelEntity.Update()`, commits UoW
- [x] `LookupRepository.GetPromotionLevelByIdOrThrowAsync(id)` — throws NotFoundException if not found
- [x] `UpdatePromotionLevelEndpointV1` Carter module

---

## 🟢 MODERATE — Status management and tag operations

---

### PATCH /api/v1/admin/content-types/{id}/activate
### PATCH /api/v1/admin/content-types/{id}/deactivate

> Activating a content type makes it selectable again when creating new categories. Deactivating it
> prevents new categories from being assigned to it, effectively sunsetting that format without
> losing any historical data or breaking existing categories.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `ActivateContentTypeCommand(Id)` / `DeactivateContentTypeCommand(Id)` |
| **Response** | `204 No Content` |

**TODOs**
- [x] `ActivateContentTypeCommand(Guid Id) : ICommand`
- [x] `ActivateContentTypeCommandHandler` — fetches entity via `ILookupRepository.GetContentTypeByIdOrThrowAsync()`, calls `ContentTypeEntity.Activate()`, commits UoW
- [x] `DeactivateContentTypeCommand(Guid Id) : ICommand`
- [x] `DeactivateContentTypeCommandHandler` — fetches entity, calls `ContentTypeEntity.Deactivate()`, commits UoW
- [x] `LookupRepository.GetContentTypeByIdOrThrowAsync(id)` — throws NotFoundException if not found
- [x] `ActivateContentTypeEndpointV1` and `DeactivateContentTypeEndpointV1` Carter modules

---

### PATCH /api/v1/admin/pricing-tiers/{id}/activate
### PATCH /api/v1/admin/pricing-tiers/{id}/deactivate

> Deactivating a pricing tier removes it from the category pricing configuration form, preventing
> admins from assigning it to new categories. Existing category prices that already use this tier
> are unaffected. Useful when discontinuing an add-on service (e.g. removing a tier that is no
> longer offered).

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `ActivatePricingTierCommand(Id)` / `DeactivatePricingTierCommand(Id)` |
| **Response** | `204 No Content` |

**TODOs**
- [x] `ActivatePricingTierCommand(Guid Id) : ICommand` → calls `PricingTierEntity.Activate()`
- [x] `DeactivatePricingTierCommand(Guid Id) : ICommand` → calls `PricingTierEntity.Deactivate()`
- [x] `ActivatePricingTierEndpointV1` and `DeactivatePricingTierEndpointV1`

---

### PATCH /api/v1/admin/promotion-levels/{id}/activate
### PATCH /api/v1/admin/promotion-levels/{id}/deactivate

> Deactivating a promotion level hides it from the order form so customers can no longer select it
> for new orders. For example, if the "À la Une" slot is temporarily unavailable (homepage redesign,
> editorial freeze), it can be deactivated without deletion.

| | |
|---|---|
| **Auth** | SuperAdmin only |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `ActivatePromotionLevelCommand(Id)` / `DeactivatePromotionLevelCommand(Id)` |
| **Response** | `204 No Content` |

**TODOs**
- [x] `ActivatePromotionLevelCommand(Guid Id) : ICommand` → calls `PromotionLevelEntity.Activate()`
- [x] `DeactivatePromotionLevelCommand(Guid Id) : ICommand` → calls `PromotionLevelEntity.Deactivate()`
- [x] `ActivatePromotionLevelEndpointV1` and `DeactivatePromotionLevelEndpointV1`

---

### POST /api/v1/admin/tags

> Creates a new content tag (e.g. "Fally Ipupa", "Kinshasa", "Afrobeats") that editors can apply
> to articles and videos. Tags are the primary discovery mechanism for public users — they let
> readers find all content about a specific artist, genre, or topic without relying on categories.

| | |
|---|---|
| **Auth** | Admin+ |
| **Rate limit** | `ContentBrowsing` |
| **Command** | `CreateTagCommand(Name, Slug)` |
| **Response** | `201` + `TagDto(Id, Name, Slug)` |

**TODOs**
- [x] `TagEntity.Create(id, name, slug)` — validate name max 50 chars, slug max 60 chars
- [x] `CreateTagCommand(string Name, string Slug) : ICommand<TagDto>`
- [x] `CreateTagCommandValidator` — `Name` and `Slug` required, max lengths, slug must match lowercase-hyphen pattern
- [x] `CreateTagCommandHandler` — checks slug not already used (`ILookupRepository.GetTagBySlugAsync()`), creates entity, calls `ILookupRepository.AddTagAsync()`, commits UoW
- [x] `LookupRepository.AddTagAsync(tag)` and `LookupRepository.GetTagBySlugAsync(slug)`
- [x] `CreateTagEndpointV1` Carter module

---

## ⚪ TRIVIAL — Public-facing read endpoints

---

### GET /api/v1/public/tags

> Returns all tags for unauthenticated users. Powers the tag cloud, tag filter bar, and tag
> navigation on the public content listing pages. Lets site visitors browse all content related to a
> specific artist, genre, or topic without needing an account.

| | |
|---|---|
| **Auth** | Anonymous |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetAllTagsQuery` |
| **Response** | `200` + `IReadOnlyList<TagDto>` |

**TODOs**
- [x] `GetAllTagsQuery : IQuery<IReadOnlyList<TagDto>>`
- [x] `GetAllTagsQueryHandler` — calls `ILookupRepository.GetAllTagsAsync()`
- [x] `LookupRepository.GetAllTagsAsync()` — `ContentDbContext.Tags.OrderBy(t => t.Name).ToListAsync()`
- [x] `GetAllTagsEndpointV1` Carter module (`.AllowAnonymous()`)

---

### GET /api/v1/public/promotion-levels

> Returns active promotion levels to unauthenticated visitors. Used on the public-facing "Our
> Services" or pricing page so potential clients can see what homepage placement upgrades are
> available before getting in touch. Feeds directly into the customer-facing order brochure.

| | |
|---|---|
| **Auth** | Anonymous |
| **Rate limit** | `ContentBrowsing` |
| **Query** | `GetActivePromotionLevelsQuery` |
| **Response** | `200` + `IReadOnlyList<PromotionLevelDto>` |

**TODOs**
- [x] `GetActivePromotionLevelsQuery : IQuery<IReadOnlyList<PromotionLevelDto>>`
- [x] `GetActivePromotionLevelsQueryHandler` — calls `ILookupRepository.GetAllPromotionLevelsAsync()`, filters `IsActive = true`
- [x] `GetActivePromotionLevelsEndpointV1` Carter module (`.AllowAnonymous()`)