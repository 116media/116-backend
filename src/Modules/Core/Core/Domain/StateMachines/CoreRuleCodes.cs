namespace _116.Core.Domain.StateMachines;

/// <summary>
/// Stable identifiers for the core domain rules reported through
/// <see cref="Exceptions.CoreRuleException" />, scoped <c>core.&lt;entity&gt;.&lt;rule&gt;</c>.
/// </summary>
public static class CoreRuleCodes
{
    /// <summary>
    /// A required file name was blank. Args: none.
    /// </summary>
    public const string FileNameRequired = "core.file.file-name-required";

    /// <summary>
    /// A required original file name was blank. Args: none.
    /// </summary>
    public const string OriginalFileNameRequired = "core.file.original-file-name-required";

    /// <summary>
    /// A required MIME type was blank. Args: none.
    /// </summary>
    public const string MimeTypeRequired = "core.file.mime-type-required";

    /// <summary>
    /// A required storage URL was blank. Args: none.
    /// </summary>
    public const string StorageUrlRequired = "core.file.storage-url-required";

    /// <summary>
    /// A file size must be greater than zero. Args: none.
    /// </summary>
    public const string FileSizeMustBePositive = "core.file.size-must-be-positive";
}
