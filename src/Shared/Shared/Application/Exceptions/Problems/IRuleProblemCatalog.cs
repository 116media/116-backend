namespace _116.Shared.Application.Exceptions.Problems;

/// <summary>
/// One aggregate's contribution to a module's problem catalog. Each aggregate owns its entries so
/// a new rule edits its owner, never a module-wide table.
/// </summary>
public interface IRuleProblemCatalog
{
    /// <summary>
    /// The problems declared by this catalog, keyed by rule code.
    /// </summary>
    IReadOnlyDictionary<string, RuleProblem> Problems { get; }
}
