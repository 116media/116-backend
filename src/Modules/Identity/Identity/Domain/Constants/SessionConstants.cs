namespace _116.Identity.Domain.Constants;

/// <summary>
/// Contains constant values for session management.
/// Provides centralized constants for export configuration.
/// </summary>
public static class SessionConstants
{
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
