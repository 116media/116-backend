using _116.Tests.Fixtures.Stubs;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;

namespace _116.Tests.Fixtures;

/// <summary>
/// An <see cref="ApiFixture" /> whose log events are captured in memory, so a test can read
/// what the host wrote for a request.
/// </summary>
/// <remarks>
/// Serilog's request-logging middleware writes through the static <see cref="Log.Logger" />,
/// which the host assigns while building. It is swapped afterwards and restored on dispose,
/// since it is process-global; this host therefore runs in its own non-parallel collection.
/// The middleware and its enrichment are untouched, so what reaches the sink is what would
/// have reached the log store.
/// </remarks>
/// <param name="db">The Testcontainer database backing this host.</param>
public class RequestLogApiFixture(PostgresFixture db) : ApiFixture(db)
{
    private ILogger? _previousLogger;
    private Logger? _capturingLogger;

    /// <summary>
    /// The events the host has written since the last reset.
    /// </summary>
    public CapturingLogSink Sink { get; } = new();

    /// <inheritdoc />
    protected override IHost CreateHost(IHostBuilder builder)
    {
        IHost host = base.CreateHost(builder);

        _previousLogger = Log.Logger;
        _capturingLogger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.Sink(Sink).CreateLogger();
        Log.Logger = _capturingLogger;

        return host;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (_previousLogger is not null)
        {
            Log.Logger = _previousLogger;
            _previousLogger = null;
        }

        _capturingLogger?.Dispose();
        _capturingLogger = null;

        base.Dispose(disposing);
    }
}
