namespace _116.Core.Application.Shared.Errors.Messages;

/// <summary>
/// Provides internal server error messages for the <c>Core</c> domain.
/// These messages describe system-level failures and service unavailability.
/// </summary>
public static class InternalServerErrorMessage
{
    /// <summary>
    /// Gets the error message for service unavailable.
    /// </summary>
    /// <param name="serviceName">The name of the unavailable service.</param>
    /// <returns>A formatted error message indicating the service is unavailable.</returns>
    public static string ServiceUnavailable(string serviceName)
    {
        return $"Service '{serviceName}' is currently unavailable";
    }

    /// <summary>
    /// Error message for database connection failure.
    /// </summary>
    /// <returns>A formatted error message indicating database connection failed.</returns>
    public static string DatabaseConnectionFailed()
    {
        return "Unable to connect to the database";
    }

    /// <summary>
    /// Gets the error message for file download failure.
    /// </summary>
    /// <param name="fileUrl">The URL that failed to download.</param>
    /// <param name="reason">The specific reason for the failure.</param>
    /// <returns>A formatted error message indicating the file download failed.</returns>
    public static string FileDownloadFailed(string fileUrl, string reason)
    {
        return $"Failed to download file from '{fileUrl}': {reason}";
    }

    /// <summary>
    /// Gets the error message for file storage failure.
    /// </summary>
    /// <param name="reason">The specific reason for the storage failure.</param>
    /// <returns>A formatted error message indicating the file storage failed.</returns>
    public static string FileStorageFailed(string reason)
    {
        return $"Failed to store file: {reason}";
    }
}
