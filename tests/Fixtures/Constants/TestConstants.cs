using _116.Identity.Domain.Enums;

namespace _116.Tests.Fixtures.Constants;

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
            public const string UserIdRequired = "User ID is required.";
            public const string UserIdInvalid = "User ID is invalid.";
        }
    }

    /// <summary>
    /// Constants for Content module entity testing.
    /// </summary>
    public static class Content
    {
        /// <summary>Constants for ContentType entity testing.</summary>
        public static class ContentType
        {
            public const int NameMaxLength = 30;
            public const string ValidName = "Article";
            public const string AnotherValidName = "Video";
        }

        /// <summary>Constants for PricingTier entity testing.</summary>
        public static class PricingTier
        {
            public const int NameMaxLength = 40;
            public const int DescriptionMaxLength = 200;
            public const string ValidName = "base_upload";
            public const string AnotherValidName = "social_boost";
            public const string ValidDescription = "Base upload fee for content.";
        }

        /// <summary>Constants for PromotionLevel entity testing.</summary>
        public static class PromotionLevel
        {
            public const int NameMaxLength = 40;
            public const string ValidName = "Featured — 7 days";
            public const string AnotherValidName = "À la Une — 14 days";
            public const int ValidDurationDays = 7;
            public const decimal ValidPriceUsd = 50m;
            public const decimal ZeroPriceUsd = 0m;
        }

        /// <summary>Constants for Tag entity testing.</summary>
        public static class Tag
        {
            public const int NameMaxLength = 50;
            public const int SlugMaxLength = 60;
            public const string ValidName = "Fally Ipupa";
            public const string ValidSlug = "fally-ipupa";
            public const string AnotherValidName = "Kinshasa";
            public const string AnotherValidSlug = "kinshasa";
        }

        /// <summary>Constants for Category entity testing.</summary>
        public static class Category
        {
            public const int NameMaxLength = 60;
            public const int SlugMaxLength = 80;
            public const int DescriptionMaxLength = 300;
            public const string ValidName = "Artist Profile";
            public const string ValidSlug = "artist-profile";
            public const string AnotherValidName = "116 Le Focus";
            public const string AnotherValidSlug = "116-le-focus";
            public const string ValidDescription = "Artist profile category.";
        }

        /// <summary>Constants for Customer entity testing.</summary>
        public static class Customer
        {
            public const int FullNameMaxLength = 100;
            public const int EmailMaxLength = 200;
            public const int PhoneMaxLength = 30;
            public const int CompanyMaxLength = 100;
            public const int NotesMaxLength = 500;
            public const string ValidFullName = "John Doe";
            public const string ValidEmail = "customer@example.com";
            public const string AnotherValidEmail = "other@example.com";
            public const string ValidPhone = "+243812345678";
            public const string ValidCompany = "Acme Music Label";
            public const string ValidNotes = "VIP customer with special pricing.";
        }

        /// <summary>Constants for Package entity testing.</summary>
        public static class Package
        {
            public const int NameMaxLength = 100;
            public const int DescriptionMaxLength = 500;
            public const string ValidName = "Artist Starter Pack";
            public const string AnotherValidName = "Premium Bundle";
            public const string ValidDescription = "Includes 1 artist profile and 1 interview.";
            public const decimal ValidFlatPriceUsd = 300m;
            public const decimal ZeroFlatPriceUsd = 0m;
        }

        /// <summary>Constants for PackageSlot entity testing.</summary>
        public static class PackageSlot
        {
            public const int ValidQuantity = 1;
            public const int AnotherValidQuantity = 2;
        }

        /// <summary>Constants for CategoryPricing entity testing.</summary>
        public static class CategoryPricing
        {
            public const decimal ValidPriceUsd = 25m;
            public const decimal ZeroPriceUsd = 0m;
            public const decimal UpdatedPriceUsd = 50m;
        }

        /// <summary>Constants for Editorial entity testing (Article, Video, ShortVideo, Lyrics).</summary>
        public static class Editorial
        {
            /// <summary>Constants for Article entity testing.</summary>
            public static class Article
            {
                public const int TitleMaxLength = 100;
                public const int SlugMaxLength = 220;
                public const int HeadlineMinLength = 100;
                public const int HeadlineMaxLength = 300;
                public const int RejectionReasonMaxLength = 500;

                public const string ValidTitle = "Fally Ipupa — Portrait d'un Géant";
                public const string ValidSlug = "fally-ipupa-portrait-dun-geant";
                public const string ValidHeadline =
                    "Retour sur la carrière époustouflante de Fally Ipupa, artiste congolais qui a conquis l'Afrique et le monde entier avec son style unique.";
                public const string ValidBody = "<p>Corps de l'article complet ici.</p>";
                public const string ValidRejectionReason = "Le contenu n'est pas conforme aux standards éditoriaux.";
            }

            /// <summary>Constants for Video entity testing.</summary>
            public static class Video
            {
                public const int TitleMaxLength = 100;
                public const int SlugMaxLength = 220;
                public const int YoutubeVideoIdMaxLength = 20;
                public const int RejectionReasonMaxLength = 500;

                public const string ValidTitle = "116 Le Focus — Fally Ipupa";
                public const string ValidSlug = "116-le-focus-fally-ipupa";
                public const string ValidYoutubeVideoId = "dQw4w9WgXcQ";
                public const string ValidDescription = "Épisode complet de 116 Le Focus avec Fally Ipupa.";
                public const string ValidRejectionReason = "La qualité vidéo ne répond pas aux critères requis.";
            }

            /// <summary>Constants for ShortVideo entity testing.</summary>
            public static class ShortVideo
            {
                public const int TitleMaxLength = 200;
                public const int SlugMaxLength = 220;

                public const string ValidTitle = "Teaser — Fally Ipupa Focus";
                public const string ValidSlug = "teaser-fally-ipupa-focus";
                public const string ValidVideoUrl = "https://res.cloudinary.com/test/video/upload/v1/test-short.mp4";
                public const string ValidVideoStorageKey = "content/shorts/test-short";
            }

            /// <summary>Constants for Lyrics entity testing.</summary>
            public static class Lyrics
            {
                public const int SongTitleMaxLength = 200;
                public const int ArtistNameMaxLength = 100;
                public const int LanguageMaxLength = 5;

                public const string ValidSongTitle = "Eloko Oyo";
                public const string ValidArtistName = "Fally Ipupa";
                public const string ValidLyricsText =
                    "Eloko oyo na lingi\nMpo na yo nde nazali\nSolola na ngai pamba te\nNa lingi yo koloba.";
                public const string ValidLanguage = "fr";
            }

            /// <summary>Constants for ArticleImage entity testing.</summary>
            public static class ArticleImage
            {
                public const string ValidStorageKey = "content/articles/test-image-key";
                public const string ValidUrl = "https://res.cloudinary.com/test/image/upload/v1/test-image.jpg";
                public const string AnotherStorageKey = "content/articles/another-image-key";
                public const string AnotherUrl = "https://res.cloudinary.com/test/image/upload/v1/another-image.jpg";
            }

            /// <summary>Cloudinary test upload result values.</summary>
            public static class Cloudinary
            {
                public const string ValidPublicId = "content/articles/uploaded-image";
                public const string ValidSecureUrl =
                    "https://res.cloudinary.com/test/image/upload/v1/uploaded-image.jpg";
                public const string ValidFormat = "jpg";
                public const int ValidWidth = 1200;
                public const int ValidHeight = 630;
                public const long ValidBytes = 102400L;
                public const string ValidResourceType = "image";
            }
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
