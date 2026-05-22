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
            public const string Roles = $"{Base}/roles";
            public const string Permissions = $"{Base}/permissions";
            public const string Users = $"{Base}/users";
            public const string Sessions = $"{Base}/sessions";
            public const string Auth = $"{Base}/auth";
        }

        public static class Public
        {
            public const string Base = $"{BaseUrl}/{ApiVersion}/public";
            public const string Auth = $"{Base}/auth";
        }
    }
}
