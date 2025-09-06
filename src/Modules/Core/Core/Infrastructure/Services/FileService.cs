using _116.Core.Application.Shared.Services;
using _116.Core.Application.Shared.Errors;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;

namespace _116.Core.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="IFileService"/> for file operations including download, storage, and management.
/// Handles remote file metadata retrieval, persistence, and lifecycle operations.
/// </summary>
/// <param name="httpClient">HTTP client for downloading files from URLs.</param>
/// <param name="fileRepository">Repository for file entity persistence.</param>
public class FileService(HttpClient httpClient, IFileRepository fileRepository) : IFileService
{
    /// <inheritdoc />
    public async Task<Guid> DownloadAndStoreAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        ValidateFileUrl(fileUrl, out Uri? uri);

        try
        {
            var (contentType, contentLength) = await GetFileMetadataAsync(uri, cancellationToken);

            var fileId = Guid.NewGuid();
            if (uri?.LocalPath != null)
            {
                string extension = ResolveExtension(uri.LocalPath, contentType);
                string resolvedContentType = ResolveContentType(extension, contentType);

                FileEntity fileEntity = CreateFileEntity(
                    fileId,
                    uri,
                    fileUrl,
                    extension,
                    resolvedContentType,
                    contentLength
                );

                await fileRepository.AddAsync(fileEntity, cancellationToken);
            }

            await fileRepository.SaveChangesAsync(cancellationToken);

            return fileId;
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

    /// <inheritdoc />
    public async Task<string?> GetFilePathAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        FileEntity? fileEntity = await fileRepository.GetByIdAsync(fileId, cancellationToken);
        return fileEntity?.StorageUrl;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        FileEntity? fileEntity = await fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (fileEntity == null) return false;

        bool deleted = fileEntity.Delete();
        if (deleted)
        {
            await fileRepository.SaveChangesAsync(cancellationToken);
        }

        return deleted;
    }

    /// <inheritdoc />
    public async Task<string?> GetPublicUrlAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        FileEntity? fileEntity = await fileRepository.GetByIdAsync(fileId, cancellationToken);
        return fileEntity?.StorageUrl;
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
    /// Creates a new <see cref="FileEntity"/> for persistence.
    /// </summary>
    private static FileEntity CreateFileEntity(
        Guid fileId,
        Uri? uri,
        string fileUrl,
        string extension,
        string contentType,
        long contentLength
    )
    {
        string fileName = $"{fileId}{extension}";
        string originalFileName = Path.GetFileName(uri!.LocalPath);

        if (string.IsNullOrEmpty(originalFileName))
        {
            originalFileName = $"avatar{extension}";
        }

        return FileEntity.Create(
            id: fileId,
            fileName: fileName,
            originalFileName: originalFileName,
            mimeType: contentType,
            storageUrl: fileUrl,
            sizeInBytes: contentLength
        );
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
