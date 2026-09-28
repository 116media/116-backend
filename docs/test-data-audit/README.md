# Test-data layer audit

An audit of how the test suites obtain their data: through the `<M>.TestData` builders and factories,
or by hand. It exists because the factory layer had drifted — 97 of 273 factory methods are called by
fewer than three files, while 66 builder chains are repeated three or more times with no factory at
all, and several suites bypass the layer entirely.

## The rule being audited

Quoted from the layer's own documentation, `Storage.TestData/Builders/Entities/FileBuilder.cs`:

> Use it for any shape a test needs; FileFactory only names chains three or more tests share.

Three consequences, and this audit measures each:

1. A builder is the default way to shape test data. Constructing an entity or a request record inline
   is a defect unless the test is the thing's own guard test.
2. A factory exists **only** to name a chain three or more tests share. Fewer callers than that and it
   should be inlined or deleted; a chain repeated three times with no factory is a missing one.
3. A domain guard test — `UserEntityTests`, `NotificationEntityTests` — **should** call
   `XEntity.Create(...)` directly, because that factory is the subject under test. Mechanically
   "fixing" those sites would be wrong, which is why [13-correctly-direct.md](13-correctly-direct.md)
   is part of this audit rather than an afterthought.

## Headline numbers

| Measure | Value |
| --- | --- |
| Test files in the repository | 1,319 (226,973 lines) |
| Builders | 131 |
| Factory methods | 273 |
| Factory methods with fewer than 3 calling files | **95** (Content 62 · Identity 23 · Storage 10) |
| Of those, would reach 3+ if the hand-rolled equivalents adopted them | **6** |
| Builder chains repeated 3+ times with no factory | **66 chains, 312 sites** |
| Entities constructed inline where a factory or builder exists | **46 in Identity infrastructure alone** |
| Byte-identical private helpers found duplicated across files | **6** (Mailer notifications) + **2** (Identity session export) |
| Defects found that are not test-data issues | **5** — see [12-defects-found.md](12-defects-found.md) |

## The documents

| File | Contents |
| --- | --- |
| [01-census.md](01-census.md) | Factory usage census: per module, per bucket, and the 6 that adoption would rescue |
| [02-identity-infrastructure.md](02-identity-infrastructure.md) | Identity `Infrastructure/` + `Domain/` test suites |
| [03-content-editorial.md](03-content-editorial.md) | Content `Application/Editorial/` + `Domain/` test suites |
| [04-storage-mailer-shared-e2e.md](04-storage-mailer-shared-e2e.md) | Storage, Mailer, shared and end-to-end suites |
| [05-identity-application.md](05-identity-application.md) | Identity `Application/` test suites |
| [06-content-commerce.md](06-content-commerce.md) | Content commerce, catalogue, interactions, infrastructure |
| [10-missing-factories.md](10-missing-factories.md) | Every chain repeated 3+ times with no factory, with the signature to add |
| [11-dead-factories.md](11-dead-factories.md) | Factory methods with no caller and no hand-rolled twin — safe to delete |
| [12-defects-found.md](12-defects-found.md) | Bugs found while reading: a broken factory, a test that asserts nothing, and three more |
| [13-correctly-direct.md](13-correctly-direct.md) | Sites that look like defects and are not. Read before any bulk change |
| [14-method-and-coverage.md](14-method-and-coverage.md) | How the audit was done, what it covers, and where it is thin |
| [15-work-plan.md](15-work-plan.md) | Ordered plan, by ratio of sites fixed to risk taken |
| [16-implementation-log.md](16-implementation-log.md) | What was applied, what the compiler measured differently, what was left |

## Status

Five areas were audited by reading the suites. Two were still in progress when this was first written;
their documents say so at the top. Every finding carries its evidence, and the numbers were re-verified
by hand where a claim was load-bearing.

**The whole work plan is implemented**: five defects fixed, 19 dead members deleted, 44 new factory and
builder files, roughly 970 sites adopted, ~11,000 lines of dead `using` directives removed, all 10,681
tests green. Four findings measured differently once the compiler saw them, and one proposal was
rejected on reading — see [16-implementation-log.md](16-implementation-log.md).
