using Quartz;

namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// Minimal <see cref="IJobExecutionContext" /> used to drive a real Quartz job once from a test.
/// A job that runs on a schedule has neither an HTTP route nor a repository method, so the only
/// way to exercise it through its real entry point is to invoke <c>Execute</c> with a context.
/// The production jobs read nothing from the context except
/// <see cref="IJobExecutionContext.CancellationToken" />, which is the single member this
/// implementation makes meaningful; everything else throws so an unnoticed new dependency on
/// scheduler state fails loudly instead of silently reading a default.
/// </summary>
public class TestJobExecutionContext : IJobExecutionContext
{
    private readonly Dictionary<object, object?> _data = [];

    /// <inheritdoc />
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Creates a context whose cancellation token is the one supplied, defaulting to a token that
    /// is never cancelled.
    /// </summary>
    /// <param name="cancellationToken">The token the job under test should observe.</param>
    public TestJobExecutionContext(CancellationToken cancellationToken = default)
    {
        CancellationToken = cancellationToken;
    }

    /// <inheritdoc />
    public IScheduler Scheduler => throw new NotSupportedException(Unsupported);

    /// <inheritdoc />
    public ITrigger Trigger => throw new NotSupportedException(Unsupported);

    /// <inheritdoc />
    public ICalendar? Calendar => throw new NotSupportedException(Unsupported);

    /// <inheritdoc />
    public bool Recovering => false;

    /// <inheritdoc />
    public TriggerKey RecoveringTriggerKey => throw new NotSupportedException(Unsupported);

    /// <inheritdoc />
    public int RefireCount => 0;

    /// <inheritdoc />
    public JobDataMap MergedJobDataMap { get; } = [];

    /// <inheritdoc />
    public IJobDetail JobDetail => throw new NotSupportedException(Unsupported);

    /// <inheritdoc />
    public IJob JobInstance => throw new NotSupportedException(Unsupported);

    /// <inheritdoc />
    public DateTimeOffset FireTimeUtc { get; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public DateTimeOffset? ScheduledFireTimeUtc => null;

    /// <inheritdoc />
    public DateTimeOffset? PreviousFireTimeUtc => null;

    /// <inheritdoc />
    public DateTimeOffset? NextFireTimeUtc => null;

    /// <inheritdoc />
    public string FireInstanceId { get; } = Guid.NewGuid().ToString("N");

    /// <inheritdoc />
    public object? Result { get; set; }

    /// <inheritdoc />
    public TimeSpan JobRunTime => TimeSpan.Zero;

    /// <inheritdoc />
    public void Put(object key, object objectValue) => _data[key] = objectValue;

    /// <inheritdoc />
    public object? Get(object key) => _data.GetValueOrDefault(key);

    private const string Unsupported =
        "The job under test read scheduler state that the integration test context does not provide.";
}
