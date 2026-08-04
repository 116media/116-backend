# Spec 14 — Verification checklist

## Goal

Confirm, once specs 01 through 13 are implemented, that the suite actually has the
properties the audit set out to give it. This is the final sweep a reviewer works
through before the remediation is called done. It is deliberately structured as
checks rather than changes: everything here either passes or names a spec that is not
finished.

The audit's own definition of done, from [00-index.md](00-index.md), is both suites
green, the full integration suite passing twice back to back with identical results,
and the grep-provable invariants below all holding.

## Scope

In scope:

- Build and both suites, run to the commands stated in Testing.
- Repeatability: the integration suite run twice back to back with identical results.
- Seven grep-provable invariants, each with the exact command and the expected result.
- A behavioural sweep: deliberately break a `src/` line and confirm a named test fails.
- Documentation closure across all fourteen specs.

Not in this spec:

- Fixing anything. A failed check here is reported against the owning spec and fixed
  there. This document does not accumulate its own change list.
- Coverage percentages. Coverage is a signal, not a target, and no threshold in this
  sweep is expressed as a coverage number.
- Spec 11's gated Change 3, which is explicitly deferred and is not part of done.

## Prerequisites

Specs 01 through 13 are implemented and merged, with every spec's own checklist
ticked. Running this sweep against a partial remediation produces failures that are
indistinguishable from regressions.

## Section A — Build and suite health

- [ ] `dotnet build` succeeds with no warnings introduced by the remediation.
- [ ] `dotnet csharpier check .` passes.
- [ ] `dotnet test tests/Unit` is green.
- [ ] `dotnet test tests/Integration --settings tests/coverage.runsettings` is green.
- [ ] The integration run completes inside `TestSessionTimeout`
      (`tests/coverage.runsettings:25`) with the headroom spec 11 recorded, and the
      measured time is written into spec 11's implementation notes.
- [ ] Executed test counts from the `.trx` files are recorded. Compare against the
      pre-remediation baseline and account for the difference: spec 10 removes roughly
      250 facts and adds theory cases, spec 12 replaces three tests with a ten-row
      theory, spec 06 replaces 104 tests with one theory. A drop that those three do
      not explain means tests were lost, not consolidated.

## Section B — Repeatability

This is the single strongest check in the sweep. Spec 01 removed the Quartz scheduler
from the test host and spec 02 made isolation the framework's job; both claims are
only credible if two consecutive runs agree exactly.

```bash
dotnet test tests/Integration --settings tests/coverage.runsettings \
  --logger "trx;LogFileName=run-1.trx"
dotnet test tests/Integration --settings tests/coverage.runsettings \
  --logger "trx;LogFileName=run-2.trx"
```

- [ ] Both runs are green.
- [ ] Both runs report identical passed, failed, and skipped counts.
- [ ] The set of executed test names is identical between the two `.trx` files.
- [ ] A third run started while a container from a previous run is still shutting down
      also passes, which is the cheapest available check that no fixture depends on a
      clean Docker state.

If run 2 differs from run 1 in any way, stop. A non-deterministic suite invalidates
every other check in this document.

## Section C — Grep-provable invariants

Each check states the command, the expected result, and the owning spec. Run them from
`apps/backend/`.

### C1 — No direct culture assignment outside `CultureScope`

```bash
grep -rn "Thread.CurrentThread.Current" tests/ | grep -v "tests/Fixtures/Helpers/CultureScope.cs"
grep -rn "CultureInfo.DefaultThreadCurrent" tests/
```

- [ ] Both return nothing. The baseline was 208 assignments across 104 files; every one
      is now a `using var _ = new CultureScope(...)`. Owner: spec 02.
- [ ] `tests/Fixtures/Helpers/CultureScope.cs` saves and restores both `CurrentCulture`
      and `CurrentUICulture`. Read the file; the pre-remediation version set only
      `CurrentUICulture`, which silently neutralises spec 13's `tr-TR` theory.

### C2 — Every error assertion pins status, `Title` and `Detail`

```bash
grep -rnE 'ShouldBeProblem\(HttpStatusCode\.[A-Za-z]+\)' tests/Integration
```

- [ ] Returns nothing. The baseline was 300 status-only calls out of 483 total
      `ShouldBeProblem` invocations. Owner: spec 04.
- [ ] `grep -rn "Obsolete" tests/Integration/Common/Extensions/HttpResponseExtensions.cs`
      returns nothing: the substring-matching migration shim, and the 182 call sites
      that used it, are gone.
- [ ] `grep -rnE 'ShouldBeProblem<[A-Za-z]+>\(\s*HttpStatusCode\.[A-Za-z]+,\s*"' tests/Integration`
      returns nothing: no expected detail is a hardcoded sentence — every one resolves
      through `BaseApiTest.Localized<TMessage>`.
- [ ] `grep -rn "allowEmptyBody" tests/Integration` returns the helper plus exactly one
      call site, a multipart model-binding failure, justified in a comment.

### C3 — No `BeGreaterThanOrEqualTo` on a deterministic count

```bash
grep -rn "BeGreaterThanOrEqualTo" tests/
```

- [ ] Returns exactly one result:
      `tests/Integration/Shared/Application/Extensions/RateLimitingExtensionTests.cs`,
      where `response.Headers.RetryAfter!.Delta!.Value.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero)`
      asserts on a genuinely non-deterministic duration. The baseline was 34; the other
      33 were counts the test seeded itself and could have asserted exactly. Owner:
      spec 05.

### C4 — No test constant duplicating a production numeric value

```bash
grep -rnE '\bconst (int|long|double) ' tests/Fixtures tests/Unit tests/Integration
```

- [ ] Every remaining constant is either a test-only value with no production
      counterpart, or an alias of the form
      `private const int MaxEmailLength = UserConstants.MaxEmailLength;`. A bare literal
      that mirrors a value in `src/BuildingBlocks/Constants/` or a domain constants
      class is a drift waiting to happen. Owner: spec 03.
- [ ] Spot-check the boundary tests specifically: any test named `...AtMax...`,
      `...ExceedingMax...` or `...Boundary...` computes its input from the production
      constant rather than from a literal.
- [ ] The OTP lockout boundary test named in spec 03 exists and asserts the exact
      attempt count at which lockout occurs.

### C5 — No `IHostedService` scheduler in the test host

```bash
grep -rn "IHostedService\|Quartz\|ISchedulerFactory" tests/Integration/Common/Fixtures/
```

- [ ] `ApiFixture.cs` contains an explicit removal of `IHostedService` registrations,
      and that removal is the only match other than the `using` it needs. Before spec
      01 the string `IHostedService` did not appear in the file at all, so four
      Quartz jobs ran live against the same four schemas every test asserted on, the
      outbox dispatcher firing roughly every 15 seconds. Owner: spec 01.
- [ ] Any test that asserted a job is *registered* — the pattern at
      `tests/Integration/Modules/Content/Infrastructure/BackgroundJobs/AbandonedDraftCleanupJobTests.cs:40-50`
      resolved `ISchedulerFactory` and asserted the job key exists — has been rewritten
      to assert the job's behaviour by invoking it directly, or deleted. A test that
      asserts the scheduler is live contradicts the fixture that removes it.

### C6 — No undisposed `CreateScope` in the base classes

```bash
grep -n "CreateScope" tests/Integration/Common/Base/BaseApiTest.cs tests/Integration/Common/Base/BaseRepositoryTest.cs
```

- [ ] Every match is preceded by `using`, or the scope is stored in a field disposed by
      the class's `DisposeAsync`. The baseline had four undisposed sites —
      `BaseApiTest.cs:52` and `BaseRepositoryTest.cs:37`, `:47` and `:59` — reached
      roughly 1,189 times per run, each holding an Npgsql connection rooted in a
      provider that lives for the whole session. Owner: spec 08 / integration 07.
- [ ] No call site outside `tests/Integration/Common/Base/` needed editing, which is
      the property that made this fix cheap; confirm by checking the diff touched only
      those two files.

### C7 — `MockBehavior` defaults documented

```bash
grep -rn "MockBehavior" tests/
```

- [ ] Either every `new Mock<T>()` in `tests/Unit/Common/Mocks/` states its
      `MockBehavior` explicitly, or the shared mock factory carries a doc comment
      stating which behaviour is used and why. The baseline had zero occurrences of
      `MockBehavior` anywhere in `tests/`, meaning every mock ran `Loose` by accident
      rather than by decision, returning defaults for unarranged calls. Owner: spec 07.
- [ ] The password-service default and the blanket `It.IsAny` defaults named in spec 07
      are gone, and the roughly 40 article handler test files that relied on them state
      their arrangements explicitly.

## Section D — Behavioural sweep

Grep proves shape; only a deliberate break proves the suite discriminates. For each
mutation: apply it to `src/`, run the named filter, confirm the named test fails,
revert. Record the observed failure for each row in this spec's implementation notes.

A mutation that does **not** produce a failure is a finding, not a formality.

| # | Mutation | Must fail |
| --- | --- | --- |
| D1 | Delete `article.Publish();` at `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/PublishArticle/AdminPublishArticleHandler.cs:48` | `AdminPublishArticleEndpointV1Tests.PublishArticle_AsSuperAdmin_ApprovedArticle_ReturnsOk`, on `persisted.Status.Should().Be(EnumContentStatus.Published)` |
| D2 | In `SessionQueryBuilder.CombineSpecification` (`SessionQueryBuilder.cs:117-120`), change `_specification.And(other: spec)` to `spec`, so each filter overwrites the previous one instead of composing | `AdminGetAllSessionsEndpointV1Tests`, the multi-filter tests at `:237` (status plus IP address) and `:294` (user id plus status) |
| D3 | Empty every `<value>` element in `src/Modules/Identity/Identity/Application/Shared/Errors/Messages/NotFoundErrorMessage.fr.resx` | `PublicLoginEndpointV1Tests` at `:306-315`, which asserts French literals against an `Accept-Language: fr` request, and spec 06's resource-completeness theory |
| D4 | Remove `new HeaderApiVersionReader("X-Api-Version")` from the `Combine` call at `src/Api/Program.cs:35-38` | `ApiVersionReaderTests.ConflictingVersionHeader_IsRejectedAsAmbiguous` (spec 12, if Option A was taken) |
| D5 | Delete the `RateLimitPolicies.AdminMetrics` registration from `ConfigureFixedWindowPolicies` (`RateLimitingExtension.cs:108-112`) | Exactly the `AdminMetrics` row of `RateLimitingExtensionTests.EveryNamedPolicy_RejectsWithTooManyRequests_AtItsConfiguredLimit`; the other nine rows must stay green |
| D6 | Revert spec 13's scoping in `AdminDeleteArticleCommentHandler`, restoring the unscoped `GetCommentByIdAsync` | `AdminDeleteArticleCommentEndpointV1Tests.DeleteArticleComment_WithCommentBelongingToAnotherArticle_ReturnsNotFound` |
| D7 | Restore `status.ToLower()` in `SessionQueryBuilder.WithStatus` | Exactly the seven `tr-TR` rows of the `WithStatus` theory; the seven `en-US` rows must stay green |
| D8 | Change `AuthenticationRateLimitConstants.PermitLimit` from 5 to 6 | The `Authentication` row of the policy theory, and any spec 03 alias test that reads the constant — but **no** test that hard-codes 5, since none should remain after spec 03 |
| D9 | In `CreateStandardProblemDetails` (`BaseExceptionStrategy.cs`), stop populating `Extensions["traceId"]` | Every row of spec 10's `ExceptionStrategyContractTests.CreateProblemDetails_ShouldProduceTheStandardEnvelope` |
| D10 | Add a new entity class to `src/Modules/Content/Content/Domain/Entities/` without a configuration | Spec 10's `ContentDbContextTests.Model_ShouldMapEveryDomainEntityWithAPrimaryKey` |

- [ ] D1 through D10 each produce the named failure.
- [ ] Each mutation is reverted and `git status` is clean before the next one.
- [ ] Any mutation that fails to produce a failure is written up as an open finding
      against its owning spec, with the spec reopened.

## Section E — Documentation closure

- [ ] Every checklist item in specs 01 through 13 is ticked, or has a recorded
      deviation stating what was done instead and why.
- [ ] The global progress list in [00-index.md](00-index.md) is fully checked.
- [ ] Every row in the "Decisions to make before starting" table in
      [00-index.md](00-index.md) records the decision actually taken, not just the
      recommended default. That includes spec 12's version-header Option A or B, spec
      11's container-sharing option for the rate-limited fixture, spec 06's
      replace-versus-fix decision, spec 09's `TimeProvider` decision, and the `MetaField`
      init-tests question in spec 08.
- [ ] Spec 11's gated Change 3 is recorded as deliberately deferred, with the
      420-second trigger stated and the running log of measured integration run times
      started.
- [ ] Where the code diverged from an audit document, the audit document is corrected.
      The rule in [00-index.md](00-index.md) is that the code wins and the spec gets
      corrected; a stale finding is worse than no finding, because the next reader
      spends time re-deriving something that is no longer true.
- [ ] The four standards documents
      ([standards/01](../standards/01-unit-testing-standard.md),
      [standards/02](../standards/02-integration-testing-standard.md),
      [standards/03](../standards/03-assertion-catalogue.md),
      [standards/04](../standards/04-test-data-and-fixtures.md)) match what the code now
      does. They are the part of this doc set that outlives the remediation, so any
      example in them that no longer compiles is a defect.
- [ ] The production defects in spec 13 have their own tickets, closed, and are
      referenced from the release notes rather than from a testing PR.
- [ ] The follow-up ticket for `SessionQueryBuilder.CombineSpecification` being called
      with a null specification is filed and linked from spec 13.
- [ ] `CLAUDE.md` still describes the testing rules accurately, in particular the
      unit-versus-integration boundary and the `X-Api-Version` claim if spec 12 took
      Option B.

## Testing

The commands this sweep runs, in order:

```bash
dotnet build
dotnet csharpier check .
dotnet test tests/Unit
dotnet test tests/Integration --settings tests/coverage.runsettings --logger "trx;LogFileName=run-1.trx"
dotnet test tests/Integration --settings tests/coverage.runsettings --logger "trx;LogFileName=run-2.trx"
```

Then Section C's greps, then Section D's ten mutations one at a time.

What must be green: everything, twice. What the sweep proves that the suites alone do
not: Section B proves the suite is deterministic, Section C proves the habits the audit
identified are actually gone rather than reduced, and Section D proves the suite fails
when production behaviour changes — which is the only property that makes any of the
rest worth having.

## Risks

**A green sweep can still be a weak suite.** Passing ten mutations does not prove the
suite catches the eleventh. Mitigation: treat Section D as a sample, not a proof, and
add a mutation to the table whenever a production defect ships that no test caught.
The table is meant to grow.

**Greps match text, not meaning.** `grep -rn "BeGreaterThanOrEqualTo"` returning one
result does not prove the 33 replacements assert the right numbers; it proves they no
longer assert a lower bound. Mitigation: Sections C and D are complementary, and the
mutations in D8 and D10 are specifically chosen to test invariants the greps cannot.

**Running this sweep against a partial remediation wastes a day.** Failures will be
attributed to regressions rather than to unfinished specs. Mitigation: the prerequisite
is stated at the top and Section E checks it explicitly through the spec checklists.

**Mutations left in the working tree.** Ten deliberate breaks applied and reverted by
hand is ten chances to commit one. Mitigation: `git status` is checked clean between
mutations, and the whole sweep runs on a branch that is discarded afterwards rather
than merged.

**"Done" invites stopping.** Every finding in this audit was introduced one
reasonable-looking pull request at a time, and nothing in this checklist prevents the
next one. Mitigation: the four standards documents, and the review habits in them, are
the actual deliverable; this sweep only confirms the starting point they describe is
real.

## Checklist

- [ ] Section A — build, formatter, both suites green, run time and test counts recorded
- [ ] Section B — integration suite run twice back to back with identical results and
      identical executed test sets
- [ ] Section C1 — no direct culture assignment outside `CultureScope`, and
      `CultureScope` covers both cultures
- [ ] Section C2 — no status-only `ShouldBeProblem` anywhere, the `[Obsolete]` shim
      deleted, no hardcoded expected detail, and the single `allowEmptyBody` justified
- [ ] Section C3 — exactly one `BeGreaterThanOrEqualTo` remains, on a duration
- [ ] Section C4 — no test constant duplicates a production numeric value, and the OTP
      lockout boundary test exists
- [ ] Section C5 — the test host removes `IHostedService` registrations, and no test
      asserts the scheduler is live
- [ ] Section C6 — every `CreateScope` in the two base classes is disposed
- [ ] Section C7 — `MockBehavior` is stated or documented, and the permissive defaults
      spec 07 named are gone
- [ ] Section D — all ten mutations produce their named failure, each reverted with a
      clean tree
- [ ] Section E — all spec checklists ticked or deviations recorded, decisions written
      down, audit documents corrected where the code diverged
