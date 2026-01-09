using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Application.Shared.Specifications;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;

namespace _116.Core.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IFileRepository"/> using Entity Framework Core.
/// </summary>
public class FileRepository(CoreDbContext context, IFileService fileService) : IFileRepository
{
    /// <inheritdoc />
    public async Task<FileEntity?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var specification = new FileByIdNotDeletedSpecification(fileId);

        return await context.Files.FirstOrDefaultBySpecificationAsync(specification, cancellationToken);
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
            sizeInBytes: uploadResult.Bytes
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
            sizeInBytes: downloadResult.SizeInBytes
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
}
