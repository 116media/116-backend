# Testing Audit

An independent, evidence-based review of `tests/` — what is wrong, why it matters,
and exactly what to do instead. Every claim in this set carries a `file:line`
reference and was verified against the code before being written down.

## How this audit was produced

Four reviewers read the test code and the production code it exercises. None of
them was allowed to read `docs/`, so nothing here is a restatement of an existing
convention — it is a fresh judgement from the code and from general C#/.NET
practice. Claims that could not be proven were dropped rather than softened.

The rule applied throughout: **a test earns its place by being able to fail.** A
test that cannot fail is worse than no test, because it occupies the space where a
real one would go and reports confidence that does not exist.

## The measured shape of the suite

| Metric | Value |
| --- | --- |
| Test files | 1,418 (`Unit` 870, `Integration` 375, `Fixtures` 173) |
| Lines of test code | ~229,000 |
| Test methods | ~8,570 (`[Fact]` 8,272, `[Theory]` 298) |
| Integration test methods | 1,879 across 357 files |
| Integration classes in one xUnit collection | 356 |
| `It.IsAny<>` usages | 3,072 |
| `It.Is<T>(…)` usages, against those 3,072 `It.IsAny` | 141 |
| `Should().NotBeNull();` as a whole statement | 1,172 |
| `DateTime.UtcNow` / `.Now` in tests | 372 |
| Reflection writes into production state | 214 |
| `[MemberData]` usages | 3 |

Two numbers frame the whole report. **8,272 facts to 298 theories** is a
copy-paste ratio, not a style choice — and copy-paste hides gaps, because a
duplicated block is never audited for the case it forgot. **356 test classes in a
single collection** means the integration suite cannot run in parallel at all,
against a CI session timeout it is already approaching.

## The headline conclusion

The suite is broad and, structurally, better built than most. File-level coverage
of `src/` is genuinely high: 337 of 352 handlers, 158 of 162 validators, and 57 of
63 entities have a matching test file. The boundary discipline in the integration
project is close to ideal — zero mocked repositories, zero mocked `DbContext`s,
zero reflection into private members.

**The problem is not breadth. It is that a large fraction of the assertions cannot
fail.** The audit found, and proved:

- 104 localization test files whose expected value is produced by the same object
  the code under test uses, so an emptied French resource file passes all of them
- 21 of 22 state-transition handler tests that never assert the state transition —
  you can delete `article.Publish()` and ship green
- A live Quartz scheduler inside the integration host firing every 15 seconds
  against the same database the tests assert on
- A gitignored `.env` that overwrites the test fixture's environment, so a
  developer machine and CI boot materially different applications
- Consequently, no test anywhere authenticates with a token the application
  actually issued

## Severity summary

| # | Finding | Severity | Doc |
| --- | --- | --- | --- |
| 1 | Background jobs run live in the test host | Critical | [integration/01](integration/01-background-jobs-in-the-test-host.md) |
| 2 | `.env` clobbers the test environment | Critical | [integration/02](integration/02-environment-divergence.md) |
| 3 | Tautological assertions (localization, `IsSuccess`, `BeOfType`) | Critical | [unit/01](unit/01-assertions-that-cannot-fail.md) |
| 4 | State transitions are never asserted | Critical | [unit/02](unit/02-state-transition-blindness.md) |
| 5 | No test uses an application-issued token | High | [integration/03](integration/03-authentication-contract-hole.md) |
| 6 | Test host diverges from production wiring | High | [integration/04](integration/04-production-wiring-divergence.md) |
| 7 | Shared mutable stubs leak between tests | High | [integration/05](integration/05-shared-mutable-state.md) |
| 8 | Culture and environment leak across parallel tests | High | [unit/03](unit/03-culture-and-environment-leakage.md) |
| 9 | Mock verification proves calls, not outcomes | High | [unit/04](unit/04-mock-verification-discipline.md) |
| 10 | Fact/Theory duplication hides coverage gaps | Medium-High | [unit/05](unit/05-duplication-and-theories.md) |
| 11 | Suite is fully serialised against a timeout cliff | High | [integration/06](integration/06-parallelism-and-runtime.md) |
| 12 | ~1,189 DI scopes created and never disposed | Medium | [integration/07](integration/07-lifecycle-and-scope-leaks.md) |
| 13 | Assertion helper silently downgrades | Medium | [integration/08](integration/08-assertion-escape-hatches.md) |
| 14 | Wall-clock dependence, no clock seam | Medium | [unit/06](unit/06-time-and-determinism.md) |
| 15 | Reflection used to build unreachable states | Medium | [unit/07](unit/07-reflection-in-tests.md) |
| 16 | Untested HTTP contract surface | Low | [integration/09](integration/09-contract-coverage-gaps.md) |

## Documents in this set

**The problems**, each with evidence, failure mode, and an exact fix:

- [unit/](unit/) — seven documents on the unit suite
- [integration/](integration/) — nine documents on the integration suite

**The target state** — what "correct" looks like here, written as standards a
reviewer can apply:

- [standards/01-unit-testing-standard.md](standards/01-unit-testing-standard.md)
- [standards/02-integration-testing-standard.md](standards/02-integration-testing-standard.md)
- [standards/03-assertion-catalogue.md](standards/03-assertion-catalogue.md)
- [standards/04-test-data-and-fixtures.md](standards/04-test-data-and-fixtures.md)

**Closing:**

- [90-remediation-plan.md](90-remediation-plan.md) — phased, ordered, costed
- [91-what-works-well.md](91-what-works-well.md) — the parts worth protecting

## Reading order

Start with [90-remediation-plan.md](90-remediation-plan.md) if you want the
ordered work list. Start with [unit/01](unit/01-assertions-that-cannot-fail.md)
and [integration/01](integration/01-background-jobs-in-the-test-host.md) if you
want to understand the two failures that most distort the suite's reported health.
