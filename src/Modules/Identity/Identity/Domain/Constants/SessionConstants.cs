namespace _116.Identity.Domain.Constants;

/// <summary>
/// Contains constant values for session management.
/// Provides centralized constants for session status, device platforms, and device patterns.
/// </summary>
public static class SessionConstants
{
    /// <summary>
    /// Session status values for filtering and querying.
    /// </summary>
    public static class Status
    {
        /// <summary>
        /// Represents an active session (not deleted and not expired).
        /// </summary>
        public const string Active = "active";

        /// <summary>
        /// Represents an expired session (past expiration date).
        /// </summary>
        public const string Expired = "expired";
    }

    /// <summary>
    /// Client platform identifiers sent via Client-Platform header.
    /// </summary>
    public static class ClientPlatform
    {
        /// <summary>
        /// iOS mobile application.
        /// </summary>
        public const string IosMobile = "ios-mobile";

        /// <summary>
        /// Android mobile application.
        /// </summary>
        public const string AndroidMobile = "android-mobile";

        /// <summary>
        /// Web browser application.
        /// </summary>
        public const string BrowserWeb = "browser-web";

        /// <summary>
        /// Progressive Web App (PWA).
        /// </summary>
        public const string PwaBrowser = "pwa-browser";

        /// <summary>
        /// Unknown or missing client platform.
        /// </summary>
        public const string Unknown = "unknown";
    }

    /// <summary>
    /// Device platform categories for session analytics (classified from user agent).
    /// </summary>
    public static class Platform
    {
        /// <summary>
        /// Mobile device platform (smartphones).
        /// </summary>
        public const string Mobile = "mobile";

        /// <summary>
        /// Desktop/laptop platform.
        /// </summary>
        public const string Desktop = "desktop";

        /// <summary>
        /// Tablet device platform.
        /// </summary>
        public const string Tablet = "tablet";

        /// <summary>
        /// Unknown or other device platforms.
        /// </summary>
        public const string Other = "other";
    }

    /// <summary>
    /// Device name patterns for platform classification.
    /// Used to determine device platform from device name strings.
    /// </summary>
    public static class DevicePatterns
    {
        /// <summary>
        /// Mobile device name patterns.
        /// </summary>
        public static readonly string[] Mobile =
        [
            "iphone",
            "android",
            "mobile",
            "phone"
        ];

        /// <summary>
        /// Tablet device name patterns.
        /// </summary>
        public static readonly string[] Tablet =
        [
            "ipad",
            "tablet"
        ];

        /// <summary>
        /// Desktop/laptop device name patterns.
        /// </summary>
        public static readonly string[] Desktop =
        [
            "windows",
            "mac",
            "linux",
            "chrome",
            "firefox",
            "safari",
            "edge",
            "opera"
        ];
    }

    /// <summary>
    /// Export-related constants.
    /// </summary>
    public static class Export
    {
        /// <summary>
        /// Default base filename for session exports.
        /// </summary>
        public const string DefaultBaseFileName = "sessions-export";

        /// <summary>
        /// CSV Content-Type for session exports.
        /// </summary>
        public const string CsvContentType = "text/csv";

        /// <summary>
        /// Xlsx Content-Type for session exports.
        /// </summary>
        public const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    }
}
