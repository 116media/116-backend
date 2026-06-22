# Route Spec 01 — Rewrite `ApiRoutes` from src constants

## Problem
`tests/Fixtures/Constants/Shared/TestConstants.ApiRoutes.cs` hardcodes route
strings (`"auth"`, `"categories"`, `"v1"`, `"admin"`, …) that already exist in
`src`. Two sources of truth → silent drift when a route changes.

## Before
```csharp
public static class ApiRoutes
{
    public const string ApiVersion = "v1";
    public const string BaseUrl = "/api";

    public static class Admin
    {
        public const string Base = $"{BaseUrl}/{ApiVersion}/admin";
        public const string Auth = $"{Base}/auth";
        public const string Categories = $"{Base}/categories";
        // ...all literals
    }
}
```

## After
```csharp
using _116.Identity.Domain.Constants;            // Admin / Public / Me
using _116.Content.Domain.Constants;             // ContentConstants
using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Roles.Constants;
using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.User.Constants;
using _116.Content.Application.Catalog.Constants;
using _116.Content.Application.Commerce.Constants;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Lookup.Constants;

public static class ApiRoutes
{
    public const string ApiVersion = "v1";
    public const string BaseUrl = "/api";

    public static class Admin
    {
        public const string Base = $"{BaseUrl}/{ApiVersion}/{IdentityConstants.Admin}";
        public const string Auth = $"{Base}/{AuthRouteConstants.Endpoint}";
        public const string Roles = $"{Base}/{RoleRouteConstants.Endpoint}";
        public const string Permissions = $"{Base}/{PermissionRouteConstants.Endpoint}";
        public const string Sessions = $"{Base}/{SessionRouteConstants.Endpoint}";
        public const string Categories = $"{Base}/{CatalogRouteConstants.Categories}";
        public const string Customers = $"{Base}/{CatalogRouteConstants.Customers}";
        public const string Packages = $"{Base}/{CatalogRouteConstants.Packages}";
        public const string Orders = $"{Base}/{CommerceRouteConstants.Orders}";
        public const string Payments = $"{Base}/{CommerceRouteConstants.Payments}";
        public const string Articles = $"{Base}/{EditorialRouteConstants.Articles}";
        public const string Videos = $"{Base}/{EditorialRouteConstants.Videos}";
        public const string Shorts = $"{Base}/{EditorialRouteConstants.Shorts}";
        public const string Lyrics = $"{Base}/{EditorialRouteConstants.Lyrics}";
        public const string ContentTypes = $"{Base}/{LookupRouteConstants.ContentTypes}";
        public const string PricingTiers = $"{Base}/{LookupRouteConstants.PricingTiers}";
        public const string PromotionLevels = $"{Base}/{LookupRouteConstants.PromotionLevels}";
        public const string Tags = $"{Base}/{LookupRouteConstants.Tags}";
        public const string Users = $"{Base}/users";   // confirm a UsersRouteConstants exists; else add one in src
    }

    public static class Public
    {
        public const string Base = $"{BaseUrl}/{ApiVersion}/{IdentityConstants.Public}";
        public const string Me = $"{Base}/{IdentityConstants.Me}";
        public const string Auth = $"{Base}/{AuthRouteConstants.Endpoint}";
        public const string Categories = $"{Base}/{CatalogRouteConstants.Categories}";
        public const string Articles = $"{Base}/{InteractionsRouteConstants.Articles}";
        public const string Videos = $"{Base}/{InteractionsRouteConstants.Videos}";
        public const string Shorts = $"{Base}/{InteractionsRouteConstants.Shorts}";
        public const string Playlists = $"{Base}/{InteractionsRouteConstants.Playlists}";
        // ...
    }
}
```

Notes:
- All segment constants in `src` are `const string`, so the interpolated test
  constants remain compile-time `const`.
- The `Users` base has no `*RouteConstants` today — either add `UserRouteConstants`
  usage if one represents the admin users collection, or introduce a src constant.
  Capture as a sub-task; do not reintroduce a bare literal silently.

## TODO checklist
- [ ] Add the `using` directives for every module `*RouteConstants` + scope constants.
- [ ] Replace every literal segment in `Admin` with the matching src constant.
- [ ] Replace every literal segment in `Public` with the matching src constant.
- [ ] Resolve the `Users` base (add/confirm a src constant).
- [ ] `dotnet build tests/Fixtures` — 0 errors.

## Acceptance
- `TestConstants.ApiRoutes` contains no bare route literals except `ApiVersion`,
  `BaseUrl`, and (temporarily) `users`.
- Changing a segment in a src `*RouteConstants` changes the test URL automatically.
