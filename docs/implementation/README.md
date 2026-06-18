# Content Module — Implementation Index

Each file covers one sub-module. Build them in this order — later sub-modules depend on earlier ones.

| Order | File | Sub-module | Depends on |
|---|---|---|---|
| 1 | `01-lookup.md` | ContentTypes, PricingTiers, PromotionLevels, Tags | — |
| 2 | `02-catalog.md` | Categories, CategoryPricing, Customers, Packages | Lookup |
| 3 | `03-editorial.md` | Articles, Videos, ShortVideos, Lyrics | Catalog |
| 4 | `04-commerce.md` | Orders, OrderItems, ItemTiers, Payments | Catalog + Editorial |
| 5 | `05-interactions.md` | Likes, Bookmarks, Shares, Comments, Ratings, Playlists | Editorial |

## Priority Legend

| Symbol | Level | Meaning |
|---|---|---|
| 🔴 | CRUCIAL | Blocks everything else — implement first |
| 🟡 | IMPORTANT | Core business features |
| 🟢 | MODERATE | Supporting features, admin UX |
| ⚪ | TRIVIAL | Nice-to-have, low-risk |

## Shared conventions (all endpoints)

- URL pattern: `/api/v1/{scope}/{resource}`
- Scopes:
  - `admin` — staff/admin operations (requires `Admin` or `SuperAdmin` role). Folder: `UseCases/Admin/`
  - `public` — visitor-facing operations, both authenticated (`RequireVisitorOnly`) and anonymous (`.AllowAnonymous()`). Folder: `UseCases/Public/`
- All admin endpoints: `.RequireAuthorization(policy: UserRolePolicies.RequireAdminOnly)` or `RequireSuperAdminOnly`
- All visitor authenticated endpoints: `.WithAuthorization(UserRolePolicies.RequireVisitorOnly)`
- Anonymous endpoints: `.AllowAnonymous()`
- All Carter modules: `.WithApiVersionSet(VersionSets.Default).MapToApiVersion(1)`
- All commands dispatched via `IDispatcher.Send()`
- No endpoint returns `204 No Content` — all return `200 OK` with `{ IsSuccess: true }` or `201 Created` with data