using _116.Storage.Domain.Enums;
using _116.Storage.Domain.Events;

namespace _116.Storage.Domain.Entities;

/// <summary>
/// Lifecycle transitions of <see cref="FileEntity" />. Its state lives in <c>Entities/FileEntity.cs</c>.
/// </summary>
public partial class FileEntity
{
    /// <summary>
    /// Marks the file as deleted (soft delete) and raises
    /// <see cref="FileSoftDeletedEvent" /> with the storage key captured at
    /// raise time so the remote asset can be cleaned post-commit.
    /// </summary>
    /// <returns>True if the file was successfully marked as deleted, false if already deleted.</returns>
    /// <remarks>
    /// This performs a soft delete, marking the file as deleted without physically removing it.
    /// </remarks>
    public bool Delete(DateTime now)
    {
        if (IsDeleted)
        {
            return false;
        }

        State = EnumFileState.Deleted;
        DeletedAt = now;

        AddDomainEvent(new FileSoftDeletedEvent(FileId: Id, StorageKey: StorageKey, Kind: Kind));

        return true;
    }

    /// <summary>
    /// Marks the file as superseded by a newer upload: applies the same
    /// soft-delete state as <see cref="Delete" /> but raises
    /// <see cref="FileReplacedEvent" /> instead, so replacement flows and
    /// plain deletions stay distinguishable to post-commit consumers.
    /// </summary>
    /// <returns>True if the file was marked as replaced, false if already deleted.</returns>
    public bool MarkReplaced(DateTime now)
    {
        if (IsDeleted)
        {
            return false;
        }

        State = EnumFileState.Replaced;
        DeletedAt = now;

        AddDomainEvent(new FileReplacedEvent(FileId: Id, OldStorageKey: StorageKey, Kind: Kind));

        return true;
    }
}
