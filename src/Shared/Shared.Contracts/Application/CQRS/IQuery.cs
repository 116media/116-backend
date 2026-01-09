namespace _116.Shared.Contracts.Application.CQRS;

/// <summary>
/// Represents a query that returns a response of type <typeparamref name="TResponse"/>.
/// Queries are read-only operations that retrieve data without changing system state.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned when the query is handled.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
    where TResponse : notnull;
