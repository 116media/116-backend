using _116.BuildingBlocks.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for User entity testing. The numeric limits alias
    /// <see cref="UserConstants" /> rather than copying it; the rest is test fixture data.
    /// </summary>
    public static class User
    {
        /// <summary>
        /// The production maximum email length (RFC 5321).
        /// </summary>
        public const int EmailMaxLength = UserConstants.MaxEmailLength;

        /// <summary>
        /// The production maximum username length.
        /// </summary>
        public const int UserNameMaxLength = UserConstants.MaxUserNameLength;

        /// <summary>
        /// The production minimum username length.
        /// </summary>
        public const int UserNameMinLength = UserConstants.MinUserNameLength;

        /// <summary>
        /// The production minimum password length.
        /// </summary>
        public const int PasswordMinLength = UserConstants.MinPasswordLength;

        /// <summary>
        /// Test-owned. Production declares no maximum password length,
        /// so this value binds nothing in <c>src/</c>.
        /// </summary>
        public const int PasswordMaxLength = 128;

        /// <summary>
        /// The production maximum country name length.
        /// </summary>
        public const int CountryMaxLength = UserConstants.MaxCountryNameLength;

        /// <summary>
        /// The production maximum full phone number length.
        /// </summary>
        public const int PhoneMaxLength = UserConstants.MaxFullPhoneNumberLength;

        /// <summary>
        /// Test-owned fixture email used wherever a test needs an address that parses.
        /// </summary>
        public const string ValidEmail = "test@example.com";

        /// <summary>
        /// Test-owned fixture username within the production length bounds.
        /// </summary>
        public const string ValidUserName = "testuser";

        /// <summary>
        /// Test-owned fixture password that satisfies the production complexity rules.
        /// </summary>
        public const string ValidPassword = "SecureP@ssw0rd!";

        /// <summary>
        /// Test-owned fixture country name.
        /// </summary>
        public const string ValidCountry = "United States";

        /// <summary>
        /// Test-owned fixture phone number in E.164 form.
        /// </summary>
        public const string ValidPhone = "+1234567890";

        /// <summary>
        /// Test-owned email for the seeded super administrator fixture.
        /// </summary>
        public const string SuperAdminEmail = "superadmin@116.com";

        /// <summary>
        /// Test-owned email for the seeded administrator fixture.
        /// </summary>
        public const string AdminEmail = "admin@116.com";

        /// <summary>
        /// Test-owned email for the seeded visitor fixture.
        /// </summary>
        public const string VisitorEmail = "visitor@116.com";

        /// <summary>
        /// Test-owned deterministic identifier for the super administrator fixture.
        /// </summary>
        public static readonly Guid SuperAdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        /// <summary>
        /// Test-owned deterministic identifier for the administrator fixture.
        /// </summary>
        public static readonly Guid AdminId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        /// <summary>
        /// Test-owned deterministic identifier for the visitor fixture.
        /// </summary>
        public static readonly Guid VisitorId = Guid.Parse("00000000-0000-0000-0000-000000000003");

        /// <summary>
        /// Test-owned placeholder hash shaped like a BCrypt digest; it hashes no known
        /// plaintext.
        /// </summary>
        public const string DefaultPasswordHash = "$2a$11$K8Xj1E9kQ2wP3rT4y5U6IeHashed.Password.ForTesting";
    }
}
