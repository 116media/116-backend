using _116.Identity.Domain.Enums;

namespace _116.Unit.Tests.Common.Constants;

/// <summary>
/// Contains constants used across all unit tests.
/// </summary>
public static class TestConstants
{
    /// <summary>
    /// Constants for Role entity testing.
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

    /// <summary>
    /// Constants for Permission entity testing.
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

    /// <summary>
    /// Constants for User entity testing.
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

        public const string DefaultPasswordHash = "$2a$11$K8Xj1E9kQ2wP3rT4y5U6IeHashed.Password.ForTesting";
    }

    /// <summary>
    /// Constants for Auth/Login testing.
    /// </summary>
    public static class Auth
    {
        // Public login test data
        public const string PublicLoginEmail = "user@example.com";
        public const string PublicLoginUserName = "testuser";
        public const string PublicLoginPassword = "Password123!";
        public const string PublicLoginInvalidPassword = "WrongPassword!";

        // Admin login test data
        public const string AdminLoginEmail = "admin@example.com";
        public const string AdminLoginPassword = "Password123!";
        public const string AdminLoginInvalidPassword = "WrongPassword!";

        // Social login test data
        public const string SocialLoginEmail = "socialuser@example.com";
        public const string SocialLoginUserName = "socialuser";
        public const string SocialLoginAvatarUrl = "https://avatar.url/image.jpg";

        // Social providers
        public const string ProviderGoogle = "Google";
        public const string ProviderGitHub = "GitHub";
        public const string ProviderFacebook = "Facebook";
        public const string ProviderMicrosoft = "Microsoft";

        // Non-existent user test data
        public const string NonExistentEmail = "nonexistent@example.com";
    }

    /// <summary>
    /// Constants for Session entity testing.
    /// </summary>
    public static class Session
    {
        public const int DeviceIdMaxLength = 256;
        public const int IpAddressMaxLength = 45;
        public const int UserAgentMaxLength = 512;

        public const string ValidDeviceId = "device-abc123-xyz789";
        public const string ValidIpAddress = "192.168.1.100";
        public const string ValidUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

        public const int DefaultAccessTokenExpirationMinutes = 60;
        public const int DefaultRefreshTokenExpirationDays = 30;

        public const string DefaultIpAddress = "127.0.0.1";
        public const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
        public static readonly EnumBrowser DefaultBrowser = EnumBrowser.Chrome;
        public static readonly EnumDevice DefaultDevice = EnumDevice.Desktop;
        public static readonly EnumPlatform DefaultPlatform = EnumPlatform.Windows;
        public static readonly EnumClient DefaultClient = EnumClient.WebApp;
        public const string DefaultDeviceId = "test-device-id-12345";
        public const string DefaultRefreshToken = "default-refresh-token-for-testing-base64encoded";
        public const string DefaultRefreshTokenHash = "hashed_default_refresh_token_sha256";
    }

    /// <summary>
    /// Constants for OTP entity testing.
    /// </summary>
    public static class Otp
    {
        public const int CodeLength = 6;
        public const int MaxAttempts = 5;
        public const int ExpirationMinutes = 10;

        public const string ValidCode = "123456";
        public const string InvalidCode = "000000";
        public const string DefaultCode = "654321";
    }

    /// <summary>
    /// Constants for File entity testing.
    /// </summary>
    public static class File
    {
        public const int FileNameMaxLength = 255;
        public const int MimeTypeMaxLength = 100;
        public const int StorageUrlMaxLength = 2048;

        public const string ValidFileName = "test-file.jpg";
        public const string ValidOriginalFileName = "original-test-file.jpg";
        public const string ValidMimeType = "image/jpeg";
        public const string ValidStorageUrl = "https://res.cloudinary.com/test/image/upload/v1234567890/test-file.jpg";
        public const long ValidSizeInBytes = 1024 * 100; // 100 KB
    }

    /// <summary>
    /// Constants for JWT testing.
    /// </summary>
    public static class Jwt
    {
        public const string ValidSecret = "ThisIsAVerySecureSecretKeyForTesting123!@#";
        public const string ValidIssuer = "116_test";
        public const string ValidAudience = "116_test_client";
        public const int AccessTokenExpirationMinutes = 60;
        public const int RefreshTokenExpirationDays = 30;
        public const string ValidAccessToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IlRlc3QgVXNlciIsImlhdCI6MTUxNjIzOTAyMn0.test_signature";
    }

    /// <summary>
    /// Constants for validation error messages testing.
    /// </summary>
    public static class ValidationMessages
    {
        public const string RequiredField = "is required";
        public const string InvalidFormat = "is not valid";
        public const string TooShort = "too short";
        public const string TooLong = "too long";
        public const string AlreadyExists = "already exists";
        public const string NotFound = "not found";

        /// <summary>
        /// Role-specific validation error messages.
        /// </summary>
        public static class Role
        {
            public const string NameRequired = "Role name is required";
            public const string NameTooLong = "Role name cannot exceed 20 characters";
            public const string DescriptionRequired = "Role description is required";
            public const string DescriptionTooLong = "Role description cannot exceed 300 characters";
        }

        /// <summary>
        /// Permission-specific validation error messages.
        /// </summary>
        public static class Permission
        {
            public const string ResourceRequired = "Permission resource is required";
            public const string ResourceTooLong = "Permission resource cannot exceed 15 characters";
            public const string ActionRequired = "Permission action is required";
            public const string ActionTooLong = "Permission action cannot exceed 15 characters";
            public const string DescriptionRequired = "Permission description is required";
            public const string DescriptionTooLong = "Permission description cannot exceed 300 characters";
        }

        /// <summary>
        /// GUID validation error messages.
        /// </summary>
        public static class Guid
        {
            public const string RoleIdRequired = "Role ID is required.";
            public const string RoleIdInvalid = "Role ID is invalid.";
            public const string PermissionIdRequired = "Permission ID is required.";
            public const string PermissionIdInvalid = "Permission ID is invalid.";
        }
    }

    /// <summary>
    /// Constants for API routes used in testing.
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
