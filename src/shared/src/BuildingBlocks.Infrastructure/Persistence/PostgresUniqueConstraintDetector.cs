using _116.BuildingBlocks.Application.Persistence;
using Npgsql;

namespace _116.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL <see cref="IUniqueConstraintDetector" />, matching SQLSTATE 23505.
/// </summary>
public sealed class PostgresUniqueConstraintDetector : IUniqueConstraintDetector
{
    /// <inheritdoc />
    public bool IsUniqueConstraintViolation(Exception exception)
    {
        return exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }
}
