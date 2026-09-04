namespace _116.Core.Domain.Enums;

/// <summary>
/// The lifecycle position of a stored file. The four states are mutually exclusive and total.
/// </summary>
public enum EnumFileState
{
    /// <summary>
    /// Uploaded, but nothing references it yet; eligible for the reaper once the grace period passes.
    /// </summary>
    Unclaimed = 0,

    /// <summary>
    /// A referencing write took ownership of it.
    /// </summary>
    Claimed = 1,

    /// <summary>
    /// Soft-deleted; the remote asset is cleaned post-commit.
    /// </summary>
    Deleted = 2,

    /// <summary>
    /// Superseded by a newer upload.
    /// </summary>
    Replaced = 3,
}
