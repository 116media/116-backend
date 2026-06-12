namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for API routes used in testing.
    /// Mirrors route constants across all module route files in <c>src/Modules/*/Application/*/Constants/</c>.
    /// </summary>
    public static class ApiRoutes
    {
        public const string ApiVersion = "v1";
        public const string BaseUrl = "/api";

        public static class Admin
        {
            public const string Base = $"{BaseUrl}/{ApiVersion}/admin";
            public const string Auth = $"{Base}/auth";
            public const string Roles = $"{Base}/roles";
            public const string Permissions = $"{Base}/permissions";
            public const string Users = $"{Base}/users";
            public const string Sessions = $"{Base}/sessions";
            public const string Categories = $"{Base}/categories";
            public const string Videos = $"{Base}/videos";
            public const string Articles = $"{Base}/articles";
            public const string Shorts = $"{Base}/shorts";
            public const string Lyrics = $"{Base}/lyrics";
            public const string Packages = $"{Base}/packages";
            public const string Orders = $"{Base}/orders";
            public const string Customers = $"{Base}/customers";
            public const string ContentTypes = $"{Base}/content-types";
            public const string PricingTiers = $"{Base}/pricing-tiers";
            public const string PromotionLevels = $"{Base}/promotion-levels";
            public const string Tags = $"{Base}/tags";
        }

        public static class Public
        {
            public const string Base = $"{BaseUrl}/{ApiVersion}/public";
            public const string Auth = $"{Base}/auth";
            public const string Categories = $"{Base}/categories";
            public const string Videos = $"{Base}/videos";
            public const string Articles = $"{Base}/articles";
            public const string Shorts = $"{Base}/shorts";
            public const string Lyrics = $"{Base}/lyrics";
            public const string Packages = $"{Base}/packages";
            public const string Me = $"{Base}/me";
            public const string ContentTypes = $"{Base}/content-types";
            public const string PromotionLevels = $"{Base}/promotion-levels";
            public const string Tags = $"{Base}/tags";
            public const string Playlists = $"{Base}/playlists";
        }
    }
}
