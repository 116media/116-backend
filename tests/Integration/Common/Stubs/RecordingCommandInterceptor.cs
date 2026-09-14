using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// Captures the SQL text of every command the host executes, so a test can assert on the shape
/// of a statement — for example that a tracked title-only edit does not rewrite <c>body</c>.
/// </summary>
public sealed class RecordingCommandInterceptor : DbCommandInterceptor, IResettableStub
{
    private readonly List<string> _commands = [];

    /// <summary>
    /// The SQL text of every command executed since the last reset, in execution order.
    /// </summary>
    public IReadOnlyList<string> Commands
    {
        get
        {
            lock (_commands)
            {
                return _commands.ToList();
            }
        }
    }

    /// <inheritdoc />
    public void Reset()
    {
        lock (_commands)
        {
            _commands.Clear();
        }
    }

    /// <inheritdoc />
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result
    )
    {
        Record(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default
    )
    {
        Record(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result
    )
    {
        Record(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        Record(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <summary>
    /// Stores one command's SQL text.
    /// </summary>
    /// <param name="command">The command about to execute.</param>
    private void Record(DbCommand command)
    {
        lock (_commands)
        {
            _commands.Add(command.CommandText);
        }
    }
}
