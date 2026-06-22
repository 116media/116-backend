# Route Spec 02 — `Routes` helper + segment replacements

## Problem
Tests append literal sub-resource/action segments to base routes (~68 sites),
plus ~13 fully hardcoded `/api/v1/...` literals. These duplicate src constants.

## Before
```csharp
await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{id}/activate", null);
$"{ApiRoutes.Admin.Orders}/{order.Id}/items/{itemId}/tiers";
private const string Url = $"{ApiRoutes.Public.Auth}/change-password";
$"{ApiRoutes.Admin.Sessions}/force-logout/{userId}";
```

## After — add `tests/Fixtures/.../Routes.cs`
```csharp
public static class Routes
{
    public static class Admin
    {
        public static class Categories
        {
            public static string Activate(Guid id)   => $"{ApiRoutes.Admin.Categories}/{id}/{CatalogRouteConstants.Activate}";
            public static string Deactivate(Guid id) => $"{ApiRoutes.Admin.Categories}/{id}/{CatalogRouteConstants.Deactivate}";
            public static string Pricing(Guid id)    => $"{ApiRoutes.Admin.Categories}/{id}/{CatalogRouteConstants.Pricing}";
        }
        public static class Orders
        {
            public static string Items(Guid id)                 => $"{ApiRoutes.Admin.Orders}/{id}/{CommerceRouteConstants.Items}";
            public static string ItemTiers(Guid oid, Guid iid)  => $"{Items(oid)}/{iid}/{CommerceRouteConstants.Tiers}";
            public static string Submit(Guid id)                => $"{ApiRoutes.Admin.Orders}/{id}/{CommerceRouteConstants.Submit}";
            public static string Payment(Guid id)               => $"{ApiRoutes.Admin.Orders}/{id}/{CommerceRouteConstants.Payment}";
        }
        public static class Sessions
        {
            public static string ForceLogout(Guid userId) => $"{ApiRoutes.Admin.Sessions}/{SessionRouteConstants.ForceLogout}/{userId}";
            public static string Metrics()                 => $"{ApiRoutes.Admin.Sessions}/{SessionRouteConstants.Metrics}";
            public static string Export()                  => $"{ApiRoutes.Admin.Sessions}/{SessionRouteConstants.Export}";
            public static string Cleanup()                 => $"{ApiRoutes.Admin.Sessions}/{SessionRouteConstants.Cleanup}";
        }
    }
    public static class Public
    {
        public static class Auth
        {
            public static string ChangePassword() => $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.ChangePassword}";
            public static string ResetPassword()  => $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.ResetPassword}";
            public static string SetPassword()     => $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.SetPassword}";
        }
        public static class Articles
        {
            public static string Likes(Guid id)     => $"{ApiRoutes.Public.Articles}/{id}/{InteractionsRouteConstants.Likes}";
            public static string Bookmarks(Guid id) => $"{ApiRoutes.Public.Articles}/{id}/{InteractionsRouteConstants.Bookmarks}";
            public static string Comments(Guid id)  => $"{ApiRoutes.Public.Articles}/{id}/{InteractionsRouteConstants.Comments}";
            public static string Shares(Guid id)    => $"{ApiRoutes.Public.Articles}/{id}/{InteractionsRouteConstants.Shares}";
        }
    }
}
```
Extend the helper to cover every segment group below.

## Segment groups to cover (source constant → where used)
- **Catalog**: activate, deactivate, pricing, slots → `CatalogRouteConstants`.
- **Roles/Permissions**: activate, deactivate, restore, hard, permissions → `RoleRouteConstants` / `PermissionRouteConstants`.
- **Commerce**: items, tiers, submit, cancel, payment, proof, verify, reject, pending-payment → `CommerceRouteConstants`.
- **Editorial**: submit, approve, publish, reject, archive, seo, tags, youtube, thumbnail, shoot, images, unpromote, promoted, active, promotion/feed → `EditorialRouteConstants`.
- **Interactions**: likes, bookmarks, comments, ratings, shares, views, videos(playlist) → `InteractionsRouteConstants`.
- **Lookup**: activate, deactivate → `LookupRouteConstants`.
- **User**: avatar, profile → `UserRouteConstants`.
- **Session**: refresh-token, revoke, force-logout, metrics, export, cleanup → `SessionRouteConstants`.
- **Auth**: change-password, set-password, forgot-password, reset-password, login, signup, resend-otp, verify-otp, sign-out, sign-out-all, social-login → `AuthRouteConstants`.

## TODO checklist
- [ ] Create `Routes` helper covering all segment groups above.
- [ ] Replace ~13 hardcoded `/api/v1/...` literals (find: `grep -rn '"/api/v' tests/Integration`).
- [ ] Replace partial segments across tests; representative files:
  - [ ] `Modules/Content/.../Catalog/.../ActivateCategory/V1/AdminActivateCategoryEndpointV1Tests.cs`
  - [ ] `Modules/Content/.../Commerce/.../AddItemTier/V1/AdminAddItemTierEndpointV1Tests.cs`
  - [ ] `Modules/Content/.../Commerce/.../AddOrderItem/V1/AdminAddOrderItemEndpointV1Tests.cs`
  - [ ] `Modules/Identity/.../Auth/.../ChangePassword/V1/PublicChangePasswordEndpointV1Tests.cs`
  - [ ] `Modules/Identity/.../Session/.../RevokeSession/V1/AdminRevokeSessionEndpointV1Tests.cs`
  - [ ] `Workflows/InteractionFlowTests.cs`
  - [ ] …and every other test with a literal segment (sweep with the grep gate below).

## Acceptance
- `grep -rnE '/(activate|deactivate|restore|items|tiers|payment|change-password|set-password|avatar|profile|force-logout|metrics|export|cleanup|likes|bookmarks|comments|ratings|shares|views|submit|approve|publish|reject|archive|seo|youtube|thumbnail|shoot)\b' tests/Integration | grep -F '"'`
  → only matches inside the `Routes` helper / src, none in test method bodies.
- `grep -rn '"/api/v' tests/Integration` → 0.
