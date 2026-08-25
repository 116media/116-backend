# Spec 14 — Verification checklist

> **Status: run 2026-08-24, C3 and C7 re-measured after three closures; Section D run
> in full 2026-08-25.** Both suites are green, the integration suite is deterministic
> across two consecutive runs, and 18 of the 19 measured invariants hold. The three
> changes this sweep first recorded as unfinished have since landed and been measured:
> spec 05 change 4 (`BeGreaterThanOrEqualTo` down to the one exempt duration), spec 07
> change 1 (blanket read defaults down to zero), and spec 10 change 3
> (`ExceptionStrategyContractTests`, which also makes mutation D9 runnable). One
> invariant is still unmet — `MockBehavior` is neither stated nor documented anywhere
> in `tests/` — and it is unmet by decision: spec 07 defers the behaviour-mode change
> and sequences its change 6 behind it. The measured numbers are in
> [Results](#results-2026-08-24) below.
>
> Section D's ten mutations were applied, run and reverted one at a time, with `src/`
> verified pristine after each revert. Six produced exactly the named failure and
> nothing else. The other four did not: D2 surfaced a real test defect, since fixed,
> and D3, D7 and D8 were themselves mis-specified — a file path that does not exist, an
> expected failure count the code cannot produce, and a mutation that cannot fail by
> construction. All four are analysed in
> [Section D](#section-d--behavioural-sweep) below, with the observed failures
> tabulated.

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
- Seven groups of grep-provable invariants (C1–C7), each with the exact command and
  the expected result; 19 individual measurements in all, tabulated under Results.
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

**How this prerequisite was actually met.** The sweep was first run against a
remediation carrying three unfinished changes, on the reasoning that the outstanding
work was bounded and named: nothing in it could turn a green result here red without
also failing the invariant that named it. That held. All three have since landed, and
the invariants that named them — C3 and C7's second clause — were re-measured rather
than reinterpreted. What remains unfinished in specs 01 through 13 is spec 07's
changes 5 and 6, spec 08's change 4, and the site-by-site re-audit spec 05's change 2
never received. Only one of those owns an invariant here: spec 07 change 6 is gated on
`MockBehavior.Strict`, which is C7's first clause.

## Section A — Build and suite health

- [ ] `dotnet build` succeeds with no warnings introduced by the remediation — not
      reported by the verification run; both suites executing proves the build
      succeeded, not that it is warning-free.
- [ ] `dotnet csharpier check .` passes — not run.
- [x] `dotnet test tests/Unit` is green. **7,693 passed, 6 skipped, 7,699 total.**
- [x] `dotnet test tests/Integration --settings tests/coverage.runsettings` is green.
      **1,936 passed, 0 skipped.**
- [x] The integration run completes inside `TestSessionTimeout`
      (`tests/coverage.runsettings:25`) with the headroom spec 11 recorded, and the
      measured time is written into spec 11's implementation notes. **2 m 23 s and
      2 m 24 s against a 600-second budget — 24% of it.** Both runs are recorded in
      spec 11's running log and in [../90-remediation-plan.md](../90-remediation-plan.md).
- [ ] Executed test counts from the `.trx` files are recorded. Compare against the
      pre-remediation baseline and account for the difference: spec 10 removes roughly
      250 facts and adds theory cases, spec 12 replaces three tests with a ten-row
      theory, spec 06 replaces 104 tests with one theory. A drop that those three do
      not explain means tests were lost, not consolidated. — **counts recorded, the
      reconciliation not performed.** The unit total rose from 7,589 to 7,693 and the
      integration total held at 1,936; spec 10 accounts for its own movement (executed
      cases 228 → 353 in the clusters changes 1, 2, 4, 5 and 6 touched, then 119 → 117
      in the strategy cluster change 3 took, for a net −2 that matches the unit total
      moving 7,695 → 7,693) but no `.trx` diff against a pre-remediation baseline was
      produced, so nothing here proves a test was not lost elsewhere and silently
      offset.

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

- [x] Both runs are green.
- [x] Both runs report identical passed, failed, and skipped counts. **1,936 passed,
      0 failed, 0 skipped, twice; 2 m 23 s and 2 m 24 s.**
- [ ] The set of executed test names is identical between the two `.trx` files —
      **counts were compared, name sets were not.** Identical totals across two runs
      of a suite with no `[Fact(Skip)]` and no conditional discovery is strong
      evidence, but it is not the check this box states.
- [ ] A third run started while a container from a previous run is still shutting down
      also passes, which is the cheapest available check that no fixture depends on a
      clean Docker state. — not run.

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

- [x] Both return nothing. The baseline was 208 assignments across 104 files; every one
      is now a `using var _ = new CultureScope(...)`. Owner: spec 02.
- [x] `tests/Fixtures/Helpers/CultureScope.cs` saves and restores both `CurrentCulture`
      and `CurrentUICulture`. Read the file; the pre-remediation version set only
      `CurrentUICulture`, which silently neutralises spec 13's `tr-TR` theory. Both are
      captured at `:23-24` and restored at `:33-34`.

### C2 — Every error assertion pins status, `Title` and `Detail`

```bash
grep -rnE 'ShouldBeProblem\(HttpStatusCode\.[A-Za-z]+\)' tests/Integration
```

- [x] Returns nothing. The baseline was 300 status-only calls out of 483 total
      `ShouldBeProblem` invocations. Owner: spec 04. **501 typed
      `ShouldBeProblem<TException>` calls now stand where those 483 were.**
- [x] `grep -rn "Obsolete" tests/Integration/Common/Extensions/HttpResponseExtensions.cs`
      returns nothing: the substring-matching migration shim, and the 182 call sites
      that used it, are gone.
- [x] `grep -rnE 'ShouldBeProblem<[A-Za-z]+>\(\s*HttpStatusCode\.[A-Za-z]+,\s*"' tests/Integration`
      returns nothing: no expected detail is a hardcoded sentence — every one resolves
      through `BaseApiTest.Localized<TMessage>`.
- [x] `grep -rn "allowEmptyBody" tests/` returns the helper's own parameter and nothing
      else. **The invariant as written expected one justified call site; there are
      zero.** The three matches are all inside
      `tests/Integration/Common/Extensions/HttpResponseExtensions.cs` — the `<param>`
      tag at `:42`, the parameter at `:49`, and its use at `:58`. No test opts in, so
      every error assertion in the suite requires a problem body — including the
      multipart model-binding case the exemption was written for. The code is stricter
      than the spec; the spec is corrected here rather than the escape hatch being kept
      warm. Owner: spec 04.

### C3 — No `BeGreaterThanOrEqualTo` on a deterministic count

```bash
grep -rn "BeGreaterThanOrEqualTo" tests/
```

- [x] Returns exactly one result:
      `tests/Integration/Shared/Application/Extensions/RateLimitingExtensionTests.cs`,
      where `response.Headers.RetryAfter!.Delta!.Value.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero)`
      asserts on a genuinely non-deterministic duration. The baseline was 34; the other
      33 were counts the test seeded itself and could have asserted exactly. Owner:
      spec 05. **Returns 1, at `RateLimitingExtensionTests.cs:176` — the exempt
      duration.** The 33 numeric-literal sites were converted across ~26
      `tests/Integration` files, and the change list's own worked example,
      `SessionRepositoryTests.cs:184`, now reads `totalCount.Should().Be(5)` after
      seeding exactly five sessions, with `result.Should().HaveCount(3)` below it.
      No site proved impossible to give an exact number, so the conversion surfaced no
      isolation defect.
- [x] Three sites in the same sweep were tightened rather than merely converted,
      because a straight conversion would have left an assertion that still could not
      fail. Only the first is one of the 33 this grep counts; the other two are
      neighbouring weak assertions the sweep picked up while it was in the file, which
      is why they do not show in the baseline of 34. The sharpest is
      `tests/Integration/Modules/Content/Infrastructure/Mappers/ArticleMapperTests.cs:121`:
      `BeGreaterThanOrEqualTo(1)` sat on a `Math.Max(1, ceil(words / 200))` floor, so
      the floor guaranteed it for any body including an empty one. It now seeds 250
      words at `:94` and asserts `Be(2)`, which pins the formula. The other two are
      `ArticleRepositoryTests.cs:74` (`HaveCountGreaterThanOrEqualTo(2)` → `HaveCount(2)`)
      and `LyricsRepositoryTests.cs:37` (`NotBeEmpty()` → `HaveCount(3)`). This is what
      the risk note below means by grep proving they no longer assert a lower bound but
      not proving they assert the right numbers.

### C4 — No test constant duplicating a production numeric value

```bash
grep -rnE '\bconst (int|long|double) ' tests/Fixtures tests/Unit tests/Integration
```

- [x] Every remaining constant is either a test-only value with no production
      counterpart, or an alias of the form
      `private const int MaxEmailLength = UserConstants.MaxEmailLength;`. A bare literal
      that mirrors a value in `src/BuildingBlocks/Constants/` or a domain constants
      class is a drift waiting to happen. Owner: spec 03. **98 constants remain; every
      length and attempt limit reads through `TestConstants`, which aliases the
      production constant** — `TestConstants.Otp.cs:22` is
      `MaxAttempts = UserConstants.MaxOtpAttempts`. The survivors that are bare
      literals are test-only values with no production counterpart, such as
      `UserBuilder.SuffixLength` and the minimum-length floors validators do not
      publish.
- [x] Spot-check the boundary tests specifically: any test named `...AtMax...`,
      `...ExceedingMax...` or `...Boundary...` computes its input from the production
      constant rather than from a literal. **The Cloudinary rows are the sharpest
      case**: `CloudinaryServiceTests.cs:198` and `:381` read
      `FileConstants.MaxVideoFileSizeBytes` and `+ 1`. That test previously used a
      100 MB literal against a 350 MB ceiling and was not a boundary test at all; see
      [10-duplication-to-theories.md](10-duplication-to-theories.md).
- [x] The OTP lockout boundary test named in spec 03 exists and asserts the exact
      attempt count at which lockout occurs. `OtpEntityTests.cs:189` asserts false one
      attempt below `TestConstants.Otp.MaxAttempts` and `:202` asserts true at it.

### C5 — No `IHostedService` scheduler in the test host

```bash
grep -rn "IHostedService\|Quartz\|ISchedulerFactory" tests/Integration/Common/Fixtures/
```

- [x] `ApiFixture.cs` contains an explicit removal of `IHostedService` registrations,
      and that removal is the only match other than the `using` it needs. Before spec
      01 the string `IHostedService` did not appear in the file at all, so four
      Quartz jobs ran live against the same four schemas every test asserted on, the
      outbox dispatcher firing roughly every 15 seconds. Owner: spec 01. The removal is
      at `ApiFixture.cs:76-77`, matching on `IHostedService` **and**
      `QuartzHostedService`, with the `using Quartz;` at `:21` and the reason at `:68`.
      Nothing else in `tests/Integration/Common/Fixtures/` mentions Quartz.
- [x] Any test that asserted a job is *registered* — the pattern at
      `tests/Integration/Modules/Content/Infrastructure/BackgroundJobs/AbandonedDraftCleanupJobTests.cs:40-50`
      resolved `ISchedulerFactory` and asserted the job key exists — has been rewritten
      to assert the job's behaviour by invoking it directly, or deleted. A test that
      asserts the scheduler is live contradicts the fixture that removes it.
      **Restated, because the code drew a distinction the spec did not.** The
      behavioural test was added — `AbandonedDraftCleanupJobTests.cs:50` builds the job
      from the host's real `IServiceScopeFactory` and executes it — and the
      registration fact was deliberately kept alongside it at `:36-47`, with the reason
      recorded at `:20-22`. It does not contradict the fixture: `ApiFixture` removes
      the hosted service that *runs* the scheduler, not the scheduler registration, so
      `CheckExists` answers without any trigger firing. The same pair exists in
      `ExpiredOtpCleanupJobTests.cs:34-38`.

### C6 — No undisposed `CreateScope` in the base classes

```bash
grep -n "CreateScope" tests/Integration/Common/Base/BaseApiTest.cs tests/Integration/Common/Base/BaseRepositoryTest.cs
```

- [x] Every match is preceded by `using`, or the scope is stored in a field disposed by
      the class's `DisposeAsync`. The baseline had four undisposed sites —
      `BaseApiTest.cs:52` and `BaseRepositoryTest.cs:37`, `:47` and `:59` — reached
      roughly 1,189 times per run, each holding an Npgsql connection rooted in a
      provider that lives for the whole session. Owner: spec 08 / integration 07. The
      two bare `CreateScope()` calls that remain (`BaseApiTest.cs:59`,
      `BaseRepositoryTest.cs:45`) are inside a private `OpenScope()` that appends to a
      `List<IServiceScope>` drained in `DisposeAsync` (`BaseApiTest.cs:208-224`,
      `BaseRepositoryTest.cs:103-117`), preferring `DisposeAsync` where the scope
      supports it. Every other match carries `using`.
- [ ] No call site outside `tests/Integration/Common/Base/` needed editing, which is
      the property that made this fix cheap; confirm by checking the diff touched only
      those two files. — the end state is consistent with the claim, but this box asks
      about a diff, and the change is not isolable in the history now that later specs
      have edited the same files.

### C7 — `MockBehavior` defaults documented

```bash
grep -rn "MockBehavior" tests/
```

- [ ] Either every `new Mock<T>()` in `tests/Unit/Common/Mocks/` states its
      `MockBehavior` explicitly, or the shared mock factory carries a doc comment
      stating which behaviour is used and why. The baseline had zero occurrences of
      `MockBehavior` anywhere in `tests/`, meaning every mock ran `Loose` by accident
      rather than by decision, returning defaults for unarranged calls. Owner: spec 07.
      — **Still fails, unchanged from the baseline, and now by decision rather than by
      omission.** `grep -rn "MockBehavior" tests/` and `grep -rn "Loose" tests/` both
      return nothing. Spec 07's Scope section rules out changing the behaviour mode
      while the defaults are being tightened, on the grounds that doing both at once
      makes the fallout unattributable, and its change 6 is sequenced behind the strict
      step. This box is the one invariant in the sweep that a landed spec deliberately
      does not satisfy; it stays unticked, and closing it is spec 07's deferred work,
      not a documentation edit.
- [x] The password-service default and the blanket `It.IsAny` defaults named in spec 07
      are gone, and the roughly 40 article handler test files that relied on them state
      their arrangements explicitly. The password-service default is inverted:
      `MockPasswordService.cs:129` arranges `Verify` to return `false`. The blanket read
      defaults are gone — `grep -rn "GetByIdAsync(It.IsAny<Guid>()" tests/Unit/Common/Mocks/`
      returns 0, after 48 defaults were removed across 18 repository mocks. **The
      "roughly 40 files" clause was already stale when it was written**: spec 13's
      scoped-overload work and spec 04's exact-detail sweep had absorbed most of that
      exposure, so 20 tests went red rather than forty, fixed by stating the arrangement
      each actually needed, with two helpers added
      (`SetupGetItemByIdOrThrowNotFound`, `SetupGetItemTierByIdOrThrowNotFound`) and
      four nullable-entity helpers de-nullified. No handler was caught asking for the
      wrong identifier.
**What the second box does not prove, and the first one would.** Grepping the blanket
defaults to zero does not by itself make an unarranged read fail. For the
`ReturnsAsync((XEntity?)null)` family the removal is behaviour-preserving — Moq's
loose default for a reference-typed read is already `null`. What it buys is that the
arrangement is no longer asserted from another folder, and that the invariant is
grep-checkable at all; loudness needs the `MockBehavior.Strict` step the first box is
waiting on. The aggregate reads — empty lists, `false`, `0`, dictionaries, tuples —
were deliberately kept for the same reason read the other way: Moq's loose provider
does not empty a `List<T>`, `Dictionary<,>` or tuple, so removing them blind yields
null-reference traces that name a collection rather than an identifier. They are
sequenced as spec 07's change 6, after Strict. The two C7 boxes are therefore one
invariant each and not a pair: the second can hold while the first does not, which is
exactly the state measured here.

## Section D — Behavioural sweep

Grep proves shape; only a deliberate break proves the suite discriminates. For each
mutation: apply it to `src/`, run the named filter, confirm the named test fails,
revert. Record the observed failure for each row in this spec's implementation notes.

A mutation that does **not** produce a failure is a finding, not a formality.

The table below is the corrected one. Three rows were rewritten after the sweep ran —
D3's file path, D7's expected count and D8's mutation — each for the reason recorded
under [Section D results](#section-d-results-2026-08-25).

| # | Mutation | Must fail |
| --- | --- | --- |
| D1 | Delete `article.Publish();` at `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/PublishArticle/AdminPublishArticleHandler.cs:48` | `AdminPublishArticleEndpointV1Tests.PublishArticle_AsSuperAdmin_ApprovedArticle_ReturnsOk`, on `persisted.Status.Should().Be(EnumContentStatus.Published)` |
| D2 | In `SessionQueryBuilder.CombineSpecification` (`SessionQueryBuilder.cs:116-119`), change `_specification.And(other: spec)` to `spec`, so each filter overwrites the previous one instead of composing | `AdminGetAllSessionsEndpointV1Tests`, the multi-filter tests at `:226` (status plus IP address) and `:274` (user id plus status) |
| D3 | Empty every `<value>` element in `src/Shared/Shared/Application/Exceptions/Messages/SharedExceptionMessage.fr.resx` — 40 data values, excluding the four `resheader` entries | `PublicLoginEndpointV1Tests` at `:307-318`, which asserts French literals against an `Accept-Language: fr` request, and spec 06's resource-completeness theory |
| D4 | Remove `new HeaderApiVersionReader("X-Api-Version")` from the `Combine` call at `src/Api/Program.cs:35-38` | `ApiVersionReaderTests.ConflictingVersionHeader_IsRejected` (spec 12, Option A) |
| D5 | Delete the `RateLimitPolicies.AdminMetrics` registration from `ConfigureFixedWindowPolicies` (`RateLimitingExtension.cs:108-112`) | Exactly the `AdminMetrics` row of `RateLimitingExtensionTests.EveryNamedPolicy_RejectsWithTooManyRequests_AtItsConfiguredLimit`; the other nine rows must stay green |
| D6 | Revert spec 13's scoping in `AdminDeleteArticleCommentHandler`, restoring the unscoped `GetCommentByIdAsync` | `AdminDeleteArticleCommentEndpointV1Tests.DeleteArticleComment_WithCommentBelongingToAnotherArticle_ReturnsNotFound` |
| D7 | Restore both the `string normalizedStatus = status.ToLower();` local **and** the `_ when normalizedStatus.Equals(...)` arms in `SessionQueryBuilder.WithStatus` (`SessionQueryBuilder.cs:33-49`) | Exactly two `tr-TR` rows of the `WithStatus` theory — `ACTIVE` and `EXPIRED`; the other five `tr-TR` rows and all seven `en-US` rows must stay green |
| D8 | Leave `AuthenticationRateLimitConstants.PermitLimit` alone and decouple the registration from it: change the permit argument of `RateLimitPolicies.Authentication` in `ConfigureSlidingWindowPolicies` (`RateLimitingExtension.cs:46-52`) to `AuthenticationRateLimitConstants.PermitLimit + 1` | Exactly the `Authentication` row of `RateLimitingExtensionTests.EveryNamedPolicy_RejectsWithTooManyRequests_AtItsConfiguredLimit`; the other nine rows must stay green |
| D9 | In `CreateStandardProblemDetails` (`BaseExceptionStrategy.cs:46`), stop populating `Extensions["traceId"]` | Every row of spec 10's `ExceptionStrategyContractTests.CreateProblemDetails_ShouldProduceTheStandardEnvelope` except the exempt `DefaultExceptionHandler` — 19 of 20 |
| D10 | Add a new entity class to `src/Modules/Content/Content/Domain/Entities/` without a configuration | Spec 10's `ContentDbContextTests.Model_ShouldMapEveryDomainEntityWithAPrimaryKeyInTheContentSchema`, and the three sibling guards in the same file |

Three further rows needed the test they name corrected. None of the three is a change
to a mutation, and all three discriminated once the name resolved.

**D9 became runnable.** It named
`ExceptionStrategyContractTests.CreateProblemDetails_ShouldProduceTheStandardEnvelope`
against a test that did not exist. Spec 10 change 3 created it, under exactly that
name (`tests/Unit/Shared/Exceptions/Handlers/Strategies/ExceptionStrategyContractTests.cs:250`),
so the row resolves. Under the mutation its 19 rows fail — 19 rather than 20, because
`DefaultExceptionHandler` never routed through `CreateStandardProblemDetails` in the
first place and is exempted from that theory by name at `:284`. That exemption is
itself a finding and is carried as an open follow-up in
[../90-remediation-plan.md](../90-remediation-plan.md); it is not a weakening of D9,
since a mutation to the shared helper cannot break a strategy that does not call it.

**D4 needed one name corrected.** Spec 12 took Option A and the test exists, but as
`ApiVersionReaderTests.ConflictingVersionHeader_IsRejected`
(`tests/Integration/Shared/Application/Versioning/ApiVersionReaderTests.cs:51`), not
`...IsRejectedAsAmbiguous`. The mutation itself was runnable as stated.

**D10 needed the same.** The row named
`ContentDbContextTests.Model_ShouldMapEveryDomainEntityWithAPrimaryKey`; the method is
`Model_ShouldMapEveryDomainEntityWithAPrimaryKeyInTheContentSchema`
(`tests/Unit/Modules/Content/Infrastructure/Persistence/ContentDbContextTests.cs:91`).

- [x] D1 through D10 each produce the named failure. — **run 2026-08-25, and the
      claim holds against the corrected table above, not against the table as it stood
      when the sweep started.** Six rows — D1, D4, D5, D6, D9, D10 — produced exactly
      the named failure and nothing else. The other four did not, each for a different
      reason: D2 named two tests and only one failed (a real test defect, since fixed),
      D3 named a file that does not exist and discriminated once retargeted, D7
      discriminated but over two rows rather than the seven it claimed, and D8 produced
      no failure at all because the mutation could not cause one. All four are analysed
      below and the table now states what was actually run.
- [x] Each mutation is reverted and `git status` is clean before the next one.
      `git status --porcelain -- src/` returned nothing after every revert, the build
      was clean, and both suites were re-run green at the end of the sweep — unit 7,693
      passed / 6 skipped, integration 1,936 passed, matching the counts in
      [Results](#results-2026-08-24).
- [x] Any mutation that fails to produce a failure is written up as an open finding
      against its owning spec, with the spec reopened. — two mutations came back short,
      and they are different kinds of thing.
      `GetAllSessions_FilterByStatusAndIpAddress_ReturnsFilteredResults` survived D2
      because it could not fail; that is a finding, it is written up in
      [13-production-defects.md](13-production-defects.md) and in
      [../90-remediation-plan.md](../90-remediation-plan.md), and it is fixed. D8's
      no-failure result was not a suite weakness but an invalid mutation; the owning
      spec is this one, so the row is rewritten in place rather than reopened
      elsewhere.

### Section D results (2026-08-25)

| # | Outcome | Observed |
| --- | --- | --- |
| D1 | Pass | Exactly `AdminPublishArticleEndpointV1Tests.PublishArticle_AsSuperAdmin_ApprovedArticle_ReturnsOk`; the other 6 tests in the file stayed green |
| D2 | **Finding, fixed** | Only one of the two named tests failed. See [D2](#d2--the-one-genuine-test-defect-the-sweep-caught) |
| D3 | Pass, row corrected | Both named failures: 1 of 67 rows of spec 06's resource-completeness theory, and 1 of 9 in `PublicLoginEndpointV1Tests` — the `Accept-Language: fr` test asserting `Impossible de trouver` and `compte utilisateur`. Row's file path was wrong; see [D3](#d3--the-row-named-a-file-that-does-not-exist) |
| D4 | Pass | Exactly `ApiVersionReaderTests.ConflictingVersionHeader_IsRejected`; the sibling agreeing-header test stayed green |
| D5 | Pass | Exactly the `AdminMetrics` row of the policy theory; the other 9 rows green |
| D6 | Pass | `AdminDeleteArticleCommentEndpointV1Tests.DeleteArticleComment_WithCommentBelongingToAnotherArticle_ReturnsNotFound`, plus the two correct siblings `PublicDeleteArticleComment…` and `PublicEditArticleComment…`; 14 others green |
| D7 | Pass, count corrected | Exactly 2 rows failed, 49 others green. The row said seven; two is the maximum the code can produce. See [D7](#d7--the-count-was-wrong-and-the-first-mutation-was-written-wrongly) |
| D8 | **Row invalid, rewritten** | The stated mutation produced no failure, correctly. The valid equivalent did. See [D8](#d8--the-original-mutation-could-not-fail) |
| D9 | Pass | Exactly 19 rows of `ExceptionStrategyContractTests.CreateProblemDetails_ShouldProduceTheStandardEnvelope`, as the note above predicted — 20 strategies minus the exempt `DefaultExceptionHandler` |
| D10 | Pass | Four guards fired: `DomainEntities_ShouldDiscoverEveryDeclaredEntityType` (the count fact), `Model_ShouldApplyAnExplicitConfigurationForEveryDomainEntity`, `Model_ShouldMapEveryDomainEntityWithAPrimaryKeyInTheContentSchema`, and `Model_ShouldNotMapAnyTypeOutsideTheDomainEntities` |

#### D2 — the one genuine test defect the sweep caught

The mutation collapses `CombineSpecification` so each filter overwrites the previous
one. Under it, only `GetAllSessions_FilterByUserIdAndStatus_ReturnsFilteredResults`
failed. `GetAllSessions_FilterByStatusAndIpAddress_ReturnsFilteredResults` stayed
green: it seeded a single non-matching session that differed in **both** filtered
dimensions at once — a different IP **and** revoked — so dropping either filter still
excluded it. The test could not detect a lost filter, whichever filter was lost.

Fixed in
`tests/Integration/Modules/Identity/Application/Session/UseCases/Admin/Queries/GetAllSessions/V1/AdminGetAllSessionsEndpointV1Tests.cs:226-248`.
It now seeds three sessions rather than two: the matching one, a **same-IP-but-revoked**
session at `:229-230` that catches the status filter being dropped, and an
**active-on-another-IP** session at `:231` that catches the IP filter being dropped.
Verified both ways — 12 of 12 green unmutated, and under the mutation both
`GetAllSessions_FilterByStatusAndIpAddress_ReturnsFilteredResults` and
`GetAllSessions_FilterByUserIdAndStatus_ReturnsFilteredResults` fail.

This is the class of weakness Section D exists to surface. The test asserted
`OnlyContain(s => s.IpAddress!.Contains(...) && s.IsActive)` and read as a perfectly
good multi-filter test; grep proved the shape, and only the deliberate break proved
the discrimination.

#### D3 — the row named a file that does not exist

The row targeted
`src/Modules/Identity/Identity/Application/Shared/Errors/Messages/NotFoundErrorMessage.fr.resx`.
There is no such file, and never was: that folder holds
`AuthenticationErrorMessage`, `AuthorizationErrorMessage`, `ConflictErrorMessage` and
`ValidationErrorMessage`, none of them not-found. The French not-found strings the
named test asserts live in
`src/Shared/Shared/Application/Exceptions/Messages/SharedExceptionMessage.fr.resx`.

Run against the real file — 40 `<value>` elements emptied, the four `resheader` values
left alone — the mutation produced both named failures. The row is corrected above;
the mutation itself was sound, only its target was wrong.

#### D7 — the count was wrong, and the first mutation was written wrongly

**The count.** The row asked for exactly seven `tr-TR` rows to fail. Only two can:
`ACTIVE` and `EXPIRED`. The theory's seven statuses are `Active`, `ACTIVE`, `active`,
`Expired`, `EXPIRED`, `Revoked`, `REVOKED`
(`tests/Unit/Modules/Identity/Application/Session/Builders/SessionQueryBuilderTests.cs:218-227`).
Turkish lowercasing only differs from invariant on `I` → `ı`, so `Active`, `active`
and `Expired` — whose `i` is already lowercase — are unaffected, and `Revoked` and
`REVOKED` contain no `i` of either case. Observed: exactly 2 rows failed, 49 others
green, the 49 being the other 12 theory rows plus the file's 37 facts.

**The methodology note, which matters more than the count.** The first attempt at this
mutation produced *no* failure, and would have been logged as a finding against a
sound test. The mutation had been written as a one-line restoration — reintroduce
`string normalizedStatus = status.ToLower();` and switch on it — but `WithStatus`'s
arms are `_ when status.Equals(...)` and ignore the switch subject entirely
(`SessionQueryBuilder.cs:33-49`). Changing the subject changes nothing. The faithful
mutation restores the local **and** points every arm back at it. Anyone re-running D7
should check the arms, not just the subject; a `switch` whose arms are all
`_ when` predicates has no dependency on what it switches over.

#### D8 — the original mutation could not fail

The row asked for `AuthenticationRateLimitConstants.PermitLimit` to change from 5 to 6
and for the `Authentication` row of the policy theory to fail. It cannot. Production
registers the policy *from that constant*
(`RateLimitingExtension.cs:46-52`) and the theory sources its expectation from *the
same constant* (`RateLimitingExtensionTests.cs:48`), so arrangement and expectation
move together. Changing a shared source of truth is not a behavioural mutation, and no
test failed — correctly. Nor was there a spec 03 alias test to catch it: spec 03's rule
is that tests read the production constant rather than copy it, which is precisely what
makes this row unfalsifiable.

That the theory reads the constant is a deliberate property, not a defect —
`RateLimitingExtensionTests.cs:36-42` records it, and a row that hardcoded 5 would
prove only that someone typed 5 twice. The behaviour worth mutating is the *link*
between the constant and the registration, which is what the rewritten row breaks.

Under the rewritten mutation, exactly the `Authentication` row failed — `permitLimit: 5`,
`POST /api/v1/public/auth/login` — and the other nine rows stayed green. What makes the
row discriminate is the `ExhaustAsync` helper
(`RateLimitingExtensionTests.cs:143-160`): it asserts that every one of `permitLimit`
requests is admitted *before* sending the one it expects to be rejected, so a limiter
configured one permit too high fails on the rejection and a limiter configured one
permit too low fails on the admission.

## Section E — Documentation closure

- [x] Every checklist item in specs 01 through 13 is ticked, or has a recorded
      deviation stating what was done instead and why. Four *change* boxes remain
      unticked and each states its reason in its own spec: spec 05 change 2 (the named
      sites were converted, the 83 surviving `BeOfType<T>` assertions were not
      re-audited), spec 07 changes 5 and 6, and spec 08 change 4. The other unticked
      boxes in those specs are process items — a suite run repeated, a ticket filed —
      and are carried in [../90-remediation-plan.md](../90-remediation-plan.md).
- [ ] The global progress list in [00-index.md](00-index.md) is fully checked. —
      **two entries short, deliberately.** Spec 07 keeps its unticked entry because
      changes 5 and 6 are not in the code, and the index's own rule is that a tick
      means every change the spec asked for is in the code and grep-verified. Spec 14
      keeps its own entry unticked: Section D is now run, but Section A's formatter
      check, build-warning check and baseline reconciliation are not, and neither are
      Section E's standards-document and `CLAUDE.md` reviews. Specs 05 and 10 are now
      ticked, because the greps in Section C no longer contradict the tick.
- [x] Every row in the "Decisions to make before starting" table in
      [00-index.md](00-index.md) records the decision actually taken, not just the
      recommended default. That includes spec 12's version-header Option A or B, spec
      11's container-sharing option for the rate-limited fixture, spec 06's
      replace-versus-fix decision, spec 09's `TimeProvider` decision, and the `MetaField`
      init-tests question in spec 08. The table is now headed "Decisions — settled" and
      carries all five. The `MetaField` row records a deferral rather than a decision,
      which is itself the outcome and is stated as such in
      [08-fixture-architecture.md](08-fixture-architecture.md).
- [x] Spec 11's gated Change 3 is recorded as deliberately deferred, with the
      420-second trigger stated and the running log of measured integration run times
      started. The log now carries four rows, the latest two being this sweep's 143 s
      and 144 s. The trigger is not close.
- [ ] Where the code diverged from an audit document, the audit document is corrected.
      The rule in [00-index.md](00-index.md) is that the code wins and the spec gets
      corrected; a stale finding is worse than no finding, because the next reader
      spends time re-deriving something that is no longer true. — **done for
      everything this sweep measured**, which is the C-section invariants and the
      specs closed here: the `allowEmptyBody` exemption, the job-registration rule in
      C5, D4's test name, D9's runnability, spec 05's over-broad query-builder
      wildcard, spec 07's stale claim about the password default and its stale
      "roughly 40 files" fallout estimate, spec 10's assumption that change 3 spanned
      13 strategies rather than 20, and spec 08's unreconcilable factory count are all
      corrected in place. It is not ticked because the sweep did not re-read the
      audit's other eighteen documents against the code.
- [ ] The four standards documents
      ([standards/01](../standards/01-unit-testing-standard.md),
      [standards/02](../standards/02-integration-testing-standard.md),
      [standards/03](../standards/03-assertion-catalogue.md),
      [standards/04](../standards/04-test-data-and-fixtures.md)) match what the code now
      does. They are the part of this doc set that outlives the remediation, so any
      example in them that no longer compiles is a defect. — not reviewed in this
      sweep. This is the highest-value remaining item: the standards outlive the
      remediation and the specs do not.
- [ ] The production defects in spec 13 have their own tickets, closed, and are
      referenced from the release notes rather than from a testing PR. — no ticket
      state is observable from the tree.
- [ ] The follow-up ticket for `SessionQueryBuilder.CombineSpecification` being called
      with a null specification is filed and linked from spec 13. — not filed; carried
      as open follow-up 8 in [../90-remediation-plan.md](../90-remediation-plan.md).
- [ ] `CLAUDE.md` still describes the testing rules accurately, in particular the
      unit-versus-integration boundary and the `X-Api-Version` claim if spec 12 took
      Option B. — not reviewed in this sweep. Spec 12 took Option A, so the
      `X-Api-Version` paragraph needs no change, but that is the only part checked.

## Results (2026-08-24)

### Suites

| Suite | Result |
| --- | --- |
| Unit | 7,693 passed, 6 skipped, 7,699 total |
| Integration | 1,936 passed, 0 skipped |
| Integration, second consecutive run | 1,936 passed, 0 skipped |
| Integration wall clock | 2 m 23 s, then 2 m 24 s — down from 3 m 41 s |

The determinism check is the one that carries weight: two consecutive full runs, the
same counts both times, on a suite whose fixtures were rebuilt by specs 01, 02 and 11.

### Measured invariants

Every row was measured from `apps/backend/` on 2026-08-24, the last three re-measured
after spec 05 change 4, spec 07 change 1 and spec 10 change 3 landed. Nineteen
measurements: 18 as expected — two of those with a deliberate exception recorded
below — and 1 failing.

| Invariant | Expected | Measured |
| --- | --- | --- |
| status-only `ShouldBeProblem(HttpStatusCode.X)` | 0 | 0 |
| typed `ShouldBeProblem<TException>` | many | 501 |
| `allowEmptyBody` call sites | 1 | 0 — see C2 |
| `StatusCode.Should().BeOneOf` | 0 | 0 |
| numeric status literals (`Status.Should().Be(404)`) | 0 | 0 |
| `Mock<` in `tests/Integration` | 0 | 0 |
| `ASPNETCORE_ENVIRONMENT` in `src/Modules` | 1 | 1 |
| `ShouldBeLocalizedForCulture` | 0 | 0 |
| `Thread.CurrentThread.Current*` in tests | 0 | 0 |
| `Faker _faker = new()` | 0 | 0 |
| `GetProperty("literal")` | 0 | 0 |
| internal builders | 0 | 0 |
| doc comments on test methods | 0 | 0 |
| raw `new BadRequestException("literal")` in `src/` | 0 | 0 |
| `Thread.Sleep` / `Task.Delay` in tests | 0 | **1** — deliberate, see below |
| `Environment.SetEnvironmentVariable` in `tests/Integration` | 0 | **2 files** — deliberate, see below |
| `BeGreaterThanOrEqualTo` in `tests/` | 1 | 1 |
| `GetByIdAsync(It.IsAny<Guid>()` in mock factories | 0 | 0 |
| `MockBehavior` stated or documented | present | **absent** — C7 fails, by decision |

### The two deliberate exceptions

Both are fixture-level, not test-level. Neither is a test tolerating
non-determinism, which is what the invariants were written to catch, and neither
should be "fixed" by a future reader.

1. **The one remaining delay is a container readiness poll.**
   `tests/Integration/Common/Fixtures/TestPostgresContainer.cs:127` waits 250 ms
   between attempts while the Postgres container accepts connections. It is in the
   fixture's startup path, runs once per session, and gates on an external process
   this suite does not control. No test asserts through it.
2. **The two env-mutating files are `ApiFixture.cs` and `CorsApiFixture.cs`.**
   `CorsApiFixture` exists precisely because `DASHBOARD_ORIGIN` must be set *before*
   host construction — CORS policy is read at startup, so a test that sets the
   variable after the host is built configures nothing. It sets it from a
   `ConfigureEnvironment()` override (`CorsApiFixture.cs:37-43`) and restores the
   previous value on dispose (`:48`), so the mutation does not outlive the fixture.
   That ordering requirement is the whole reason a second fixture exists rather than a
   per-test override, and removing the `SetEnvironmentVariable` call would silently
   disable spec 12's preflight coverage rather than break it.

### Reopened, and since closed

| Spec | Change | Evidence it is closed |
| --- | --- | --- |
| [05](05-outcome-assertions.md) | 4 — exact counts | `grep -rn "BeGreaterThanOrEqualTo" tests/` → 1, the `TimeSpan.Zero` duration at `RateLimitingExtensionTests.cs:176`; 33 sites converted across ~26 files, 3 of them tightened |
| [07](07-mock-discipline.md) | 1 — mock read defaults | `grep -rn "GetByIdAsync(It.IsAny<Guid>()" tests/Unit/Common/Mocks/` → 0; 48 defaults removed across 18 mocks, 20 red tests given the arrangement they needed |
| [10](10-duplication-to-theories.md) | 3 — strategy contract theory | `ExceptionStrategyContractTests.cs` — 3 theories, 2 facts, 59 rows, 20 strategies; strategy facts 119 → 58; D9 resolves |

### Still open

| Spec | Change | Evidence |
| --- | --- | --- |
| [07](07-mock-discipline.md) | 5 — dead helpers | 97 uncalled helpers, 26 `new Mock<IFormFile>`, 155 raw mocks |
| [07](07-mock-discipline.md) | 6 — aggregate read defaults | not started; gated on `MockBehavior.Strict`, which is C7's first clause |

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
      — **suites, run time and counts recorded; build warnings, formatter and the
      baseline reconciliation not run**
- [x] Section B — integration suite run twice back to back with identical results and
      identical executed test sets — verified 2026-08-25 by diffing the executed
      test-name sets and outcomes from `.trx`, not by pass count: unit 7,693 passed /
      6 skipped and integration 1,936 passed, identical on both runs
- [x] Section B — the name-set diff surfaced one non-deterministic test *name*:
      `PublicGetOwnArticleFavoritesEndpointV1Tests` embedded `Guid.NewGuid()` in a
      `[MemberData]` row, so its display name changed every run. Behaviour was
      deterministic, but name-keyed CI history and flaky-test detection could never
      track it. The row was split into its own `[Fact]`, moving the identifier into
      the body where it cannot reach the display name
- [x] Section C1 — no direct culture assignment outside `CultureScope`, and
      `CultureScope` covers both cultures
- [x] Section C2 — no status-only `ShouldBeProblem` anywhere, the `[Obsolete]` shim
      deleted, no hardcoded expected detail, and the `allowEmptyBody` opt-in has no
      call sites at all
- [x] Section C3 — exactly one `BeGreaterThanOrEqualTo` remains, on a duration, and the
      three sites that a straight conversion would have left unfalsifiable were
      tightened instead
- [x] Section C4 — no test constant duplicates a production numeric value, and the OTP
      lockout boundary test exists
- [x] Section C5 — the test host removes `IHostedService` registrations, and the
      surviving registration facts are compatible with that removal
- [x] Section C6 — every `CreateScope` in the two base classes is disposed
- [ ] Section C7 — `MockBehavior` is stated or documented, and the permissive defaults
      spec 07 named are gone — **the defaults are gone, 48 removed across 18 mocks;
      `MockBehavior` is still absent and stays that way until spec 07's deferred strict
      step, which is why this box is unticked while its second clause holds**
- [x] Section D — all ten mutations produce their named failure, each reverted with a
      clean tree — run 2026-08-25. `git status --porcelain -- src/` returned nothing
      after every revert, and both suites were re-run green afterwards. Four rows
      needed correcting against the code and are corrected above: D3's file path, D7's
      expected count, D8's mutation, and D10's test name
- [x] Section D — the sweep produced one genuine finding. D2's
      `GetAllSessions_FilterByStatusAndIpAddress_ReturnsFilteredResults` seeded a
      non-matching session that differed in both filtered dimensions at once, so it
      could not detect either filter being dropped. It now seeds a same-IP-but-revoked
      session and an active-on-another-IP session, and fails under the mutation
- [ ] Section E — all spec checklists ticked or deviations recorded, decisions written
      down, audit documents corrected where the code diverged — **deviations and
      decisions recorded; the four standards documents and `CLAUDE.md` not re-read
      against the code**
