namespace _116.Shared.Contracts.Application.CQRS;

/// <summary>
/// Represents a command that does not return a result.
/// This is a marker interface for commands that perform actions without returning data.
/// </summary>
public interface ICommand : IRequest;

/// <summary>
/// Represents a command with a response of type <typeparamref name="TResult"/>.
/// Commands encapsulate intent to change system state and may optionally return data.
/// </summary>
/// <typeparam name="TResult">The type of the response returned when the command is handled.</typeparam>
public interface ICommand<out TResult> : IRequest<TResult>;
