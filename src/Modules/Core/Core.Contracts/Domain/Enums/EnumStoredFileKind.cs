namespace _116.Core.Contracts.Domain.Enums;

/// <summary>
/// The storage class an asset is stored and deleted as.
/// </summary>
public enum EnumStoredFileKind
{
    /// <summary>
    /// An image asset.
    /// </summary>
    Image,

    /// <summary>
    /// A video asset.
    /// </summary>
    Video,

    /// <summary>
    /// A non-media asset such as a PDF.
    /// </summary>
    Raw,
}
