namespace _116.Shared.Application.Exceptions.Problems;

/// <summary>
/// Composes a module's per-aggregate catalogs into the single lookup its strategy reads.
/// </summary>
public static class RuleProblemCatalog
{
    /// <summary>
    /// Merges the catalogs into one lookup. A duplicate code across catalogs throws at first use
    /// rather than silently shadowing an entry.
    /// </summary>
    /// <param name="catalogs">The per-aggregate catalogs to merge.</param>
    /// <returns>The merged lookup, keyed by rule code.</returns>
    public static Dictionary<string, RuleProblem> Merge(params IRuleProblemCatalog[] catalogs)
    {
        return catalogs.SelectMany(catalog => catalog.Problems).ToDictionary(entry => entry.Key, entry => entry.Value);
    }
}
