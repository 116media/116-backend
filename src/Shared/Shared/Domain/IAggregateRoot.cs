namespace _116.Shared.Domain;

/// <summary>
/// Marks a type as an aggregate root: the entry point into its aggregate, and the only kind of
/// type <see cref="IRepository{TEntity,TId}" /> may be opened over.
/// </summary>
public interface IAggregateRoot;
