namespace _116.Shared.Domain;

/// <summary>
/// Marks a type as an aggregate root: the entry point into its aggregate, and the only kind of
/// type <c>IRepository&lt;TEntity, TId&gt;</c> may be opened over.
/// </summary>
public interface IAggregateRoot;
