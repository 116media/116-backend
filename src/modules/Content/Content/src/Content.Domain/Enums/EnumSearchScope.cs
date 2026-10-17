namespace _116.Content.Domain.Enums;

/// <summary>
/// The content types a public search can be narrowed to.
/// </summary>
public enum EnumSearchScope
{
    /// <summary>
    /// Every searchable content type, ranked together.
    /// </summary>
    All,

    /// <summary>
    /// Published articles only.
    /// </summary>
    Articles,

    /// <summary>
    /// Published videos only.
    /// </summary>
    Videos,

    /// <summary>
    /// Active short videos only.
    /// </summary>
    Shorts,
}
