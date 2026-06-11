namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Permission entity testing.
    /// Mirrors <c>src/BuildingBlocks/Constants/PermissionConstants.cs</c>.
    /// </summary>
    public static class Permission
    {
        public const int ResourceMaxLength = 15;
        public const int ResourceMinLength = 2;
        public const int ActionMaxLength = 15;
        public const int ActionMinLength = 2;
        public const int DescriptionMaxLength = 300;
        public const int DescriptionMinLength = 10;

        public const string ValidResource = "users";
        public const string ValidAction = "read";
        public const string ValidDescription = "Allows reading user information from the system.";
    }
}
