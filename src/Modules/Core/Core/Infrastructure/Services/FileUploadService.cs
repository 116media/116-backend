using _116.Core.Application.Shared.Persistence;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace _116.Core.Infrastructure.Services;

/// <summary>
/// Uploads assets to cloud storage and records them through the file repository, committing
/// each step with <see cref="ICoreUnitOfWork" />.
/// </summary>
/// <param name="fileRepository">Repository for file data access operations.</param>
/// <param name="fileService">Cloud storage gateway.</param>
/// <param name="imageColorService">Extracts an image's dominant and foreground colors.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="timeProvider">The clock the replacement stamp is read from.</param>
public class FileUploadService(
    IFileRepository fileRepository,
    IFileService fileService,
    IImageColorService imageColorService,
    ICoreUnitOfWork unitOfWork,
    TimeProvider timeProvider
) : IFileUploadService
{
    /// <inheritdoc />
    public async Task<FileEntity> UpdateAvatarFromFileAsync(
        Guid? currentAvatarFileId,
        IFormFile avatarFile,
        string userId,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    )
    {
        if (currentAvatarFileId.HasValue)
        {
            await MarkReplacedAsync(currentAvatarFileId.Value, cancellationToken);
        }

        return await UploadAndStoreAvatarAsync(
            avatarFile: avatarFile,
            userId: userId,
            originalFileName: originalFileName,
            mimeType: mimeType,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<FileEntity?> UpdateAvatarFromUrlAsync(
        Guid? currentAvatarFileId,
        string newAvatarUrl,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        if (currentAvatarFileId.HasValue)
        {
            FileEntity? existingFile = await fileRepository.GetByIdAsync(currentAvatarFileId.Value, cancellationToken);
            if (
                existingFile is not null
                && string.Equals(existingFile.StorageUrl, newAvatarUrl, StringComparison.OrdinalIgnoreCase)
            )
            {
                return null;
            }

            await MarkReplacedAsync(currentAvatarFileId.Value, cancellationToken);
        }

        return await DownloadAndStoreAvatarFromUrlAsync(
            avatarUrl: newAvatarUrl,
            userId: userId,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<FileEntity> ReplaceImageFileAsync(
        Guid? currentFileId,
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    )
    {
        if (currentFileId.HasValue)
        {
            await MarkReplacedAsync(currentFileId.Value, cancellationToken);
        }

        return await UploadAndStoreImageFileAsync(
            file: file,
            publicId: publicId,
            folder: folder,
            originalFileName: originalFileName,
            mimeType: mimeType,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<FileEntity> ReplaceVideoFileAsync(
        Guid? currentFileId,
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    )
    {
        if (currentFileId.HasValue)
        {
            await MarkReplacedAsync(currentFileId.Value, cancellationToken);
        }

        return await UploadAndStoreVideoFileAsync(
            file: file,
            publicId: publicId,
            folder: folder,
            originalFileName: originalFileName,
            mimeType: mimeType,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<FileEntity> UploadAndStoreRawFileAsync(
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    )
    {
        FileUploadResult uploadResult = await fileService.UploadRawFileAsync(
            file: file,
            publicId: publicId,
            folder: folder,
            cancellationToken: cancellationToken
        );

        return await StoreAsync(
            FileEntity.Create(
                id: uploadResult.FileId,
                fileName: publicId,
                originalFileName: originalFileName,
                mimeType: mimeType,
                storageUrl: uploadResult.SecureUrl,
                sizeInBytes: uploadResult.Bytes,
                storageKey: uploadResult.PublicId
            ),
            cancellationToken
        );
    }

    /// <summary>
    /// Uploads an avatar and records it.
    /// </summary>
    /// <param name="avatarFile">The file to upload.</param>
    /// <param name="userId">The owner, used as the storage public id.</param>
    /// <param name="originalFileName">The filename as submitted.</param>
    /// <param name="mimeType">The uploaded file's MIME type.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The stored file.</returns>
    private async Task<FileEntity> UploadAndStoreAvatarAsync(
        IFormFile avatarFile,
        string userId,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken
    )
    {
        FileUploadResult uploadResult = await fileService.UploadFileAsync(
            file: avatarFile,
            publicId: userId,
            folder: "avatars",
            cancellationToken: cancellationToken
        );

        return await StoreAsync(
            FileEntity.Create(
                id: uploadResult.FileId,
                fileName: userId,
                originalFileName: originalFileName,
                mimeType: mimeType,
                storageUrl: uploadResult.SecureUrl,
                sizeInBytes: uploadResult.Bytes,
                storageKey: uploadResult.PublicId
            ),
            cancellationToken
        );
    }

    /// <summary>
    /// Fetches an avatar from a URL and records it.
    /// </summary>
    /// <param name="avatarUrl">The URL to fetch from.</param>
    /// <param name="userId">The owner the avatar belongs to.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The stored file.</returns>
    private async Task<FileEntity> DownloadAndStoreAvatarFromUrlAsync(
        string avatarUrl,
        string userId,
        CancellationToken cancellationToken
    )
    {
        FileDownloadResult downloadResult = await fileService.DownloadFileAsync(avatarUrl, cancellationToken);

        return await StoreAsync(
            FileEntity.Create(
                id: downloadResult.FileId,
                fileName: downloadResult.FileName,
                originalFileName: downloadResult.OriginalFileName,
                mimeType: downloadResult.MimeType,
                storageUrl: downloadResult.StorageUrl,
                sizeInBytes: downloadResult.SizeInBytes
            ),
            cancellationToken
        );
    }

    /// <summary>
    /// Uploads an image and records it, carrying the colors extracted from its bytes.
    /// </summary>
    /// <param name="file">The image to upload.</param>
    /// <param name="publicId">The storage public id.</param>
    /// <param name="folder">The destination storage folder.</param>
    /// <param name="originalFileName">The filename as submitted.</param>
    /// <param name="mimeType">The uploaded file's MIME type.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The stored file.</returns>
    private async Task<FileEntity> UploadAndStoreImageFileAsync(
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken
    )
    {
        FileUploadResult uploadResult = await fileService.UploadFileAsync(
            file: file,
            publicId: publicId,
            folder: folder,
            cancellationToken: cancellationToken
        );

        // A null result leaves both color columns unset rather than failing the upload.
        ImageColors? colors = await imageColorService.ExtractAsync(file, cancellationToken);

        return await StoreAsync(
            FileEntity.Create(
                id: uploadResult.FileId,
                fileName: publicId,
                originalFileName: originalFileName,
                mimeType: mimeType,
                storageUrl: uploadResult.SecureUrl,
                sizeInBytes: uploadResult.Bytes,
                storageKey: uploadResult.PublicId,
                dominantColorHex: colors?.DominantColorHex,
                foregroundColorHex: colors?.ForegroundColorHex
            ),
            cancellationToken
        );
    }

    /// <summary>
    /// Uploads a video and records it.
    /// </summary>
    /// <param name="file">The video to upload.</param>
    /// <param name="publicId">The storage public id.</param>
    /// <param name="folder">The destination storage folder.</param>
    /// <param name="originalFileName">The filename as submitted.</param>
    /// <param name="mimeType">The uploaded file's MIME type.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The stored file.</returns>
    private async Task<FileEntity> UploadAndStoreVideoFileAsync(
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken
    )
    {
        FileUploadResult uploadResult = await fileService.UploadVideoFileAsync(
            file: file,
            publicId: publicId,
            folder: folder,
            cancellationToken: cancellationToken
        );

        return await StoreAsync(
            FileEntity.Create(
                id: uploadResult.FileId,
                fileName: publicId,
                originalFileName: originalFileName,
                mimeType: mimeType,
                storageUrl: uploadResult.SecureUrl,
                sizeInBytes: uploadResult.Bytes,
                storageKey: uploadResult.PublicId
            ),
            cancellationToken
        );
    }

    /// <summary>
    /// Stages a newly uploaded file and commits it.
    /// </summary>
    /// <param name="file">The file to record.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The stored file.</returns>
    private async Task<FileEntity> StoreAsync(FileEntity file, CancellationToken cancellationToken)
    {
        await fileRepository.AddAsync(file, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return file;
    }

    /// <summary>
    /// Marks the superseded file replaced and commits before the new upload lands, so the
    /// replacement fact dispatches carrying the old storage key.
    /// </summary>
    /// <param name="fileId">The file being replaced.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True when the row transitioned; false when missing or already deleted.</returns>
    private async Task<bool> MarkReplacedAsync(Guid fileId, CancellationToken cancellationToken)
    {
        FileEntity? file = await fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file is null || !file.MarkReplaced(timeProvider.GetUtcNow().UtcDateTime))
        {
            return false;
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
