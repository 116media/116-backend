# What this suite does well

An audit that only lists defects gives a false picture and, worse, invites changes
that damage the parts that are right. This suite is above average for its size, and
several decisions in it are better than what is typical in .NET codebases of this
scale. These are the properties to protect while fixing everything else.

## Boundary discipline in the integration project is close to ideal

Across 48,226 lines of integration test code:

- **zero** mocked repositories
- **zero** mocked `DbContext`s
- **zero** uses of reflection to reach private members
- **zero** hand-built `ServiceCollection`s
- **zero** `new FooHandler()` / `new FooValidator()` constructions

The only `Moq` usage in the entire project is `Mock<IApplicationBuilder>` in two
module-seeding tests, which is host plumbing rather than a mocked collaborator.
Most suites this size have drifted into constructing handlers directly inside
integration tests to chase a coverage number; this one has not.

Adapters that are stubbed inside the host still get their real code path proven
against owned loopback servers: `SmtpEmailSenderTests` speaks real SMTP over a real
socket, and the Odesli and Resend adapter tests serve real HTTP. That is the
correct line, drawn deliberately rather than by accident.

## Assertion depth in endpoint tests is genuinely strong

A scripted scan flagged 18 tests that appeared to assert a success status with no
state or body check. On manual inspection **all 18 were false positives** — every
one verified persisted state through a private helper the scan could not follow.

Measured across the integration suite: 1,035 tests assert persisted state and 964
assert response body or problem shape. Auth coverage is systematic rather than ad
hoc — 215 files test 401, 139 test 403, 192 test 404, 106 test 400/422, 77 test 409.

## Builders drive real domain methods

This is the single best decision in the fixture layer.

```csharp
// tests/Fixtures/Builders/Entities/Content/VideoBuilder.cs:309-338
case EnumContentStatus.Published:
    entity.MarkPendingReview();
    entity.Approve();
    entity.Publish(errors);
    break;
```

Most suites at this scale would reflect `Status = Published` into place. Because
these builders replay the real transition sequence, a builder **cannot** produce an
entity the domain forbids, and adding a guard to `Publish()` makes the builders
fail loudly instead of manufacturing an impossible aggregate.

The OTP counter-example in [fixtures/01](fixtures/01-constant-drift.md) stands out
precisely because it is an anomaly against this otherwise consistent discipline.

## Routes are composed from production constants, and the rule actually held

1,401 route references built from `EditorialRouteConstants`,
`CommerceRouteConstants`, and their siblings — and **zero** hardcoded `"/api/v1/…"`
literals across 357 integration test files. Renaming a route segment in `src/`
breaks the build rather than 200 tests at runtime.

Rules like this are easy to state and rare to sustain. This one was sustained. It
is also the model for fixing the constant drift documented in
[fixtures/01](fixtures/01-constant-drift.md): the same discipline, applied to
numbers instead of strings.

## Seeding correctly separates arrangement from behaviour

```csharp
// tests/Integration/Common/Base/BaseApiTest.cs:87-109
foreach (EntityEntry<IAggregate> entry in context.ChangeTracker.Entries<IAggregate>())
{
    entry.Entity.ClearDomainEvents();
}

await context.SaveChangesAsync();
```

Because builders call real domain methods, they raise real domain events; because
seeding uses application-container contexts, the dispatch interceptor is live and
would fire welcome emails and notification writes during every test's arrange step.
Clearing the events makes seeding equivalent to loading pre-existing rows.

That interaction is subtle, someone reasoned it through, and the doc comment
explaining it is genuinely good — it explains *why*, not *what*.

## Cache invalidation between tests was anticipated, not discovered

`BaseApiTest` invalidates three eviction tokens on every test start, because the
`IMemoryCache` lives in the process-lifetime host and therefore survives the
database reset. This is a leak most suites find six months in, as an
unreproducible ordering flake. Here it was handled up front, with the reasoning
written down.

## The `Workflows/` folder is the house standard

`CacheInvalidationRegressionTests` follows a four-step pattern: warm the cache over
HTTP, make the data stale via a raw write that bypasses the event pipeline, **prove
the stale value is still served**, then drive the mutation and assert freshness.
That third step is what separates a real cache-invalidation test from one that
would pass with no cache at all.

`ExternalAssetCleanupFlowTests` proves by failure injection that a storage outage
can orphan a remote asset but can never block a commit or leave dangling rows.
`EmailDeliveryFlowTests` documents a genuinely subtle inversion — the OTP row
stores only a hash, so the test extracts the plaintext from the delivered email and
drives it back through the real verify endpoint.

## Domain-entity tests are the strongest layer

```csharp
// tests/Unit/Modules/Content/Domain/Entities/ContentOrderEntityTests.cs:112-198
```

These assert the state transition, the raised domain event **with its payload**,
and that no event is raised on the failure path. This is the standard the handler
layer should be held to, and the fact that it already exists in the same repository
makes [unit/02](unit/02-state-transition-blindness.md) a consistency problem rather
than a knowledge problem.

## Async hygiene is clean

Zero `async void` tests. Zero blocking `.Result` / `.Wait()` /
`.GetAwaiter().GetResult()`. Zero un-awaited `ThrowAsync` assertions out of 508.
867 of 868 `Verify` calls in the unit suite carry an explicit `Times`. This is
better than almost any suite of comparable size.

## Naming discipline is near-perfect

6,615 of 6,737 unit test methods follow the `Method_Scenario_ExpectedResult`
convention, and a scripted cross-check of all 1,859 integration test names against
their asserted status codes found exactly **one** genuine mismatch. Across 1,418
files written by multiple people, that is remarkable consistency.

## The host boots once

`PostgresFixture` eagerly touches the API fixture's services so the application
starts exactly once for 1,876 tests. Per-class `WebApplicationFactory`
instantiation is the most common performance mistake in .NET integration suites,
and this suite does not make it.

## Real resources instead of mocked localizers

`TestErrorsFactory` builds actual error-factory instances over the real embedded
`.resx` catalogs rather than `Mock<IStringLocalizer<T>>`. A missing or malformed
resource key can therefore fail a test rather than being papered over by a mock
returning the key name. The implementation has defects
([fixtures/03](fixtures/03-localizer-factory-defects.md)), but the decision is
correct and should survive the fix.

## What follows from this

Two things.

**The fixes in this audit are almost all small.** A suite this well-structured does
not need rebuilding; it needs a few hundred assertions strengthened and four
infrastructure defects corrected. Nothing here argues for starting over.

**Where a good pattern already exists in-repo, prefer it to a new one.** Every
finding in this set has a correct example somewhere in the same codebase —
`ContentOrderEntityTests` for state assertions, `CultureScope` for culture pinning,
the route constants for shared values, `Workflows/` for integration depth. The
remediation is largely a matter of spreading practices the team has already proven
it knows how to apply.
