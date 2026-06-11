namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Role entity testing.
    /// Mirrors <c>src/BuildingBlocks/Constants/RoleConstants.cs</c>.
    /// </summary>
    public static class Role
    {
        public const int NameMaxLength = 20;
        public const int NameMinLength = 2;
        public const int DescriptionMaxLength = 300;
        public const int DescriptionMinLength = 10;

        public const string ValidName = "TestRole";
        public const string ValidDescription = "A valid test role description for testing purposes.";

        public const string SuperAdminName = "SuperAdmin";
        public const string AdminName = "Admin";
        public const string VisitorName = "Visitor";

        public const string SuperAdminDescription = "Full system access with all permissions.";
        public const string AdminDescription = "Administrative access to manage system resources.";
        public const string VisitorDescription = "Standard public user with limited access.";
    }
}
