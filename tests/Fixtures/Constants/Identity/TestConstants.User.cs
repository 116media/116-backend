namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for User entity testing.
    /// Mirrors <c>src/BuildingBlocks/Constants/UserConstants.cs</c>.
    /// </summary>
    public static class User
    {
        public const int EmailMaxLength = 256;
        public const int UserNameMaxLength = 50;
        public const int UserNameMinLength = 3;
        public const int PasswordMinLength = 8;
        public const int PasswordMaxLength = 128;
        public const int CountryMaxLength = 100;
        public const int PhoneMaxLength = 20;

        public const string ValidEmail = "test@example.com";
        public const string ValidUserName = "testuser";
        public const string ValidPassword = "SecureP@ssw0rd!";
        public const string ValidCountry = "United States";
        public const string ValidPhone = "+1234567890";

        public const string SuperAdminEmail = "superadmin@116.com";
        public const string AdminEmail = "admin@116.com";
        public const string VisitorEmail = "visitor@116.com";

        public static readonly Guid SuperAdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public static readonly Guid AdminId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        public static readonly Guid VisitorId = Guid.Parse("00000000-0000-0000-0000-000000000003");

        public const string DefaultPasswordHash = "$2a$11$K8Xj1E9kQ2wP3rT4y5U6IeHashed.Password.ForTesting";
    }
}
