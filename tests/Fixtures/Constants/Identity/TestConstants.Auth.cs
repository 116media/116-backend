namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Auth/Login testing. Every value is test-owned credential and provider
    /// fixture data with no production counterpart; the passwords satisfy the complexity rules.
    /// </summary>
    public static class Auth
    {
        public const string PublicLoginEmail = "user@example.com";
        public const string PublicLoginUserName = "testuser";
        public const string ValidPassword = "Test123!abc";
        public const string PublicLoginPassword = "Password123!";
        public const string PublicLoginInvalidPassword = "WrongPassword!";

        public const string OldPassword = "Old123!abc";
        public const string NewPassword = "New123!abc";
        public const string ChangedPassword = "NewPass123!abc";
        public const string IncorrectCurrentPassword = "WrongPassword123!";
        public const string SocialAccountPassword = "AnyPass123!";
        public const string ResetNewPassword = "NewSecure123!abc";

        public const string AdminLoginEmail = "admin@example.com";
        public const string AdminLoginPassword = "Password123!";
        public const string AdminLoginInvalidPassword = "WrongPassword!";

        public const string SocialLoginEmail = "socialuser@example.com";
        public const string SocialLoginUserName = "socialuser";
        public const string SocialLoginAvatarUrl = "https://avatar.url/image.jpg";

        public const string ProviderGoogle = "Google";
        public const string ProviderGitHub = "GitHub";
        public const string ProviderFacebook = "Facebook";
        public const string ProviderMicrosoft = "Microsoft";

        public const string NonExistentEmail = "nonexistent@example.com";
    }
}
