# Remediation plan

Ordered by return on effort, with dependencies respected. Each phase is
independently shippable and leaves the suite greener than it found it.

The ordering principle: **fix what is actively lying or actively flaking before
fixing what is merely weak.** A test that fails randomly costs the team more than a
test that silently proves nothing, and a test that proves nothing costs more than
one that is merely duplicated.

## Phase 1 — Stop the bleeding (days, not weeks)

These four are the only findings where something is wrong *right now* rather than
fragile. Nothing else should start before them.

| # | Change | Files | Doc |
| --- | --- | --- | --- |
| 1.1 | Remove the Quartz hosted service from the test host | 1 | [integration/01](integration/01-background-jobs-in-the-test-host.md) |
| 1.2 | Stop `.env` clobbering the fixture environment; read `IHostEnvironment` | 5 | [integration/02](integration/02-environment-divergence.md) |
| 1.3 | Alias production constants; fix the OTP boundary test | 10 | [fixtures/01](fixtures/01-constant-drift.md) |
| 1.4 | Reset stubs between tests via `IResettableStub` | 5 | [integration/05](integration/05-shared-mutable-state.md) |

**Why these first.** 1.1 is a live race — a background dispatcher fires roughly a
dozen times per run against the same rows four tests assert on. 1.2 means local and
CI currently boot different applications, so every other result is suspect until it
is fixed. 1.3 is a security boundary (the OTP brute-force lockout) with no working
test. 1.4 lets one failing test cascade into the next, which turns a single red
result into a misleading pair.

**Expect 1.3 to turn some tests red.** The length constants have drifted upward, so
"invalid at max + 1" tests have been asserting against the wrong edge. Each failure
is a boundary that was never being tested; fix the test, not the constant.

## Phase 2 — Close the contract holes (1–2 weeks)

Depends on 1.2 being done.

| # | Change | Doc |
| --- | --- | --- |
| 2.1 | Delete `OverrideJwtAuthentication`; add the login-token round trip | [integration/03](integration/03-authentication-contract-hole.md) |
| 2.2 | Restore production DbContext pooling and explicit interceptor registration | [integration/04](integration/04-production-wiring-divergence.md) |
| 2.3 | Replace the two `Moq`-based seeding tests with a real Development host | [integration/04](integration/04-production-wiring-divergence.md) |
| 2.4 | Convert all 482 `ShouldBeProblem` call sites to `ShouldBeProblem<TException>` with a resolved localized detail | [integration/08](integration/08-assertion-escape-hatches.md) |
| 2.5 | Make empty-body tolerance opt-in in `ShouldBeProblem` — **done** | [integration/08](integration/08-assertion-escape-hatches.md) |

**2.4 is the highest-yield single change in the audit.** Today 300 of the 483 calls
assert a status and nothing more, and 182 pass a string to an `[Obsolete]`
substring-matching shim. Pinning the status, the ProblemDetails `Title` and the exact
localized `Detail` converts the suite's largest blind spot into real discrimination
and will immediately expose the wrong-reason passes — the five tests asserting a 404
that their named branch cannot produce. Budget time to fix what it surfaces.

**2.4 needs no production change.** An earlier plan added a machine-readable `code`
extension to ProblemDetails; it was implemented, rejected and fully reverted, because
the code was consumed with a substring match and a third of this codebase's entity
tokens are substrings of another — `"Article"` matched a response whose error was
`ArticleComment`. The detail assertion uses exact equality and reuses what the
application already emits. See
[specs/04](specs/04-error-assertion-discipline.md).

**2.4 starts from a red suite.** 169 of the 182 string arguments are entity names and
resource keys written against the reverted `code` extension; with `code` gone they
match nothing in a French detail. Converting them is the first tranche, and the suite
gets greener as the sweep progresses.

## Phase 3 — Make unit assertions mean something (2–3 weeks)

| # | Change | Scale | Doc |
| --- | --- | --- | --- |
| 3.1 | Assert state transitions and domain events in handler tests | 21 files | [unit/02](unit/02-state-transition-blindness.md) |
| 3.2 | Fix or replace the 104 localization tests | 104 files | [unit/01](unit/01-assertions-that-cannot-fail.md) |
| 3.3 | Adopt `CultureScope`; join the env-var collection | 108 files | [unit/03](unit/03-culture-and-environment-leakage.md) |
| 3.4 | Tighten mock defaults; `It.Is<T>` where arguments matter | 3 + ~40 files | [fixtures/05](fixtures/05-mock-defaults-and-dead-helpers.md) |
| 3.5 | Replace `BeGreaterThanOrEqualTo(N)` with `Be(N)` | 33 sites | [standards/03](standards/03-assertion-catalogue.md) |
| 3.6 | Make the 5 `NotBeNull`-only query-builder test files evaluate predicates | 5 files | [standards/03](standards/03-assertion-catalogue.md) |

Do 3.2 and 3.3 as one change — fixing either alone leaves localization assertions
at the mercy of ambient thread state. The single best version of 3.2 is to delete
all 104 and write one resource-completeness theory that asserts every neutral key
exists in `en` and `fr`; that catches every missing translation, which the 104
currently catch none of.

Expect 3.4 to surface missing arrangements across roughly 40 article handler test
files. **That surfacing is the deliverable**, not a side effect — each one is a
test that was relying on a permissive default rather than stating its dependency.

## Phase 4 — Structural (3–4 weeks, do last)

| # | Change | Doc |
| --- | --- | --- |
| 4.1 | Dispose DI scopes in the integration base classes | [integration/07](integration/07-lifecycle-and-scope-leaks.md) |
| 4.2 | Make builders `public`; delete ~191 dead members; write the layering rule | [fixtures/02](fixtures/02-builder-visibility-and-factory-explosion.md) |
| 4.3 | Cache the localizer container; drop the inert `culture` parameter | [fixtures/03](fixtures/03-localizer-factory-defects.md) |
| 4.4 | Per-instance `Faker` randomizers | [fixtures/04](fixtures/04-random-data-determinism.md) |
| 4.5 | Introduce `TimeProvider` in `src/`, `FakeTimeProvider` in tests | [unit/06](unit/06-time-and-determinism.md) |
| 4.6 | Collapse the top duplication clusters into `[Theory]` + `[MemberData]` | [unit/05](unit/05-duplication-and-theories.md) |
| 4.7 | Assembly container fixture; shard `DatabaseCollection`; drop the second container | [integration/06](integration/06-parallelism-and-runtime.md) |

4.1 is cheap and should arguably move earlier — two files, 1,189 call sites fixed
with zero edits at the call sites. It is placed here only because it is not
currently causing failures.

4.7 is the largest change and the one to attempt last. If appetite is low, the
one-line version — tmpfs mount plus `fsync=off` on the Postgres container —
recovers meaningful time at no risk and buys headroom against the 600-second CI
session timeout the suite is walking toward.

## Phase 5 — Coverage gaps

| # | Change | Doc |
| --- | --- | --- |
| 5.1 | Version-header, CORS preflight, and policy-registration tests | [integration/09](integration/09-contract-coverage-gaps.md) |
| 5.2 | `nameof` in the two unsafe reflection sites; replace 138 unit-test reflection writes with builders | [unit/07](unit/07-reflection-in-tests.md) |
| 5.3 | Delete the 6 assertion-free tests or give them assertions | [unit/04](unit/04-mock-verification-discipline.md) |

## Two production defects found along the way

These surfaced during the audit and are **not** test problems. They need their own
tickets:

1. **Child entities are not scoped to their parent.**
   `AdminDeleteArticleCommentHandler` looks up the comment, then separately looks up
   the article and discards the result — it never checks that the comment belongs to
   that article. A moderator can delete any comment under any article id. The same
   shape appears in the package-slot removal handler. No test can catch this today
   because every test passes a matching pair.

2. **Culture-sensitive `ToLower()` in a status filter.** `SessionQueryBuilder`
   lowercases a status string with the current culture before an
   invariant-culture comparison. Under a Turkish locale `"ACTIVE".ToLower()` yields
   a dotless `ı`, the comparison fails, and the filter silently becomes null. The
   `ToLower()` is redundant — the comparison below it is already case-insensitive.

## How to keep this from recurring

The findings cluster into four habits, and each has a standard in this doc set:

- Assertions that cannot fail → [standards/03](standards/03-assertion-catalogue.md),
  governed by one question: *would this still pass if the method body were
  `return default;`?*
- Test-host divergence → [standards/02](standards/02-integration-testing-standard.md):
  replace outbound edges only, never composition
- Duplicated values and shapes → [standards/04](standards/04-test-data-and-fixtures.md):
  alias production constants, compose builders, do not copy
- Shared mutable state → isolation is the framework's job, enforced in the base
  class, not each author's discipline

Adding these to code review is worth more than any single fix above, because every
finding in this audit was introduced one reasonable-looking pull request at a time.
