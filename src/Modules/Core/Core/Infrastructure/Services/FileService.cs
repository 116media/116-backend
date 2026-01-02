using _116.Core.Application.Shared.Errors;
using _116.Core.Application.Shared.Services;
using _116.Shared.Application.Exceptions;

using Microsoft.AspNetCore.Http;

namespace _116.Core.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="IFileService"/> for file operations including download, storage, and management.
/// Handles remote file metadata retrieval and cloud storage operations.
/// </summary>
/// <param name="httpClient">HTTP client for downloading files from URLs.</param>
/// <param name="cloudinaryService">Service for Cloudinary cloud storage operations.</param>
public class FileService(HttpClient httpClient, ICloudinaryService cloudinaryService) : IFileService
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
            Bytes: uploadResult.Bytes
        );
    }

    /// <inheritdoc />
    public async Task<FileDownloadResult> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        ValidateFileUrl(fileUrl, out Uri? uri);

        try
        {
            var (contentType, contentLength) = await GetFileMetadataAsync(uri, cancellationToken);

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
            throw CoreErrors.FileDownloadFailed(fileUrl, ex.Message);
        }
        catch (Exception ex) when (ex is not ArgumentException and not BadRequestException)
        {
            throw CoreErrors.FileStorageFailed(ex.Message);
        }
    }

    /// <summary>
    /// Validates the provided file URL and parses it into a <see cref="Uri"/>.
    /// </summary>
    private static void ValidateFileUrl(string fileUrl, out Uri? uri)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            throw CoreErrors.FileUrlRequired();
        }

        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out uri))
        {
            throw CoreErrors.InvalidFileUrl(fileUrl);
        }
    }

    /// <summary>
    /// Retrieves metadata (content type and size) for the specified file.
    /// </summary>
    private async Task<(string? ContentType, long ContentLength)> GetFileMetadataAsync(
        Uri? uri,
        CancellationToken cancellationToken
    )
    {
        using var headRequest = new HttpRequestMessage(HttpMethod.Head, uri);
        using HttpResponseMessage headResponse = await httpClient.SendAsync(headRequest, cancellationToken);
        headResponse.EnsureSuccessStatusCode();

        string? contentType = headResponse.Content.Headers.ContentType?.MediaType;
        long contentLength = headResponse.Content.Headers.ContentLength ?? 0;

        if (contentLength == 0)
        {
            contentLength = await TryGetContentLengthWithRangeAsync(uri, cancellationToken)
                         ?? await TryGetContentLengthFallbackAsync(uri, cancellationToken)
                         ?? 0;
        }

        return (contentType, contentLength);
    }

    /// <summary>
    /// Attempts to determine content length using a range request.
    /// </summary>
    private async Task<long?> TryGetContentLengthWithRangeAsync(Uri? uri, CancellationToken cancellationToken)
    {
        using var partialRequest = new HttpRequestMessage(HttpMethod.Get, uri);
        partialRequest.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 0);

        using HttpResponseMessage partialResponse = await httpClient.SendAsync(partialRequest, cancellationToken);
        return partialResponse.Content.Headers.ContentRange?.Length;
    }

    /// <summary>
    /// Attempts to determine content length by downloading headers only.
    /// </summary>
    private async Task<long?> TryGetContentLengthFallbackAsync(Uri? uri, CancellationToken cancellationToken)
    {
        var sampleRequest = new HttpRequestMessage(HttpMethod.Get, uri);
        HttpResponseMessage sampleResponse = await httpClient.SendAsync(
            sampleRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );
        return sampleResponse.Content.Headers.ContentLength;
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
    private static string GetExtensionFromContentType(string? contentType) => contentType switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/gif" => ".gif",
        "image/webp" => ".webp",
        _ => ".bin"
    };

    /// <summary>
    /// Gets the MIME content type from the specified file extension.
    /// </summary>
    private static string GetContentTypeFromExtension(string? extension) => extension switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        _ => "application/octet-stream"
    };
}
