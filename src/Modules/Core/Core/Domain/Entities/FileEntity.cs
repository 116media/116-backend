using System.ComponentModel.DataAnnotations;
using _116.BuildingBlocks.Constants;
using _116.Core.Domain.Events;
using _116.Core.Domain.Exceptions;
using _116.Core.Domain.StateMachines;
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
    /// Provider-agnostic storage key used to identify and delete the file
    /// from cloud storage. For Cloudinary this is the public ID.
    /// </summary>
    [MaxLength(FileConstants.MaxStorageKeyLength)]
    public string? StorageKey { get; private set; }

    /// <summary>
    /// The dominant color of the image as <c>#RRGGBB</c>, used as a background
    /// color on the frontend. Null for non-image files or when extraction yields
    /// no usable color.
    /// </summary>
    [MaxLength(FileConstants.ColorHexLength)]
    public string? DominantColorHex { get; private set; }

    /// <summary>
    /// The accessible foreground (text) color as <c>#RRGGBB</c>, computed from
    /// <see cref="DominantColorHex" /> via WCAG contrast (black or white). Null
    /// whenever <see cref="DominantColorHex" /> is null.
    /// </summary>
    [MaxLength(FileConstants.ColorHexLength)]
    public string? ForegroundColorHex { get; private set; }

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
    /// <param name="storageKey">Provider-agnostic storage key (e.g. Cloudinary public ID).</param>
    /// <param name="dominantColorHex">Optional dominant color as <c>#RRGGBB</c> (background).</param>
    /// <param name="foregroundColorHex">Optional contrasting foreground color as <c>#RRGGBB</c> (text).</param>
    /// <returns>A new <see cref="FileEntity"/> instance.</returns>
    public static FileEntity Create(
        Guid id,
        string fileName,
        string originalFileName,
        string mimeType,
        string storageUrl,
        long sizeInBytes,
        string? storageKey = null,
        string? dominantColorHex = null,
        string? foregroundColorHex = null
    )
    {
        Exception? error = (fileName, originalFileName, mimeType, storageUrl, sizeInBytes) switch
        {
            var (f, _, _, _, _) when string.IsNullOrWhiteSpace(f) => new CoreRuleException(
                CoreRuleCodes.FileNameRequired
            ),
            var (_, o, _, _, _) when string.IsNullOrWhiteSpace(o) => new CoreRuleException(
                CoreRuleCodes.OriginalFileNameRequired
            ),
            var (_, _, m, _, _) when string.IsNullOrWhiteSpace(m) => new CoreRuleException(
                CoreRuleCodes.MimeTypeRequired
            ),
            var (_, _, _, s, _) when string.IsNullOrWhiteSpace(s) => new CoreRuleException(
                CoreRuleCodes.StorageUrlRequired
            ),
            (_, _, _, _, <= 0) => new CoreRuleException(CoreRuleCodes.FileSizeMustBePositive),
            _ => null,
        };

        if (error is not null)
        {
            throw error;
        }

        return new FileEntity
        {
            Id = id,
            FileName = fileName,
            OriginalFileName = originalFileName,
            MimeType = mimeType,
            StorageUrl = storageUrl,
            SizeInBytes = sizeInBytes,
            StorageKey = storageKey,
            DominantColorHex = dominantColorHex,
            ForegroundColorHex = foregroundColorHex,
        };
    }

    /// <summary>
    /// Marks the file as deleted (soft delete) and raises
    /// <see cref="FileSoftDeletedEvent" /> with the storage key captured at
    /// raise time so the remote asset can be cleaned post-commit.
    /// </summary>
    /// <returns>True if the file was successfully marked as deleted, false if already deleted.</returns>
    /// <remarks>
    /// This performs a soft delete, marking the file as deleted without physically removing it.
    /// </remarks>
    public bool Delete()
    {
        if (IsDeleted)
        {
            return false;
        }

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;

        AddDomainEvent(new FileSoftDeletedEvent(FileId: Id, StorageKey: StorageKey));

        return true;
    }

    /// <summary>
    /// Marks the file as superseded by a newer upload: applies the same
    /// soft-delete state as <see cref="Delete" /> but raises
    /// <see cref="FileReplacedEvent" /> instead, so replacement flows and
    /// plain deletions stay distinguishable to post-commit consumers.
    /// </summary>
    /// <returns>True if the file was marked as replaced, false if already deleted.</returns>
    public bool MarkReplaced()
    {
        if (IsDeleted)
        {
            return false;
        }

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;

        AddDomainEvent(new FileReplacedEvent(FileId: Id, OldStorageKey: StorageKey));

        return true;
    }
}
