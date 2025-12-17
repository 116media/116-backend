using System.ComponentModel.DataAnnotations;

using _116.BuildingBlocks.Constants;
using _116.Core.Application.Shared.Errors;
using _116.Shared.Application.Exceptions;
using _116.Shared.Domain;

namespace _116.Core.Domain.Entities;

/// <summary>
/// Represents a file uploaded to the system, containing metadata and storage information
/// for managing files across the application.
/// </summary>
/// <remarks>
/// This entity serves as an aggregate root for file management, storing both the original
/// file information and the system-generated metadata for storage and retrieval.
/// </remarks>
public class FileEntity : Aggregate<Guid>
{
    /// <summary>
    /// System-generated unique filename used for storage.
    /// </summary>
    [MaxLength(FileConstants.MaxFileNameLength)]
    public string FileName { get; private set; } = null!;

    /// <summary>
    /// Original filename as uploaded by the user.
    /// </summary>
    [MaxLength(FileConstants.MaxOriginalFileNameLength)]
    public string OriginalFileName { get; private set; } = null!;

    /// <summary>
    /// MIME type of the file (e.g., "image/jpeg", "application/pdf").
    /// </summary>
    [MaxLength(FileConstants.MaxMimeTypeLength)]
    public string MimeType { get; private set; } = null!;

    /// <summary>
    /// URL or path where the file is stored in the storage system.
    /// </summary>
    [MaxLength(FileConstants.MaxStorageUrlLength)]
    public string StorageUrl { get; private set; } = null!;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long SizeInBytes { get; private set; }

    /// <summary>
    /// Indicates whether the file has been deleted (soft delete).
    /// </summary>
    public bool IsDeleted { get; private set; } = FileConstants.DefaultIsDeleted;

    /// <summary>
    /// Date and time when the file was deleted, in UTC.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Creates a new file entity.
    /// </summary>
    /// <param name="id">The unique identifier of the file.</param>
    /// <param name="fileName">System-generated unique filename.</param>
    /// <param name="originalFileName">Original filename as uploaded.</param>
    /// <param name="mimeType">MIME type of the file.</param>
    /// <param name="storageUrl">Storage URL or path.</param>
    /// <param name="sizeInBytes">File size in bytes.</param>
    /// <returns>A new <see cref="FileEntity"/> instance.</returns>
    public static FileEntity Create(
        Guid id,
        string fileName,
        string originalFileName,
        string mimeType,
        string storageUrl,
        long sizeInBytes
    )
    {
        BadRequestException? error = (fileName, originalFileName, mimeType, storageUrl, sizeInBytes) switch
        {
            var (f, _, _, _, _) when string.IsNullOrWhiteSpace(f) => CoreErrors.FileNameRequired(),
            var (_, o, _, _, _) when string.IsNullOrWhiteSpace(o) => CoreErrors.OriginalFileNameRequired(),
            var (_, _, m, _, _) when string.IsNullOrWhiteSpace(m) => CoreErrors.MimeTypeRequired(),
            var (_, _, _, s, _) when string.IsNullOrWhiteSpace(s) => CoreErrors.StorageUrlRequired(),
            (_, _, _, _, <= 0) => CoreErrors.FileSizeMustBeGreaterThanZero(),
            _ => null
        };

        if (error is not null) throw error;

        return new FileEntity
        {
            Id = id,
            FileName = fileName,
            OriginalFileName = originalFileName,
            MimeType = mimeType,
            StorageUrl = storageUrl,
            SizeInBytes = sizeInBytes,
        };
    }

    /// <summary>
    /// Updates the storage URL of the file.
    /// </summary>
    /// <param name="newStorageUrl">The new storage URL.</param>
    public void UpdateStorageUrl(string newStorageUrl)
    {
        if (string.IsNullOrWhiteSpace(newStorageUrl))
        {
            throw CoreErrors.StorageUrlRequired();
        }

        StorageUrl = newStorageUrl;
    }

    /// <summary>
    /// Marks the file as deleted (soft delete).
    /// </summary>
    /// <returns>True if the file was successfully marked as deleted, false if already deleted.</returns>
    /// <remarks>
    /// This performs a soft delete, marking the file as deleted without physically removing it.
    /// </remarks>
    public bool Delete()
    {
        if (IsDeleted) return false;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        return true;
    }
}
