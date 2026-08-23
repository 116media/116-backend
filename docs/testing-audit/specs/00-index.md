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

- [ ] 01 — Test host fidelity
- [ ] 02 — Test isolation
- [ ] 03 — Constant aliasing
- [ ] 04 — Error assertion discipline
- [ ] 05 — Outcome assertions
- [ ] 06 — Localization testing
- [ ] 07 — Mock discipline
- [ ] 08 — Fixture architecture
- [ ] 09 — Time and determinism
- [ ] 10 — Duplication to theories
- [ ] 11 — Suite performance
- [ ] 12 — Contract coverage
- [ ] 13 — Production defects
- [ ] 14 — Verification

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
