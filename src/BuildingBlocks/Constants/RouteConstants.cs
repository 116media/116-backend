namespace _116.BuildingBlocks.Constants;

/// <summary>
/// Provides a centralized collection of API route constants, serving as a single source of truth for endpoint paths.
/// This ensures consistency across the application, reduces duplication, and simplifies future maintenance.
/// </summary>
public static class RouteConstants
{
    public static class V1
    {
        private const string Base = "/api/v1";

        /// <summary>
        /// Contains administrative API routes (users, roles, authentication).
        /// </summary>
        public static class Admin
        {
            /// <summary>
            /// Route for managing users (create, update, delete, list).
            /// </summary>
            public const string Users = $"{Base}/admin/users";

            /// <summary>
            /// Route for managing roles and permissions.
            /// </summary>
            public const string Roles = $"{Base}/admin/roles";

            /// <summary>
            /// Route for administrative authentication (i.e., login, forgot-password, etc.).
            /// </summary>
            public const string Auth = $"{Base}/admin/auth";

            /// <summary>
            /// Route for accessing and updating admin profiles.
            /// </summary>
            public const string Profile = $"{Base}/admin/profile";
        }

        /// <summary>
        /// Contains publicly accessible API routes.
        /// </summary>
        public static class Public
        {
            /// <summary>
            /// Route for public authentication (login, register).
            /// </summary>
            public const string Auth = $"{Base}/public/auth";

            /// <summary>
            /// Route for accessing and updating user profiles.
            /// </summary>
            public const string Profile = $"{Base}/public/profile";
        }
    }
}

