namespace _116.Shared.Application.Services;

/// <summary>
/// Guards non-idempotent handlers against replayed events.
/// </summary>
public interface IProcessedDomainEventStore
{
    /// <summary>
    /// Claims one handler's invocation for one event, returning false when it already ran.
    /// </summary>
    /// <param name="eventId">The event being handled.</param>
    /// <param name="handlerName">The handler about to run.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True when this invocation is the first; false when it is a replay.</returns>
    Task<bool> TryRecordAsync(Guid eventId, string handlerName, CancellationToken cancellationToken = default);
}
