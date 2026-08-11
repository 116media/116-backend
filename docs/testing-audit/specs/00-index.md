# Testing Remediation — Implementation Specs

Read [../00-overview.md](../00-overview.md) for the measured evidence and
[../90-remediation-plan.md](../90-remediation-plan.md) for why the phases are
ordered as they are. These specs are the implementation contract: each one is a
self-contained unit of work with an exact change list, a test plan, and a
checklist.

Work in the order below. Specs 01–03 unblock everything else, because until the
test host matches production and tests stop leaking state into each other, no
result from the later specs can be trusted.

| # | Spec | Covers | Phase |
| --- | --- | --- | --- |
| 01 | [01-test-host-fidelity.md](01-test-host-fidelity.md) | Quartz removal, `.env` precedence, `IHostEnvironment`, pooling, interceptors, the JWT round trip | 1–2 |
| 02 | [02-test-isolation.md](02-test-isolation.md) | Stub reset contract, DI scope disposal, culture scoping, env-var collections, per-instance `Faker` | 1–4 |
| 03 | [03-constant-aliasing.md](03-constant-aliasing.md) | Alias production constants; fix the OTP lockout boundary | 1 |
| 04 | [04-error-assertion-discipline.md](04-error-assertion-discipline.md) | Status + `Title` + exact localized `Detail` at 482 sites, opt-in empty-body, `BeOneOf` | 2 |
| 05 | [05-outcome-assertions.md](05-outcome-assertions.md) | State transitions, `IsSuccess`, `BeOfType`, `NotBeNull`, `BeGreaterThanOrEqualTo`, query-builder predicates | 3 |
| 06 | [06-localization-testing.md](06-localization-testing.md) | Replace 104 self-comparing tests with a resource-completeness theory; fix `LocalizerFactory` | 3 |
| 07 | [07-mock-discipline.md](07-mock-discipline.md) | Blanket `It.IsAny` defaults, the password-service default, `It.Is<T>`, assertion-free tests | 3 |
| 08 | [08-fixture-architecture.md](08-fixture-architecture.md) | Public builders, dead-member deletion, layering rule, `nameof` reflection | 4 |
| 09 | [09-time-and-determinism.md](09-time-and-determinism.md) | `TimeProvider` in `src/`, `FakeTimeProvider` in tests, sleep removal, the 2030 assertion | 4 |
| 10 | [10-duplication-to-theories.md](10-duplication-to-theories.md) | Collapse the top clusters into `[Theory]` + `[MemberData]` | 4 |
| 11 | [11-suite-performance.md](11-suite-performance.md) | Container flags, assembly fixture, collection sharding, second container removal | 4 |
| 12 | [12-contract-coverage.md](12-contract-coverage.md) | Version header, CORS preflight, rate-limit policy registration | 5 |
| 13 | [13-production-defects.md](13-production-defects.md) | The real bugs the audit surfaced — parent scoping, culture-sensitive `ToLower()`, three un-localized guards | any |
| 14 | [14-verification-checklist.md](14-verification-checklist.md) | Final sweep: grep-provable invariants, suite health, doc closure | last |

## Global progress

All fourteen specs have been implemented and the verification sweep has been run,
including its behavioural section. Both suites are green at **7,693 unit (6 skipped,
7,699 total) / 1,936 integration** tests, and the integration suite ran twice back to
back with identical results in 2 m 23 s and 2 m 24 s, down from 3 m 41 s. Each spec
carries an "Implementation notes" section recording where the code disagreed with the
spec.

Section D's ten mutations were run on 2026-08-25 and are the first evidence in this
doc set that the suite fails when production behaviour changes, rather than that it is
shaped correctly. Nine of the ten discriminated as specified. The tenth, D2, exposed a
multi-filter integration test that could not fail; it is fixed, and the write-up is in
[13-production-defects.md](13-production-defects.md). Three rows of the mutation table
were themselves wrong and are corrected in
[14-verification-checklist.md](14-verification-checklist.md) — a file path that does
not exist, an expected failure count the code cannot produce, and a mutation that
changed a constant both production and the test read, so nothing could fail.

A tick below means every *change* the spec asked for is in the code and grep-verified.
It does not mean every line of that spec's checklist is ticked: several landed specs
still carry an unticked process item — a suite run repeated back to back, a ticket to
file — that is not a code change and cannot be confirmed from the tree. Those are
listed under open follow-ups rather than quietly ticked.

The three changes an earlier pass recorded as outstanding — spec 05 change 4, spec 07
change 1 and spec 10 change 3 — have since landed and are measured in
[14-verification-checklist.md](14-verification-checklist.md). One spec keeps an
unticked box below, rather than a tick with a footnote: spec 07's changes 5 and 6 are
not in the code at all, change 6 deliberately so.

- [x] 01 — Test host fidelity
- [x] 02 — Test isolation
- [x] 03 — Constant aliasing
- [x] 04 — Error assertion discipline
- [x] 05 — Outcome assertions — all six changes in the code; change 4 converted all 33
      `BeGreaterThanOrEqualTo(n)` sites and tightened three assertions past a straight
      conversion, leaving one `TimeSpan` exemption suite-wide. Change 2's own box stays
      unticked in the spec: the named sites were converted, but the 83 surviving
      `BeOfType<T>` assertions were not re-audited against their declared types
- [x] 06 — Localization testing
- [ ] 07 — Mock discipline — changes 1 through 4 landed, plus 27 swallowed-exception Act
      phases fixed; 48 blanket read defaults removed across 18 mock factories.
      **Changes 5 and 6 outstanding**: the dead-helper deletion was not attempted, and
      change 6 is gated on `MockBehavior.Strict` by design
- [x] 08 — Fixture architecture — changes 1, 2 and 3 done; change 4's `nameof`
      conversion done suite-wide, its stricter "no `SetValue` in `tests/Unit`" clause
      deliberately not met because it contradicts the same change's own rule
- [x] 09 — Time and determinism
- [x] 10 — Duplication to theories — changes 1, 2, 4, 5 and 6 landed, 222 facts into 39
      theories carrying 336 rows; change 3 followed with
      `ExceptionStrategyContractTests`, 3 theories and 2 facts over 59 rows and 20
      strategies, and the facts under `Handlers/Strategies/` fell 119 → 58
- [x] 11 — Suite performance
- [x] 12 — Contract coverage
- [x] 13 — Production defects
- [ ] 14 — Verification — run 2026-08-24, C3 and C7 re-measured after the three
      closures; **Section D run in full 2026-08-25**. Sections A through D recorded, 18
      of 19 measured invariants holding — the survivor is `MockBehavior`, which is
      absent by design until spec 07 change 6. All ten mutations were applied, run and
      reverted with `src/` verified pristine between each: nine discriminated, D2
      surfaced a test that could not fail and is now fixed, and four rows were
      corrected against the code (D3's file path, D7's expected count, D8's mutation
      and D10's test name). The entry stays unticked for Section A's formatter,
      build-warning and baseline-reconciliation items and Section E's
      standards-document review

Five production fixes landed alongside, all under spec 13's mandate and none anywhere
else: the child-entity parent scoping (thirteen handlers, not the two the audit named),
the culture-sensitive `ToLower()` in the session status filter, the three
un-localized `ForceUnpromote` guards, the bodiless-400 on nine upload endpoints, and
the `TimeProvider` seams spec 09's decision authorised. Spec 04's proposed sixth
change — a `code` extension on `ProblemDetails` — was built, rejected in review and
fully reverted; no line of it remains.

Open follow-ups that implementation surfaced but did not fix are listed in
[../90-remediation-plan.md](../90-remediation-plan.md).

## Decisions — settled

All five were taken on 2026-08-22, four of them at the recommended default. The
exception is spec 04: the recommended option there was implemented, rejected in
review and reverted, and the row below records the decision actually taken. They
are binding for the implementation; a spec that wants to deviate records the
reason in its own implementation notes.

| Decision | Spec | Taken |
| --- | --- | --- |
| Replace the 104 localization tests with one resource-completeness theory, or fix each in place | 06 | **Replace** — one theory covers all 99 resource files; 104 fixed-in-place tests still cover only the strings they name |
| Introduce `TimeProvider` into `src/` domain methods, or confine the fix to tests | 09 | **Introduce it** — the alternative leaves ~30 tolerance-based assertions and 6 s of sleeps permanently |
| Shard `DatabaseCollection`, or take only the container flags | 11 | **Container flags only** — shard when the 600 s CI budget is actually threatened, with measurements |
| Discriminate errors with a new machine-readable `code` extension on ProblemDetails, or with what the response already carries | 04 | **Use what it already carries** — status + `Title` (`nameof(TException)`) + the exact localized `Detail`, asserted with `Be`. The `code` extension was built and reverted: it was consumed by a substring match, and a third of the entity tokens here are substrings of another, so `"Article"` matched an `ArticleComment` error. No `src/` change is needed |
| Delete `MetaField` init-tests for Mailer (added recently) as the Content/Identity equivalents were in July | 08 | **Keep** — revisit with the team; they were deliberately removed once |

## Implementation order taken

The table order is the reading order, not the build order. Three constraints
reorder it, all of them recorded in the owning specs:

1. **03 before 02 Change 5** — `UserBuilder` truncates a username against
   `TestConstants.User.UserNameMaxLength`; until 03 aliases it, that value is 50
   against a production column of 20.
2. **02 Change 2 before 01 Change 3** — restoring `AddDbContextPool` while ~1,189
   scopes are abandoned exhausts the pool instead of gaining fidelity.
3. **06 before 02 Change 3** — 06 deletes the localization theories that 02
   Change 3 would otherwise convert to `CultureScope`, so doing 02 first wastes
   the edit on files that are about to lose the code being edited.

Executed order: 03 → 02.1–2 → 01.1,2,4,5 → 01.3 → 06 → 02.3–5 → 13 → 04 → 05 →
07 → 08 → 09 → 10 → 11 → 12 → 14.

## Ground rules for every spec

- **A test that cannot fail is not a test.** Before adding or keeping an
  assertion, state what change to `src/` would break it. If nothing would, the
  assertion does not go in.
- **Do not weaken a test to make it pass.** Several changes here will turn tests
  red — the constant aliasing especially. Each red test is a boundary that was
  never being checked; fix the test, not the constant.
- **Never change `src/` to make a test easier**, except where a spec explicitly
  says so (09's `TimeProvider`, 13's production defects). Those are called out
  individually with their reasoning. Spec 04 proposed a third such change, a
  machine-readable `code` extension on ProblemDetails; it was built, rejected and
  reverted, and the spec now discriminates errors using what the response already
  carries.
- **Behaviour must not change.** With the exception of spec 13, every change here
  is to test code or test infrastructure. Any production edit that is not in a
  spec is out of scope and needs its own discussion.
- **Deviations get recorded** in the owning spec's implementation notes, never
  left implicit. If the code disagrees with a spec, the code wins and the spec
  gets corrected.

## What "done" looks like

Both suites green, the full integration suite passing twice back to back with
identical results, and the grep-provable invariants in
[14-verification-checklist.md](14-verification-checklist.md) all holding. The
audit documents stay as the record of why each change was made.

**Measured against that definition on 2026-08-24:** all three clauses hold. Both
suites are green, the integration suite passed twice back to back with identical
results, and C3 now returns its single exempt duration assertion. C7's second clause —
no blanket `It.IsAny` read default in a mock factory — holds at zero. Its first
clause, `MockBehavior` stated or documented, is still absent, and that is the one
invariant this doc set records as unmet by decision rather than by omission: spec 07's
Scope section rules out changing the behaviour mode while the defaults are being
tightened, and spec 07 change 6 is sequenced behind it.
