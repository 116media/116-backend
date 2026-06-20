# Additional Optimization Opportunities

These are lower-priority improvements to consider after the primary fix (shared ApiFixture).

## 1. Connection Pooling for Respawn

**Current**: Each `ResetAsync()` call opens and closes a new `NpgsqlConnection`.

**Improvement**: Keep a single connection open or use a connection pool.

```csharp
// Instead of:
public async Task ResetAsync()
{
    await using var connection = new NpgsqlConnection(ConnectionString);
    await connection.OpenAsync();
    await _respawner.ResetAsync(connection);
}

// Consider:
private NpgsqlConnection? _resetConnection;

public async Task ResetAsync()
{
    _resetConnection ??= new NpgsqlConnection(ConnectionString);
    if (_resetConnection.State != ConnectionState.Open)
        await _resetConnection.OpenAsync();
    await _respawner.ResetAsync(_resetConnection);
}
```

**Expected savings**: ~2-5ms per test (connection handshake) = ~1-3s total.

## 2. DI Scope Disposal in CreateDbContext

**Current**: `CreateDbContext<T>()` creates a scope but never disposes it.

**Improvement**: Track scopes and dispose them, or let callers manage the scope lifecycle
through `await using`:

The current implementation already returns `TDbContext` which test code uses with
`await using var ctx = CreateDbContext<T>()`. EF Core's `DbContext` disposal handles
the immediate concern. The scope leak is minor but should be cleaned up for correctness.

## 3. Batch User Seeding

**Current**: `SeedTestUsersAsync()` creates a DbContext scope per test, inserts 3 users,
and calls `SaveChangesAsync()`.

**Improvement**: Since users are always the same 3 records, use raw SQL for faster insertion:

```sql
INSERT INTO identity.users (id, email, ...) VALUES (...), (...), (...)
ON CONFLICT DO NOTHING;
```

**Expected savings**: ~2-5ms per API test = ~1-2s total across 350+ tests.

## 4. Respawn Schema Separation for Repository Tests

Repository tests that only touch a single schema (e.g., only `identity`) could use a
schema-specific Respawner that truncates fewer tables, reducing reset time.

**Trade-off**: More complexity for marginal gain. Not recommended unless test count
grows significantly.

## 5. Test-Level Parallelism with Transaction Rollback

Instead of Respawn (which truncates after each test), each test could run inside a
database transaction that is rolled back at the end:

```csharp
public async ValueTask InitializeAsync()
{
    _transaction = await context.Database.BeginTransactionAsync();
}

public async ValueTask DisposeAsync()
{
    await _transaction.RollbackAsync();
}
```

**Pros**: Eliminates Respawn overhead entirely, enables per-test parallelism (each test
has its own transaction snapshot).

**Cons**: Some operations (DDL, sequence resets, multi-context scenarios) don't work
inside transactions. This is a significant architectural change and may break tests that
use multiple DbContexts or rely on committed data visibility.

**Verdict**: Worth exploring as a future optimization, but too risky for now.

## 6. Selective Test Running in CI

For CI pipelines, consider running only tests affected by changed files:

```bash
dotnet test --filter "FullyQualifiedName~Content" tests/Integration
```

This is a CI/CD concern, not a code change, but dramatically reduces feedback time
for incremental changes.

## Priority Order

1. **Shared ApiFixture** (this PR) — 6-12x improvement
2. Connection pooling for Respawn — marginal
3. DI scope disposal — correctness fix, no perf impact
4. Batch user seeding — marginal
5. Transaction rollback — future consideration, high risk
6. Selective test running — CI concern
