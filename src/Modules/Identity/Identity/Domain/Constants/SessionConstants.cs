namespace _116.Identity.Domain.Constants;

/// <summary>
/// Contains constant values for session management.
/// Provides centralized constants for device patterns and export configuration.
/// </summary>
public static class SessionConstants
{
    /// <summary>
    /// Device name patterns for platform classification.
    /// Used to determine the device platform from device name strings.
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
