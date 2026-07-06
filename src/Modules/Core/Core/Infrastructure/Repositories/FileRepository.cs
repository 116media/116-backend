using _116.Core.Application.Shared.Errors.Facade;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Application.Shared.Specifications;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace _116.Core.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IFileRepository"/> using Entity Framework Core.
/// </summary>
public class FileRepository(
    CoreDbContext context,
    IFileService fileService,
    IImageColorService imageColorService,
    CoreI18n i18n
) : IFileRepository
{
    /// <inheritdoc />
    public async Task<FileEntity?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var specification = new FileByIdNotDeletedSpecification(fileId);

        return await context.Files.FirstOrDefaultBySpecificationAsync(specification, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, FileEntity>> GetByIdsAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken = default
    )
    {
        if (fileIds.Count == 0)
        {
            return new Dictionary<Guid, FileEntity>();
        }

        return await context
            .Files.Where(file => fileIds.Contains(file.Id) && !file.IsDeleted)
            .ToDictionaryAsync(file => file.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, string>> GetStorageUrlsByIdsAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken = default
    )
    {
        if (fileIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        Guid[] distinctIds = fileIds.Distinct().ToArray();

        return await context
            .Files.Where(file => distinctIds.Contains(file.Id) && !file.IsDeleted)
            .ToDictionaryAsync(file => file.Id, file => file.StorageUrl, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(FileEntity file, CancellationToken cancellationToken = default)
    {
        await context.Files.AddAsync(file, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(FileEntity file, CancellationToken cancellationToken = default)
    {
        context.Files.Update(file);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Remove(FileEntity file)
    {
        context.Files.Remove(file);
    }

    /// <inheritdoc />
    public async Task<FileEntity?> GetAvatarFileAsync(Guid? avatarFileId, CancellationToken cancellationToken = default)
    {
        return avatarFileId.HasValue ? await GetByIdAsync(avatarFileId.Value, cancellationToken) : null;
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FileEntity> UploadAndStoreAvatarAsync(
        IFormFile avatarFile,
        string userId,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    )
    {
        // Upload to Cloudinary via FileService
        FileUploadResult uploadResult = await fileService.UploadFileAsync(
            file: avatarFile,
            publicId: userId,
            folder: "avatars",
            cancellationToken: cancellationToken
        );

        // Create file entity with Cloudinary metadata
        var fileEntity = FileEntity.Create(
            id: uploadResult.FileId,
            fileName: userId,
            originalFileName: originalFileName,
            mimeType: mimeType,
            storageUrl: uploadResult.SecureUrl,
            sizeInBytes: uploadResult.Bytes,
            i18n: i18n,
            storageKey: uploadResult.PublicId
        );

        // Persist to the Database
        await AddAsync(fileEntity, cancellationToken);
        await SaveChangesAsync(cancellationToken);

        return fileEntity;
    }

    /// <inheritdoc />
    public async Task<FileEntity> DownloadAndStoreAvatarFromUrlAsync(
        string avatarUrl,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        // Download file metadata from URL via FileService
        FileDownloadResult downloadResult = await fileService.DownloadFileAsync(avatarUrl, cancellationToken);

        // Create file entity with downloaded file metadata
        var fileEntity = FileEntity.Create(
            id: downloadResult.FileId,
            fileName: downloadResult.FileName,
            originalFileName: downloadResult.OriginalFileName,
            mimeType: downloadResult.MimeType,
            storageUrl: downloadResult.StorageUrl,
            sizeInBytes: downloadResult.SizeInBytes,
            i18n: i18n
        );

        // Persist to Database
        await AddAsync(fileEntity, cancellationToken);
        await SaveChangesAsync(cancellationToken);

        return fileEntity;
    }

    /// <inheritdoc />
    public async Task<FileEntity?> UpdateAvatarFromUrlAsync(
        Guid? currentAvatarFileId,
        string newAvatarUrl,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        // Check if user already has avatar with same URL
        bool shouldUpdateAvatar = true;
        if (currentAvatarFileId.HasValue)
        {
            FileEntity? existingFile = await GetByIdAsync(currentAvatarFileId.Value, cancellationToken);
            if (
                existingFile != null
                && string.Equals(existingFile.StorageUrl, newAvatarUrl, StringComparison.OrdinalIgnoreCase)
            )
            {
                shouldUpdateAvatar = false;
            }
        }

        if (!shouldUpdateAvatar)
        {
            return null;
        }

        // Delete old avatar file entity if it exists
        if (currentAvatarFileId.HasValue)
        {
            FileEntity? oldFile = await GetByIdAsync(currentAvatarFileId.Value, cancellationToken);
            if (oldFile != null)
            {
                Remove(oldFile);
                await SaveChangesAsync(cancellationToken);
            }
        }

        // Download and store new avatar from URL
        FileEntity fileEntity = await DownloadAndStoreAvatarFromUrlAsync(
            avatarUrl: newAvatarUrl,
            userId: userId,
            cancellationToken: cancellationToken
        );

        return fileEntity;
    }

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
        // Delete old avatar file entity if it exists
        if (currentAvatarFileId.HasValue)
        {
            FileEntity? oldFile = await GetByIdAsync(currentAvatarFileId.Value, cancellationToken);
            if (oldFile != null)
            {
                Remove(oldFile);
                await SaveChangesAsync(cancellationToken);
            }
        }

        // Upload and store new avatar
        FileEntity fileEntity = await UploadAndStoreAvatarAsync(
            avatarFile: avatarFile,
            userId: userId,
            originalFileName: originalFileName,
            mimeType: mimeType,
            cancellationToken: cancellationToken
        );

        return fileEntity;
    }

    /// <inheritdoc />
    public async Task<FileEntity?> UpdateAvatarUrlFromSourceAsync(
        Guid? currentAvatarFileId,
        string? avatarUrl,
        string userId,
        bool isAvatarSourceManual,
        CancellationToken cancellationToken = default
    )
    {
        bool canUpdateAvatar = !string.IsNullOrWhiteSpace(avatarUrl) && !isAvatarSourceManual;

        if (!canUpdateAvatar)
        {
            return null;
        }

        return await UpdateAvatarFromUrlAsync(currentAvatarFileId, avatarUrl!, userId, cancellationToken);
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

        var fileEntity = FileEntity.Create(
            id: uploadResult.FileId,
            fileName: publicId,
            originalFileName: originalFileName,
            mimeType: mimeType,
            storageUrl: uploadResult.SecureUrl,
            sizeInBytes: uploadResult.Bytes,
            i18n: i18n,
            storageKey: uploadResult.PublicId
        );

        await AddAsync(fileEntity, cancellationToken);
        await SaveChangesAsync(cancellationToken);

        return fileEntity;
    }

    /// <inheritdoc />
    public async Task<FileEntity> UploadAndStoreImageFileAsync(
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    )
    {
        FileUploadResult uploadResult = await fileService.UploadFileAsync(
            file: file,
            publicId: publicId,
            folder: folder,
            cancellationToken: cancellationToken
        );

        // Best-effort: derive the poster's dominant/foreground colors from the
        // image bytes; a null result simply leaves both color columns unset.
        ImageColors? colors = await imageColorService.ExtractAsync(file, cancellationToken);

        var fileEntity = FileEntity.Create(
            id: uploadResult.FileId,
            fileName: publicId,
            originalFileName: originalFileName,
            mimeType: mimeType,
            storageUrl: uploadResult.SecureUrl,
            sizeInBytes: uploadResult.Bytes,
            i18n: i18n,
            storageKey: uploadResult.PublicId,
            dominantColorHex: colors?.DominantColorHex,
            foregroundColorHex: colors?.ForegroundColorHex
        );

        await AddAsync(fileEntity, cancellationToken);
        await SaveChangesAsync(cancellationToken);

        return fileEntity;
    }

    /// <inheritdoc />
    public async Task<FileEntity> UploadAndStoreVideoFileAsync(
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    )
    {
        FileUploadResult uploadResult = await fileService.UploadVideoFileAsync(
            file: file,
            publicId: publicId,
            folder: folder,
            cancellationToken: cancellationToken
        );

        var fileEntity = FileEntity.Create(
            id: uploadResult.FileId,
            fileName: publicId,
            originalFileName: originalFileName,
            mimeType: mimeType,
            storageUrl: uploadResult.SecureUrl,
            sizeInBytes: uploadResult.Bytes,
            i18n: i18n,
            storageKey: uploadResult.PublicId
        );

        await AddAsync(fileEntity, cancellationToken);
        await SaveChangesAsync(cancellationToken);

        return fileEntity;
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
            FileEntity? oldFile = await GetByIdAsync(currentFileId.Value, cancellationToken);
            if (oldFile?.StorageKey is not null)
            {
                await fileService.DeleteFileAsync(oldFile.StorageKey, cancellationToken);
            }

            await SoftDeleteByIdAsync(currentFileId.Value, cancellationToken);
        }

        return await UploadAndStoreImageFileAsync(
            file,
            publicId,
            folder,
            originalFileName,
            mimeType,
            cancellationToken
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
            FileEntity? oldFile = await GetByIdAsync(currentFileId.Value, cancellationToken);
            if (oldFile?.StorageKey is not null)
            {
                await fileService.DeleteFileAsync(oldFile.StorageKey, cancellationToken);
            }

            await SoftDeleteByIdAsync(currentFileId.Value, cancellationToken);
        }

        return await UploadAndStoreVideoFileAsync(
            file,
            publicId,
            folder,
            originalFileName,
            mimeType,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<bool> SoftDeleteByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        FileEntity? file = await GetByIdAsync(fileId, cancellationToken);
        if (file is null)
        {
            return false;
        }

        bool deleted = file.Delete();
        if (deleted)
        {
            await UpdateAsync(file, cancellationToken);
            await SaveChangesAsync(cancellationToken);
        }

        return deleted;
    }
}
