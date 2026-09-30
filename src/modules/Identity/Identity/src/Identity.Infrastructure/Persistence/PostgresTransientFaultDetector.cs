using _116.Identity.Application.Shared.Authorizations.Contracts;
using Npgsql;

namespace _116.Identity.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL <see cref="ITransientFaultDetector" />. Matches the connection-class SQLSTATEs
/// (08xxx) and the three shutdown codes (57P01 to 57P03).
/// </summary>
public sealed class PostgresTransientFaultDetector : ITransientFaultDetector
{
    private static readonly HashSet<string> UnreachableStates =
    [
        "08000", // connection_exception
        "08001", // sqlclient_unable_to_establish_sqlconnection
        "08003", // connection_does_not_exist
        "08004", // sqlserver_rejected_establishment_of_sqlconnection
        "08006", // connection_failure
        "08007", // transaction_resolution_unknown
        "57P01", // admin_shutdown
        "57P02", // crash_shutdown
        "57P03", // cannot_connect_now
    ];

    /// <inheritdoc />
    public bool IsUnreachable(Exception exception)
    {
        return exception switch
        {
            TimeoutException => true,
            NpgsqlException npgsql => npgsql.SqlState is not null && UnreachableStates.Contains(npgsql.SqlState),
            _ => false,
        };
    }
}
