using System.Net.Http.Headers;
using _116.Core.Application.Shared.Errors.Facade;
using _116.Core.Application.Shared.Services;
using _116.Shared.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace _116.Core.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="IFileService"/> for file operations including download, storage, and management.
/// Handles remote file metadata retrieval and cloud storage operations.
/// </summary>
/// <param name="httpClient">HTTP client for downloading files from URLs.</param>
/// <param name="cloudinaryService">Service for Cloudinary cloud storage operations.</param>
/// <param name="i18n">The Core i18n facade for localized errors.</param>
/// <param name="urlSafetyGuard">Guard that rejects SSRF-prone URLs before they are fetched.</param>
/// <param name="logger">Logger for download diagnostics.</param>
public class FileService(
    HttpClient httpClient,
    ICloudinaryService cloudinaryService,
    CoreI18n i18n,
    IUrlSafetyGuard urlSafetyGuard,
    ILogger<FileService> logger
) : IFileService
{
    /// <inheritdoc />
    public async Task<FileUploadResult> UploadFileAsync(
        IFormFile file,
        string publicId,
        string? folder = null,
        CancellationToken cancellationToken = default
    )
    {
        // Upload to Cloudinary (with overwrite enabled)
        CloudinaryUploadResult uploadResult = await cloudinaryService.UploadImageAsync(
            file,
            publicId,
            folder,
            cancellationToken
        );

        // Generate file ID and return result
        var fileId = Guid.NewGuid();

        return new FileUploadResult(
            FileId: fileId,
            SecureUrl: uploadResult.SecureUrl,
            Format: uploadResult.Format,
            Width: uploadResult.Width,
            Height: uploadResult.Height,
            Bytes: uploadResult.Bytes,
            PublicId: uploadResult.PublicId
        );
    }

    /// <inheritdoc />
    public async Task<FileUploadResult> UploadRawFileAsync(
        IFormFile file,
        string publicId,
        string? folder = null,
        CancellationToken cancellationToken = default
    )
    {
        CloudinaryUploadResult uploadResult = await cloudinaryService.UploadRawAsync(
            file,
            publicId,
            folder,
            cancellationToken
        );

        var fileId = Guid.NewGuid();

        return new FileUploadResult(
            FileId: fileId,
            SecureUrl: uploadResult.SecureUrl,
            Format: uploadResult.Format,
            Width: uploadResult.Width,
            Height: uploadResult.Height,
            Bytes: uploadResult.Bytes,
            PublicId: uploadResult.PublicId
        );
    }

    /// <inheritdoc />
    public async Task<FileUploadResult> UploadVideoFileAsync(
        IFormFile file,
        string publicId,
        string? folder = null,
        CancellationToken cancellationToken = default
    )
    {
        CloudinaryUploadResult uploadResult = await cloudinaryService.UploadVideoAsync(
            file,
            publicId,
            folder,
            cancellationToken
        );

        var fileId = Guid.NewGuid();

        return new FileUploadResult(
            FileId: fileId,
            SecureUrl: uploadResult.SecureUrl,
            Format: uploadResult.Format,
            Width: uploadResult.Width,
            Height: uploadResult.Height,
            Bytes: uploadResult.Bytes,
            PublicId: uploadResult.PublicId
        );
    }

    /// <inheritdoc />
    public async Task<bool> DeleteFileAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        return await cloudinaryService.DeleteImageAsync(storageKey, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FileDownloadResult> DownloadFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default
    )
    {
        ValidateFileUrl(fileUrl, out Uri? uri);

        try
        {
            var (contentType, contentLength) = await GetFileMetadataAsync(uri!, cancellationToken);

            string localPath = uri?.LocalPath ?? string.Empty;
            string extension = ResolveExtension(localPath, contentType);
            string resolvedContentType = ResolveContentType(extension, contentType);

            var fileId = Guid.NewGuid();
            string fileName = $"{fileId}{extension}";
            string originalFileName = Path.GetFileName(localPath);

            return new FileDownloadResult(
                FileId: fileId,
                FileName: fileName,
                OriginalFileName: originalFileName,
                MimeType: resolvedContentType,
                StorageUrl: fileUrl,
                SizeInBytes: contentLength
            );
        }
        catch (HttpRequestException ex)
        {
            // Log the detail for diagnostics; never reflect provider text back to the client.
            logger.LogWarning(ex, "Remote file download failed");
            throw i18n.File.FileDownloadFailed();
        }
        catch (Exception ex)
            when (ex is not ArgumentException and not BadRequestException and not InternalServerException)
        {
            logger.LogWarning(ex, "Remote file download failed unexpectedly");
            throw i18n.File.FileDownloadFailed();
        }
    }

    /// <summary>
    /// Validates the provided file URL and parses it into a <see cref="Uri"/>.
    /// </summary>
    private void ValidateFileUrl(string fileUrl, out Uri? uri)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            throw i18n.File.FileUrlRequired();
        }

        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out uri))
        {
            throw i18n.File.InvalidFileUrl(fileUrl);
        }
    }

    /// <summary>
    /// Retrieves metadata (content type and size) for the specified file.
    /// </summary>
    private async Task<(string? ContentType, long ContentLength)> GetFileMetadataAsync(
        Uri uri,
        CancellationToken cancellationToken
    )
    {
        using HttpResponseMessage headResponse = await SendGuardedAsync(
            HttpMethod.Head,
            uri,
            range: null,
            HttpCompletionOption.ResponseContentRead,
            cancellationToken
        );
        headResponse.EnsureSuccessStatusCode();

        string? contentType = headResponse.Content.Headers.ContentType?.MediaType;
        long contentLength = headResponse.Content.Headers.ContentLength ?? 0;

        if (contentLength == 0)
        {
            contentLength =
                await TryGetContentLengthWithRangeAsync(uri, cancellationToken)
                ?? await TryGetContentLengthFallbackAsync(uri, cancellationToken)
                ?? 0;
        }

        return (contentType, contentLength);
    }

    /// <summary>
    /// Attempts to determine content length using a range request.
    /// </summary>
    private async Task<long?> TryGetContentLengthWithRangeAsync(Uri uri, CancellationToken cancellationToken)
    {
        using HttpResponseMessage partialResponse = await SendGuardedAsync(
            HttpMethod.Get,
            uri,
            new RangeHeaderValue(0, 0),
            HttpCompletionOption.ResponseContentRead,
            cancellationToken
        );
        return partialResponse.Content.Headers.ContentRange?.Length;
    }

    /// <summary>
    /// Attempts to determine content length by downloading headers only.
    /// </summary>
    private async Task<long?> TryGetContentLengthFallbackAsync(Uri uri, CancellationToken cancellationToken)
    {
        using HttpResponseMessage sampleResponse = await SendGuardedAsync(
            HttpMethod.Get,
            uri,
            range: null,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );
        return sampleResponse.Content.Headers.ContentLength;
    }

    /// <summary>
    /// Sends a request, re-validating every hop against the SSRF guard. Auto-redirect is disabled on
    /// the client, so redirects are followed manually here — each target is guarded before it is
    /// dialled, and the chain is capped to stop redirect loops.
    /// </summary>
    /// <param name="method">The HTTP method to send.</param>
    /// <param name="uri">The initial URL to fetch.</param>
    /// <param name="range">Optional byte range to request; null for the full response.</param>
    /// <param name="completion">When the returned task completes (headers-read or content-read).</param>
    /// <param name="cancellationToken">Token to cancel the send.</param>
    /// <returns>The first non-redirect response, with all hops verified safe.</returns>
    private async Task<HttpResponseMessage> SendGuardedAsync(
        HttpMethod method,
        Uri uri,
        RangeHeaderValue? range,
        HttpCompletionOption completion,
        CancellationToken cancellationToken
    )
    {
        const int maxHops = 5;
        Uri current = uri;

        for (var hop = 0; hop < maxHops; hop++)
        {
            await urlSafetyGuard.EnsureSafeAsync(current, cancellationToken);

            using var request = new HttpRequestMessage(method, current);
            if (range is not null)
            {
                request.Headers.Range = range;
            }

            HttpResponseMessage response = await httpClient.SendAsync(request, completion, cancellationToken);

            if ((int)response.StatusCode is < 300 or >= 400)
            {
                return response;
            }

            Uri? location = response.Headers.Location;
            response.Dispose();

            if (location is null)
            {
                throw i18n.File.FileDownloadFailed();
            }

            current = location.IsAbsoluteUri ? location : new Uri(current, location);
        }

        throw i18n.File.FileDownloadFailed();
    }

    /// <summary>
    /// Resolves the file extension from either the local path or content type.
    /// </summary>
    private static string ResolveExtension(string localPath, string? contentType)
    {
        string extension = Path.GetExtension(localPath);
        return string.IsNullOrEmpty(extension) ? GetExtensionFromContentType(contentType) : extension;
    }

    /// <summary>
    /// Resolves the MIME type from either the extension or the provided content type.
    /// </summary>
    private static string ResolveContentType(string extension, string? contentType)
    {
        return string.IsNullOrEmpty(contentType) ? GetContentTypeFromExtension(extension) : contentType;
    }

    /// <summary>
    /// Gets the file extension from the specified content type.
    /// </summary>
    private static string GetExtensionFromContentType(string? contentType) =>
        contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            _ => ".bin",
        };

    /// <summary>
    /// Gets the MIME content type from the specified file extension.
    /// </summary>
    private static string GetContentTypeFromExtension(string? extension) =>
        extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream",
        };
}
