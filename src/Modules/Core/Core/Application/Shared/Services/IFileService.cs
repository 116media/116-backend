namespace _116.Core.Application.Shared.Services;

/// <summary>
/// Service interface for file operations including download, storage, and management.
/// </summary>
/// <remarks>
/// This service handles file operations such as downloading files from URLs,
/// storing them locally, and managing file metadata.
/// </remarks>
public interface IFileService
{
    /// <summary>
    /// Downloads a file from the specified URL and stores it locally.
    /// </summary>
    /// <param name="fileUrl">The URL of the file to download.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The unique identifier of the stored file.</returns>
    /// <exception cref="ArgumentException">Thrown when the URL is invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when the file cannot be downloaded.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be stored.</exception>
    /// <remarks>
    /// This method downloads files from external sources (like social provider avatars)
    /// and stores them in the local file system with a unique identifier.
    /// The returned GUID can be used to reference the file in the database.
    /// </remarks>
    Task<Guid> DownloadAndStoreAsync(string fileUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the local file path for a stored file by its identifier.
    /// </summary>
    /// <param name="fileId">The unique identifier of the file.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The local file path if the file exists, otherwise null.</returns>
    /// <remarks>
    /// This method retrieves the local file path for a previously stored file.
    /// Returns null if the file with the specified ID doesn't exist.
    /// </remarks>
    Task<string?> GetFilePathAsync(Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a stored file by its identifier.
    /// </summary>
    /// <param name="fileId">The unique identifier of the file to delete.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True if the file was successfully deleted, false if it didn't exist.</returns>
    /// <remarks>
    /// This method removes the file from local storage and any associated metadata.
    /// </remarks>
    Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the public URL for accessing a stored file.
    /// </summary>
    /// <param name="fileId">The unique identifier of the file.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The public URL for the file if it exists, otherwise null.</returns>
    /// <remarks>
    /// This method returns a URL that can be used to access the file via HTTP.
    /// Useful for serving avatar images and other user-uploaded content.
    /// </remarks>
    Task<string?> GetPublicUrlAsync(Guid fileId, CancellationToken cancellationToken = default);
}
