using _116.BuildingBlocks.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Permission entity testing. The maximum lengths alias
    /// <see cref="PermissionConstants" /> rather than copying it; the minimums are test-owned.
    /// </summary>
    public static class Permission
    {
        /// <summary>
        /// The production maximum permission resource length.
        /// </summary>
        public const int ResourceMaxLength = PermissionConstants.MaxPermissionResourceLength;

        /// <summary>
        /// Test-owned. Production enforces no minimum on the resource,
        /// so this value binds nothing in <c>src/</c>.
        /// </summary>
        public const int ResourceMinLength = 2;

        /// <summary>
        /// The production maximum permission action length, a separate constant from the
        /// resource limit.
        /// </summary>
        public const int ActionMaxLength = PermissionConstants.MaxPermissionActionLength;

        /// <summary>
        /// Test-owned. Production enforces no minimum on the action,
        /// so this value binds nothing in <c>src/</c>.
        /// </summary>
        public const int ActionMinLength = 2;

        /// <summary>
        /// The production maximum permission description length.
        /// </summary>
        public const int DescriptionMaxLength = PermissionConstants.MaxPermissionDescriptionLength;

        /// <summary>
        /// Test-owned. Production enforces no minimum on the description,
        /// so this value binds nothing in <c>src/</c>.
        /// </summary>
        public const int DescriptionMinLength = 10;

        /// <summary>
        /// Test-owned fixture resource name within the production length bound.
        /// </summary>
        public const string ValidResource = "users";

        /// <summary>
        /// Test-owned fixture action name within the production length bound.
        /// </summary>
        public const string ValidAction = "read";

        /// <summary>
        /// Test-owned fixture permission description within the production length bound.
        /// </summary>
        public const string ValidDescription = "Allows reading user information from the system.";
    }
}
