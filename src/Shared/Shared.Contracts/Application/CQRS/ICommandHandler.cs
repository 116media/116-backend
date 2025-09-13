namespace _116.Shared.Contracts.Application.CQRS;

/// <summary>
/// Handles commands that do not return a response.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle, which returns no result.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand> where TCommand : ICommand;

/// <summary>
/// Handles commands that return a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    where TResponse : notnull;
