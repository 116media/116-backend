using _116.BuildingBlocks.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Role entity testing. The maximum lengths alias
    /// <see cref="RoleConstants" /> rather than copying it; the minimums are test-owned.
    /// </summary>
    public static class Role
    {
        /// <summary>
        /// The production maximum role name length.
        /// </summary>
        public const int NameMaxLength = RoleConstants.MaxRoleNameLength;

        /// <summary>
        /// Test-owned. Production enforces no minimum on the role name,
        /// so this value binds nothing in <c>src/</c>.
        /// </summary>
        public const int NameMinLength = 2;

        /// <summary>
        /// The production maximum role description length.
        /// </summary>
        public const int DescriptionMaxLength = RoleConstants.MaxRoleDescriptionLength;

        /// <summary>
        /// Test-owned. Production enforces no minimum on the role description,
        /// so this value binds nothing in <c>src/</c>.
        /// </summary>
        public const int DescriptionMinLength = 10;

        /// <summary>
        /// Test-owned fixture role name within the production length bound.
        /// </summary>
        public const string ValidName = "TestRole";

        /// <summary>
        /// Test-owned fixture role description within the production length bound.
        /// </summary>
        public const string ValidDescription = "A valid test role description for testing purposes.";

        /// <summary>
        /// The seeded super administrator role name.
        /// </summary>
        public const string SuperAdminName = "SuperAdmin";

        /// <summary>
        /// The seeded administrator role name.
        /// </summary>
        public const string AdminName = "Admin";

        /// <summary>
        /// The seeded visitor role name.
        /// </summary>
        public const string VisitorName = "Visitor";

        /// <summary>
        /// Test-owned description for the super administrator role fixture.
        /// </summary>
        public const string SuperAdminDescription = "Full system access with all permissions.";

        /// <summary>
        /// Test-owned description for the administrator role fixture.
        /// </summary>
        public const string AdminDescription = "Administrative access to manage system resources.";

        /// <summary>
        /// Test-owned description for the visitor role fixture.
        /// </summary>
        public const string VisitorDescription = "Standard public user with limited access.";
    }
}
