using _116.BuildingBlocks.Constants;
using _116.Identity.Domain.Enums;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Session entity testing. The column length limits alias
    /// <see cref="SessionConstants" /> rather than copying it; the rest is test fixture data.
    /// </summary>
    public static class Session
    {
        /// <summary>
        /// The production maximum device identifier length.
        /// </summary>
        public const int DeviceIdMaxLength = SessionConstants.MaxDeviceIdLength;

        /// <summary>
        /// The production maximum IP address length, sized for IPv6.
        /// </summary>
        public const int IpAddressMaxLength = SessionConstants.MaxIpAddressLength;

        /// <summary>
        /// The production maximum user agent length, distinct from
        /// <see cref="SessionConstants.MaxRefreshTokenHashLength" />.
        /// </summary>
        public const int UserAgentMaxLength = SessionConstants.MaxUserAgentLength;

        /// <summary>
        /// Test-owned fixture device identifier within the production length bound.
        /// </summary>
        public const string ValidDeviceId = "device-abc123-xyz789";

        /// <summary>
        /// Test-owned fixture IPv4 address.
        /// </summary>
        public const string ValidIpAddress = "192.168.1.100";

        /// <summary>
        /// Test-owned fixture user agent string.
        /// </summary>
        public const string ValidUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

        /// <summary>
        /// Test-owned. Production reads <c>JWT_ACCESS_TOKEN_EXPIRATION</c> at startup,
        /// so this value binds nothing in <c>src/</c>.
        /// </summary>
        public const int DefaultAccessTokenExpirationMinutes = 60;

        /// <summary>
        /// Test-owned. Production reads <c>JWT_REFRESH_TOKEN_EXPIRATION</c> at startup,
        /// so this value binds nothing in <c>src/</c>.
        /// </summary>
        public const int DefaultRefreshTokenExpirationDays = 30;

        /// <summary>
        /// Test-owned. Production reads <c>JWT_SESSION_ABSOLUTE_LIFETIME_IN_DAYS</c> at startup,
        /// so this value binds nothing in <c>src/</c>.
        /// </summary>
        public const int DefaultAbsoluteLifetimeDays = 30;

        /// <summary>
        /// Test-owned loopback address used when a test does not care about the origin IP.
        /// </summary>
        public const string DefaultIpAddress = "127.0.0.1";

        /// <summary>
        /// Test-owned user agent used when a test does not care about the client string.
        /// </summary>
        public const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

        /// <summary>
        /// Test-owned browser the session builder assigns by default.
        /// </summary>
        public static readonly EnumBrowser DefaultBrowser = EnumBrowser.Chrome;

        /// <summary>
        /// Test-owned device type the session builder assigns by default.
        /// </summary>
        public static readonly EnumDevice DefaultDevice = EnumDevice.Desktop;

        /// <summary>
        /// Test-owned platform the session builder assigns by default.
        /// </summary>
        public static readonly EnumPlatform DefaultPlatform = EnumPlatform.Windows;

        /// <summary>
        /// Test-owned client the session builder assigns by default.
        /// </summary>
        public static readonly EnumClient DefaultClient = EnumClient.WebApp;

        /// <summary>
        /// Test-owned device identifier the session builder assigns by default.
        /// </summary>
        public const string DefaultDeviceId = "test-device-id-12345";

        /// <summary>
        /// Test-owned placeholder refresh token; production mints these at runtime.
        /// </summary>
        public const string DefaultRefreshToken = "default-refresh-token-for-testing-base64encoded";

        /// <summary>
        /// Test-owned placeholder refresh token hash, not the hash of
        /// <see cref="DefaultRefreshToken" />.
        /// </summary>
        public const string DefaultRefreshTokenHash = "hashed_default_refresh_token_sha256";
    }
}
