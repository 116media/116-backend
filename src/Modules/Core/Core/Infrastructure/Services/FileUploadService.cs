using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using _116.Core.Domain.Exceptions;
using _116.Core.Domain.StateMachines;
using Microsoft.AspNetCore.Http;

namespace _116.Core.Infrastructure.Services;

/// <summary>
/// Uploads assets to cloud storage and stages their file rows. Staging never commits: the
/// caller's transaction does, so the file and the row referencing it land together.
/// </summary>
/// <param name="fileRepository">Repository for file data access operations.</param>
/// <param name="fileService">Cloud storage gateway.</param>
/// <param name="imageColorService">Extracts an image's dominant and foreground colors.</param>
/// <param name="timeProvider">The clock the replacement stamp is read from.</param>
public class FileUploadService(
    IFileRepository fileRepository,
    IFileService fileService,
    IImageColorService imageColorService,
    TimeProvider timeProvider
) : IFileUploadService
{
    /// <inheritdoc />
    public async Task<FileEntity> UploadImageAsync(
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    )
    {
        FileUploadResult upload = await fileService.UploadFileAsync(
            file: file,
            publicId: publicId,
            folder: folder,
            cancellationToken: cancellationToken
        );

        // A null result leaves both colour columns unset rather than failing the upload.
        ImageColors? colors = await imageColorService.ExtractAsync(file, cancellationToken);

        return FileEntity.Create(
            id: upload.FileId,
            fileName: publicId,
            originalFileName: originalFileName,
            mimeType: mimeType,
            storageUrl: upload.SecureUrl,
            sizeInBytes: upload.Bytes,
            storageKey: upload.PublicId,
            dominantColorHex: colors?.DominantColorHex,
            foregroundColorHex: colors?.ForegroundColorHex
        );
    }

    /// <inheritdoc />
    public async Task<FileEntity> UploadVideoAsync(
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    )
    {
        FileUploadResult upload = await fileService.UploadVideoFileAsync(
            file: file,
            publicId: publicId,
            folder: folder,
            cancellationToken: cancellationToken
        );

        return Describe(upload, publicId, originalFileName, mimeType);
    }

    /// <inheritdoc />
    public async Task<FileEntity> UploadRawAsync(
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    )
    {
        FileUploadResult upload = await fileService.UploadRawFileAsync(
            file: file,
            publicId: publicId,
            folder: folder,
            cancellationToken: cancellationToken
        );

        return Describe(upload, publicId, originalFileName, mimeType);
    }

    /// <inheritdoc />
    public async Task<FileEntity> UploadAvatarAsync(
        IFormFile avatarFile,
        string userId,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    )
    {
        FileUploadResult upload = await fileService.UploadFileAsync(
            file: avatarFile,
            publicId: userId,
            folder: "avatars",
            cancellationToken: cancellationToken
        );

        return Describe(upload, userId, originalFileName, mimeType);
    }

    /// <inheritdoc />
    public async Task<FileEntity?> UploadAvatarFromUrlAsync(
        Guid? currentAvatarFileId,
        string avatarUrl,
        CancellationToken cancellationToken = default
    )
    {
        if (await HasAvatarFromAsync(currentAvatarFileId, avatarUrl, cancellationToken))
        {
            return null;
        }

        FileDownloadResult download = await fileService.DownloadFileAsync(avatarUrl, cancellationToken);

        return FileEntity.Create(
            id: download.FileId,
            fileName: download.FileName,
            originalFileName: download.OriginalFileName,
            mimeType: download.MimeType,
            storageUrl: download.StorageUrl,
            sizeInBytes: download.SizeInBytes
        );
    }

    /// <inheritdoc />
    public async Task<FileEntity> RecordAsync(
        FileEntity file,
        Guid? supersededFileId = null,
        CancellationToken cancellationToken = default
    )
    {
        if (file.IsRecorded)
        {
            throw new CoreRuleException(CoreRuleCodes.FileAlreadyRecorded);
        }

        if (supersededFileId.HasValue)
        {
            FileEntity? superseded = await fileRepository.GetByIdAsync(supersededFileId.Value, cancellationToken);
            superseded?.MarkReplaced(timeProvider.GetUtcNow().UtcDateTime);
        }

        await fileRepository.AddAsync(file, cancellationToken);

        return file;
    }

    /// <summary>
    /// Whether the avatar in use is already the one served from that URL.
    /// </summary>
    /// <param name="avatarFileId">The avatar in use, or null when there is none.</param>
    /// <param name="avatarUrl">The URL to compare against.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True when the avatar already comes from that URL.</returns>
    private async Task<bool> HasAvatarFromAsync(
        Guid? avatarFileId,
        string avatarUrl,
        CancellationToken cancellationToken
    )
    {
        if (avatarFileId is not Guid fileId)
        {
            return false;
        }

        FileEntity? avatar = await fileRepository.GetByIdAsync(fileId, cancellationToken);

        return string.Equals(avatar?.StorageUrl, avatarUrl, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Builds the unrecorded file for an upload result.
    /// </summary>
    /// <param name="upload">The storage result.</param>
    /// <param name="publicId">The storage public id, which is also the stored filename.</param>
    /// <param name="originalFileName">The filename as submitted.</param>
    /// <param name="mimeType">The uploaded file's MIME type.</param>
    /// <returns>The unrecorded file.</returns>
    private static FileEntity Describe(
        FileUploadResult upload,
        string publicId,
        string originalFileName,
        string mimeType
    )
    {
        return FileEntity.Create(
            id: upload.FileId,
            fileName: publicId,
            originalFileName: originalFileName,
            mimeType: mimeType,
            storageUrl: upload.SecureUrl,
            sizeInBytes: upload.Bytes,
            storageKey: upload.PublicId
        );
    }
}
