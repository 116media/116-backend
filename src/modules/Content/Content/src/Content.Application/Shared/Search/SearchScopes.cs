using _116.Content.Domain.Enums;

namespace _116.Content.Application.Shared.Search;

/// <summary>
/// Translates between the API's scopes and the content types the projection stores.
/// The only place the two enums meet.
/// </summary>
public static class SearchScopes
{
    private static readonly Dictionary<EnumSearchScope, EnumCoreContentType> ContentTypes = new()
    {
        [EnumSearchScope.Articles] = EnumCoreContentType.Article,
        [EnumSearchScope.Videos] = EnumCoreContentType.Video,
        [EnumSearchScope.Shorts] = EnumCoreContentType.Short,
    };

    private static readonly Dictionary<EnumCoreContentType, EnumSearchScope> Scopes = ContentTypes.ToDictionary(
        entry => entry.Value,
        entry => entry.Key
    );

    /// <summary>
    /// Every scope that narrows to a type, so a caller can report a count for each even
    /// when the query matched none of them.
    /// </summary>
    public static IReadOnlyCollection<EnumSearchScope> Narrowing { get; } = [.. ContentTypes.Keys];

    /// <summary>
    /// The content type a scope narrows to, or null for <see cref="EnumSearchScope.All" />,
    /// which narrows to none.
    /// </summary>
    /// <param name="scope">The scope to translate.</param>
    /// <returns>The content type, or null.</returns>
    public static EnumCoreContentType? ToContentType(EnumSearchScope scope)
    {
        return ContentTypes.TryGetValue(scope, out EnumCoreContentType type) ? type : null;
    }

    /// <summary>
    /// The scope a content type is counted under, or null for a type no scope exposes.
    /// </summary>
    /// <param name="type">The content type to translate.</param>
    /// <returns>The scope, or null.</returns>
    public static EnumSearchScope? ToScope(EnumCoreContentType type)
    {
        return Scopes.TryGetValue(type, out EnumSearchScope scope) ? scope : null;
    }
}
