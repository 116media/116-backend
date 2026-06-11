using _116.Identity.Domain.Enums;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Session entity testing.
    /// Mirrors <c>src/BuildingBlocks/Constants/SessionConstants.cs</c>.
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
}
