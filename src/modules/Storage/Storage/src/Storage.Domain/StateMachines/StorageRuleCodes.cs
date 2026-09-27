namespace _116.Storage.Domain.StateMachines;

/// <summary>
/// Stable identifiers for the core domain rules reported through
/// <see cref="Exceptions.StorageRuleException" />, scoped <c>core.&lt;entity&gt;.&lt;rule&gt;</c>.
/// </summary>
public static class StorageRuleCodes
{
    /// <summary>
    /// A required file name was blank. Args: none.
    /// </summary>
    public const string FileNameRequired = "storage.file.file-name-required";

    /// <summary>
    /// A required original file name was blank. Args: none.
    /// </summary>
    public const string OriginalFileNameRequired = "storage.file.original-file-name-required";

    /// <summary>
    /// A required MIME type was blank. Args: none.
    /// </summary>
    public const string MimeTypeRequired = "storage.file.mime-type-required";

    /// <summary>
    /// A required storage URL was blank. Args: none.
    /// </summary>
    public const string StorageUrlRequired = "storage.file.storage-url-required";

    /// <summary>
    /// A file size must be greater than zero. Args: none.
    /// </summary>
    public const string FileSizeMustBePositive = "storage.file.size-must-be-positive";

    /// <summary>
    /// A file that already has a row was handed in to be recorded again. Args: none.
    /// </summary>
    public const string FileAlreadyRecorded = "storage.file.already-recorded";
}
