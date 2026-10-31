namespace _116.Content.Infrastructure.Search;

/// <summary>
/// Settings the search reader and the projectors share.
/// </summary>
public static class SearchConfig
{
    /// <summary>
    /// The primary text-search configuration. The catalogue is mostly French, so this is
    /// the one whose stemming matters most.
    /// </summary>
    public const string French = "french";

    /// <summary>
    /// The secondary configuration. Titles and descriptions are regularly English, which
    /// French stemming leaves unstemmed, so both are indexed and both are queried.
    /// </summary>
    public const string English = "english";
}
