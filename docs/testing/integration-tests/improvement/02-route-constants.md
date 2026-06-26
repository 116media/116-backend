# 02 — Route Constants: Single Source of Truth

Production already centralizes every URL segment in
`src/Modules/**/Constants/*RouteConstants.cs`. The tests **ignore those** and
re-hardcode the same strings in `tests/Fixtures/Constants/Shared/TestConstants.ApiRoutes.cs`,
plus ~68 inline segments and ~13 fully-hardcoded `/api/...` literals. When a
route changes in `src`, the tests silently drift.

## Source-of-truth inventory (in `src`)

Scope + version:
- `src/BuildingBlocks/Utils/ApiVersionUrl.cs` — `/api/v{version}/{path}`.
- `Identity.Domain.Constants.IdentityConstants` — `Admin`, `Public`, `Me`.
- `Content.Domain.Constants.ContentConstants` — `Admin`, `Public`.

Route-constant classes (segment name → value highlights):

| Class | Module | Key segments |
| --- | --- | --- |
| `AuthRouteConstants` | Identity/Auth | `Endpoint=auth`, `login`, `signup`, `change-password`, `set-password`, `forgot-password`, `reset-password`, `resend-otp`, `verify-otp`, `sign-out`, `sign-out-all`, `social-login` |
| `RoleRouteConstants` | Identity/Roles | `Endpoint=roles`, `activate`, `deactivate`, `restore`, `hard`, `permissions` |
| `PermissionRouteConstants` | Identity/Roles | `Endpoint=permissions`, `activate`, `deactivate`, `restore`, `hard` |
| `UserRouteConstants` | Identity/User | `Endpoint=user`, `avatar`, `profile` |
| `SessionRouteConstants` | Identity/Session | `Endpoint=sessions`, `refresh-token`, `revoke`, `force-logout`, `metrics`, `export`, `cleanup` |
| `CatalogRouteConstants` | Content/Catalog | `categories`, `customers`, `packages`, `pricing`, `slots`, `activate`, `deactivate` |
| `CommerceRouteConstants` | Content/Commerce | `orders`, `items`, `tiers`, `submit`, `cancel`, `payment`, `proof`, `verify`, `reject`, `pending-payment`, `payments` |
| `EditorialRouteConstants` | Content/Editorial | `articles`, `videos`, `shorts`, `lyrics`, `submit`, `approve`, `publish`, `reject`, `archive`, `images`, `tags`, `seo`, `youtube`, `thumbnail`, `shoot`, `activate`, `deactivate`, `promoted`, `active`, `unpromote`, `promotion/feed` |
| `InteractionsRouteConstants` | Content/Interactions | `articles`, `videos`, `shorts`, `playlists`, `likes`, `bookmarks`, `shares`, `comments`, `ratings`, `views` |
| `LookupRouteConstants` | Content/Lookup | `content-types`, `pricing-tiers`, `promotion-levels`, `tags`, `activate`, `deactivate` |

The Fixtures `.csproj` already references all module assemblies, so these are
directly usable from test code — no new dependency needed.

## The two anti-patterns in tests

**1. Hardcoded mirror.** `TestConstants.ApiRoutes` literally re-types the
segments (`Auth = $"{Base}/auth"`, `Categories = $"{Base}/categories"`, …) with a
comment admitting it "mirrors" src.

**2. Partial hardcoding.** Tests append literal sub-resource/action segments to a
base route:

```csharp
// ❌ "activate" is a literal duplicating CatalogRouteConstants.Activate
await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/activate", null);

// ❌ "items"/"tiers" duplicate CommerceRouteConstants
$"{ApiRoutes.Admin.Orders}/{order.Id}/items/{itemId}/tiers"

// ❌ "change-password" duplicates AuthRouteConstants.ChangePassword
private const string Url = $"{ApiRoutes.Public.Auth}/change-password";
```

## The refactor

**Step A — `ApiRoutes` composed from src.** Rewrite
`TestConstants.ApiRoutes` so bases come from src constants:

```csharp
using _116.Identity.Domain.Constants;
using _116.Identity.Application.Auth.Constants;
using _116.Content.Application.Catalog.Constants;
// ...

public static class ApiRoutes
{
    public const string ApiVersion = "v1";
    public const string BaseUrl = "/api";

    public static class Admin
    {
        public const string Base = $"{BaseUrl}/{ApiVersion}/{IdentityConstants.Admin}";
        public const string Auth = $"{Base}/{AuthRouteConstants.Endpoint}";
        public const string Categories = $"{Base}/{CatalogRouteConstants.Categories}";
        // ...
    }
}
```

(These remain `const` because the src constants are `const`.)

**Step B — typed route helpers.** Add a `Routes` helper layer in `tests/Fixtures`
for sub-resource/action URLs so no test ever concatenates a literal segment:

```csharp
public static class Routes
{
    public static class Admin
    {
        public static class Categories
        {
            public static string Activate(Guid id) =>
                $"{ApiRoutes.Admin.Categories}/{id}/{CatalogRouteConstants.Activate}";
            public static string Pricing(Guid id) =>
                $"{ApiRoutes.Admin.Categories}/{id}/{CatalogRouteConstants.Pricing}";
        }
        public static class Orders
        {
            public static string Items(Guid orderId) =>
                $"{ApiRoutes.Admin.Orders}/{orderId}/{CommerceRouteConstants.Items}";
            public static string ItemTiers(Guid orderId, Guid itemId) =>
                $"{ApiRoutes.Admin.Orders}/{orderId}/{CommerceRouteConstants.Items}/{itemId}/{CommerceRouteConstants.Tiers}";
        }
    }
    public static class Public
    {
        public static class Me
        {
            public static string Roles() => $"{ApiRoutes.Public.Me}/{RoleRouteConstants.Endpoint}";
        }
    }
}
```

**Step C — replace usages.** Swap the ~68 partial segments and ~13 hardcoded
literals across the suite for `ApiRoutes.*` + `Routes.*` + `*RouteConstants.*`.

The full segment-by-segment list and per-file checklist are in
[`specs/routes/01-apiroutes-rewrite.md`](specs/routes/01-apiroutes-rewrite.md) and
[`specs/routes/02-segment-replacements.md`](specs/routes/02-segment-replacements.md).

## Acceptance

- `grep -rn '"/api/v' tests/Integration` → 0 results.
- No literal action/sub-resource segments (`/activate`, `/items`, `/change-password`, …) in test URL strings.
- `TestConstants.ApiRoutes` references src constants, not literals.
