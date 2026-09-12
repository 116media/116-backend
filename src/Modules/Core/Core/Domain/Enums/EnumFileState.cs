namespace _116.Core.Domain.Enums;

/// <summary>
/// The lifecycle position of a stored file. The three states are mutually exclusive and total.
/// </summary>
public enum EnumFileState
{
    /// <summary>
    /// Recorded and referenced; the live state of every file row.
    /// </summary>
    Stored = 0,

    /// <summary>
    /// Soft-deleted; the remote asset is cleaned post-commit.
    /// </summary>
    Deleted = 1,

    /// <summary>
    /// Superseded by a newer upload.
    /// </summary>
    Replaced = 2,
}
