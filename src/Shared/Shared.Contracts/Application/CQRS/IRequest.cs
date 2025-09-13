namespace _116.Shared.Contracts.Application.CQRS;

/// <summary>
/// Base marker interface for all requests (commands and queries).
/// </summary>
public interface IRequest;

/// <summary>
/// Base interface for requests that return a response.
/// </summary>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
public interface IRequest<out TResponse> : IRequest;
