using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;

namespace _116.Tests.Fixtures.Stubs;

/// <summary>
/// A Serilog sink that keeps every event in memory, so a test can assert on what the host
/// would have written to the log store rather than on the code that produces it.
/// </summary>
public class CapturingLogSink : ILogEventSink, IResettableStub
{
    private readonly ConcurrentQueue<LogEvent> _events = new();

    /// <summary>
    /// The events written since the last reset, in arrival order.
    /// </summary>
    public IReadOnlyList<LogEvent> Events => [.. _events];

    /// <inheritdoc />
    public void Emit(LogEvent logEvent) => _events.Enqueue(logEvent);

    /// <inheritdoc />
    public void Reset()
    {
        while (_events.TryDequeue(out _)) { }
    }

    /// <summary>
    /// Returns the rendered value of a property on every captured event that carries it.
    /// </summary>
    /// <param name="name">The property name, for example <c>RequestPath</c>.</param>
    /// <returns>One string per event carrying the property.</returns>
    public IReadOnlyList<string> PropertyValues(string name)
    {
        return
        [
            .. _events
                .Where(logEvent => logEvent.Properties.ContainsKey(name))
                .Select(logEvent => logEvent.Properties[name].ToString().Trim('"')),
        ];
    }
}
