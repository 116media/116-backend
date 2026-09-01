namespace _116.Shared.Application.Services;

/// <summary>
/// The fallback guard for hosts with no module registered: every invocation is treated as a
/// first run. Modules replace it with a store backed by their own schema, which is what makes
/// replay suppression real.
/// </summary>
public class NoOpProcessedDomainEventStore : IProcessedDomainEventStore
{
    /// <inheritdoc />
    public Task<bool> TryRecordAsync(Guid eventId, string handlerName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
