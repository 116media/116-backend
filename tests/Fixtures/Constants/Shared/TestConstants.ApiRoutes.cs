using _116.Content.Application.Catalog.Constants;
using _116.Content.Application.Commerce.Constants;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Lookup.Constants;
using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Roles.Constants;
using _116.Identity.Application.Session.Constants;
using _116.Identity.Domain.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Base API route constants used in testing.
    /// Composed from the production route constants in
    /// <c>src/Modules/*/Application/*/Constants/*RouteConstants.cs</c> so the test
    /// URLs stay in lockstep with the real routes (single source of truth).
    /// Sub-resource and action segments are built via the <c>Routes</c> helper.
    /// </summary>
    public static class ApiRoutes
    {
        public const string ApiVersion = "v1";
        public const string BaseUrl = "/api";

        /// <summary>
        /// The unscoped artist claim-request route base — <c>POST /api/v1/artists/{id}/claim</c>
        /// deliberately lives outside both the <c>admin</c> and <c>public</c> route groups.
        /// </summary>
        public const string Artists = $"{BaseUrl}/{ApiVersion}/{EditorialRouteConstants.Artists}";

        /// <summary>
        /// The unscoped lyrics submission/correction-revision route base — e.g.
        /// <c>POST /api/v1/lyrics/submissions</c>, <c>POST /api/v1/lyrics/{id}/revisions</c> —
        /// deliberately lives outside both the <c>admin</c> and <c>public</c> route groups.
        /// </summary>
        public const string Lyrics = $"{BaseUrl}/{ApiVersion}/{EditorialRouteConstants.Lyrics}";

        /// <summary>
        /// The unscoped translation-revision route base — e.g.
        /// <c>POST /api/v1/translations/{id}/revisions</c> — deliberately lives outside both the
        /// <c>admin</c> and <c>public</c> route groups.
        /// </summary>
        public const string Translations = $"{BaseUrl}/{ApiVersion}/{EditorialRouteConstants.Translations}";

        public static class Admin
        {
            public const string Base = $"{BaseUrl}/{ApiVersion}/{IdentityConstants.Admin}";

            public const string Auth = $"{Base}/{AuthRouteConstants.Endpoint}";
            public const string Roles = $"{Base}/{RoleRouteConstants.Endpoint}";
            public const string Permissions = $"{Base}/{PermissionRouteConstants.Endpoint}";

            // No dedicated route-constant class exists for the admin users
            // collection yet; tracked in specs/routes/01-apiroutes-rewrite.md.
            public const string Users = $"{Base}/users";

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
            public const string Artists = $"{Base}/{EditorialRouteConstants.Artists}";

            /// <summary>
            /// Admin newsletter subscribers listing route.
            /// </summary>
            public const string NewsletterSubscribers = $"{Base}/newsletter/subscribers";
            public const string Albums = $"{Base}/{EditorialRouteConstants.Albums}";

            /// <summary>
            /// The admin-scoped translation-revision moderation route base — e.g.
            /// <c>PUT /api/v1/admin/translations/revisions/{id}</c>.
            /// </summary>
            public const string Translations = $"{Base}/{EditorialRouteConstants.Translations}";
        }

        public static class Public
        {
            public const string Base = $"{BaseUrl}/{ApiVersion}/{IdentityConstants.Public}";

            public const string Auth = $"{Base}/{AuthRouteConstants.Endpoint}";
            public const string Me = $"{Base}/{IdentityConstants.Me}";
            public const string Categories = $"{Base}/{CatalogRouteConstants.Categories}";
            public const string Packages = $"{Base}/{CatalogRouteConstants.Packages}";
            public const string Articles = $"{Base}/{InteractionsRouteConstants.Articles}";
            public const string Videos = $"{Base}/{InteractionsRouteConstants.Videos}";
            public const string Shorts = $"{Base}/{InteractionsRouteConstants.Shorts}";
            public const string Playlists = $"{Base}/{InteractionsRouteConstants.Playlists}";
            public const string Lyrics = $"{Base}/{EditorialRouteConstants.Lyrics}";
            public const string ContentTypes = $"{Base}/{LookupRouteConstants.ContentTypes}";
            public const string PromotionLevels = $"{Base}/{LookupRouteConstants.PromotionLevels}";
            public const string Tags = $"{Base}/{LookupRouteConstants.Tags}";
            public const string Artists = $"{Base}/{EditorialRouteConstants.Artists}";

            /// <summary>
            /// Public newsletter routes (subscriptions, confirm, unsubscribe).
            /// </summary>
            public const string Newsletter = $"{Base}/newsletter";

            /// <summary>
            /// Public in-app notification routes (feed, unread-count, read, read-all).
            /// </summary>
            public const string Notifications = $"{Base}/notifications";
        }
    }
}
