# Performance Benchmarks

## Pre-Fix Baseline

| Metric | Value |
|--------|-------|
| Total tests | 535 |
| Total execution time | ~6+ minutes (~360s) |
| WebApplicationFactory instances created | 45 |
| Respawn resets | 535 |
| User seed operations | ~350 (API tests only) |
| Parallelism | None (single collection, serial execution) |
| Average time per test | ~670ms |

## Cost Breakdown per Test (Pre-Fix)

For a typical API test:

```
WebApplicationFactory share of boot time .... ~2-5s (amortized across class tests)
Respawn.ResetAsync() ........................ ~10-30ms
SeedTestUsersAsync() ........................ ~5-15ms
Test-specific seeding ....................... ~5-20ms
HTTP request + handler execution ............ ~5-50ms
Assertions .................................. ~1ms
                                              --------
Total per test (in-class) ................... ~30-120ms
Total per class (including factory boot) .... ~2-5s + N * 30-120ms
```

The problem is not per-test cost — it's 45 factory boots.

## Post-Fix Expectations

| Metric | Before | After |
|--------|--------|-------|
| WebApplicationFactory instances | 45 | 1 |
| Factory boot time (total) | 90-225s | 2-5s |
| Respawn resets | 535 | 535 (unchanged) |
| User seed operations | ~350 | ~350 (unchanged) |
| Estimated total time | ~360s | ~30-60s |
| Estimated per-test average | ~670ms | ~55-110ms |

## How to Measure

Run integration tests with timing:

```bash
time ./scripts/run-tests-with-coverage.sh integration
```

Or without coverage for raw speed:

```bash
time DOTNET_ENVIRONMENT=Testing dotnet test tests/Integration --configuration Release
```

## Post-Fix Actual Results

> To be filled in after the fix is applied and verified.

| Metric | Value |
|--------|-------|
| Total execution time | TBD |
| Average time per test | TBD |
| Speedup factor | TBD |
