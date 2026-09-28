# Factory usage census

Counted across all 1,319 test files. "Callers" means *distinct files* that name the method, excluding
the file that declares it.

## Per module

| Module | **Under 3 callers** | 3+ | Total |
| --- | --- | --- | --- |
| Content | **62** | 123 | 185 |
| Identity | **23** | 37 | 60 |
| Storage | **10** | 18 | 28 |
| Mailer | — | — | 0 |
| **Total** | **95** | **178** | **273** |

By caller count: 19 have none, 48 have one, 28 have two.

> **Counting correction.** A first pass reported 97 and 28-with-no-caller by matching `Class.Method(`
> only. 16 of the 273 methods are **extension methods** on `Mock<T>`, invoked as
> `_orderPaymentFactoryMock.SetupGetByOrderId(...)` — the class name never appears at the call site.
> Nine of them were miscounted as uncalled. Any future census must match both syntaxes; see
> [14-method-and-coverage.md](14-method-and-coverage.md).

Mailer had no factories at all when this was written; its five are proposed in
[10-missing-factories.md](10-missing-factories.md).

## The 95, classified

| Class | Count | What to do |
| --- | --- | --- |
| Would reach 3+ callers if the hand-rolled equivalents adopted them | 6 | Adopt — see below |
| Wrap a domain factory whose only direct callers are its own guard tests | ~40 | Leave. The guard tests are right to call the factory directly |
| Name a shape no test asks for, and nothing hand-rolls it | ~50 | Inline at the call site, or delete — see [11-dead-factories.md](11-dead-factories.md) |

## The 6 that adoption rescues

| Factory | Callers today | Hand-rolled elsewhere | Would total |
| --- | --- | --- | --- |
| `CategoryFactory.CreateGossip` | 2 | 2 | 4 |
| `ArticleFactory.CreateRejected` | 1 | 2 | 3 |
| `ArticleFactory.CreateArchived` | 1 | 2 | 3 |
| `MockVerifyPaymentFactory.SetupVerifyAsync` | 1 | — | — |
| `MockVerifyPaymentFactory.SetupVerifyAsyncThrows` | 0 | 0 | — |
| `MockVerifyPaymentFactory.VerifyVerifyCalled` | 0 | 0 | — |

> The three `MockVerifyPaymentFactory` rows are struck through by the counting correction above:
> `SetupVerifyAsync` is called at `AdminVerifyPaymentHandlerTests.cs:64`, and reading the three
> hand-rolled `.Verify(...)` calls in that file shows **none of them matches** the factory member —
> two are `Times.Never` and one asserts specific arguments, strictly stronger than the member's
> all-`It.IsAny` signature. So the honest figure is **3 adoptable factories, not 6**, and the other two
> `MockVerifyPaymentFactory` members are dead. This is what reading catches and counting cannot.

`MockOrderPaymentFactory.SetupGetByOrderId` in fact has **6 call sites across 3 files** and
`SetupGetByOrderIdNotFound` has 2 — both were miscounted, not under-used.

## What the census does not measure

A factory can have three callers and still be wrong — for example
`VideoFactory.CreateWithCategory` has 12 and does not do what its name says
([12-defects-found.md](12-defects-found.md)). Caller count is a smell, not a verdict.
