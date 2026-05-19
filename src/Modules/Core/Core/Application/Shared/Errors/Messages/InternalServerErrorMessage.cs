using Microsoft.Extensions.Localization;

namespace _116.Core.Application.Shared.Errors.Messages;

/// <summary>
/// Provides internal server error messages for the <c>Core</c> domain.
/// These messages describe system-level failures and service unavailability.
/// </summary>
public class InternalServerErrorMessage(IStringLocalizer<InternalServerErrorMessage> localizer)
{
    /// <summary>
    /// Gets the error message for service unavailable.
    /// </summary>
    /// <param name="serviceName">The name of the unavailable service.</param>
    /// <returns>A formatted error message indicating the service is unavailable.</returns>
    public string ServiceUnavailable(string serviceName)
    {
        return string.Format(localizer["ServiceUnavailable"], serviceName);
    }

    /// <summary>
    /// Error message for database connection failure.
    /// </summary>
    /// <returns>A formatted error message indicating database connection failed.</returns>
    public string DatabaseConnectionFailed()
    {
        return localizer["DatabaseConnectionFailed"];
    }

    /// <summary>
    /// Gets the error message for file download failure.
    /// </summary>
    /// <param name="fileUrl">The URL that failed to download.</param>
    /// <param name="reason">The specific reason for the failure.</param>
    /// <returns>A formatted error message indicating the file download failed.</returns>
    public string FileDownloadFailed(string fileUrl, string reason)
    {
        return string.Format(localizer["FileDownloadFailed"], fileUrl, reason);
    }

    /// <summary>
    /// Gets the error message for file storage failure.
    /// </summary>
    /// <param name="reason">The specific reason for the storage failure.</param>
    /// <returns>A formatted error message indicating the file storage failed.</returns>
    public string FileStorageFailed(string reason)
    {
        return string.Format(localizer["FileStorageFailed"], reason);
    }
}
