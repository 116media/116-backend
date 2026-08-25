# Remediation plan

Ordered by return on effort, with dependencies respected. Each phase is
independently shippable and leaves the suite greener than it found it.

The ordering principle: **fix what is actively lying or actively flaking before
fixing what is merely weak.** A test that fails randomly costs the team more than a
test that silently proves nothing, and a test that proves nothing costs more than
one that is merely duplicated.

## Phase status

| Phase | Status |
| --- | --- |
| 1 — Stop the bleeding | **Done.** 1.1–1.4 all landed (specs 01, 02, 03) |
| 2 — Close the contract holes | **Done.** 2.1–2.5 all landed (specs 01, 04) |
| 3 — Make unit assertions mean something | **Partial.** 3.1, 3.2, 3.3, 3.5 and 3.6 landed (specs 05, 06, 02). **3.4 landed in part** — argument matching, the password default and the mock read defaults yes (48 removed across 18 factories); the dead-helper deletion no |
| 4 — Structural | **Partial.** 4.1, 4.3, 4.4, 4.5, 4.6 and 4.7 landed (specs 02, 06, 09, 10, 11). 4.2 is spec 08, changes 1–3 done and change 4 deliberately partial |
| 5 — Coverage gaps | **Done.** 5.1 landed (spec 12). 5.2 landed: `nameof` suite-wide and the builder half under spec 08. 5.3 landed — no assertion-free test remains, and 27 further tests that swallowed their own exception were found and fixed |

All fourteen specs are implemented and the verification sweep in
[specs/14](specs/14-verification-checklist.md) has been run, behavioural section
included. The three changes an earlier pass recorded as outstanding — spec 05 change 4,
spec 07 change 1 and spec 10 change 3 — have since landed and been measured. Two
remain, both in spec 07 and both named above and in
[specs/00-index.md](specs/00-index.md): change 5, the dead-helper deletion, and change
6, which is gated on `MockBehavior.Strict` by design.

The sweep's ten mutations were applied, run and reverted on 2026-08-25, with
`git status --porcelain -- src/` returning nothing after each revert. Nine
discriminated as specified. The tenth produced the one test defect this document
records below, and three of the ten rows turned out to be mis-specified and are
corrected in spec 14.

Eighteen of the nineteen measured invariants hold. The one that fails is C7's
`MockBehavior` clause, and it fails by decision rather than by omission: spec 07's own
Scope section rules out changing the behaviour mode while the defaults are being
tightened, because doing both at once makes the fallout unattributable.

Both suites are green — **7,693 unit (6 skipped, 7,699 total) and 1,936 integration**
— and the integration suite ran twice back to back with identical results, which is
the strongest single claim in this document: it is the check that specs 01, 02 and 11
were built to make passable. The unit total moved 7,695 → 7,693 when spec 10 change 3
replaced 61 duplicated per-strategy facts with 59 contract rows covering 7 strategies
no fact had reached.

The estimates in this document were written before implementation and several of them
are now known to be wrong. Where a spec's implementation notes contradict a number
here, the spec is right — see in particular the container arithmetic in
[specs/11](specs/11-suite-performance.md) and the handler count in
[specs/13](specs/13-production-defects.md).

The integration suite's wall clock went from **3 m 41 s to 2 m 23 s** across this
work, on 1,936 tests, with a second consecutive run at 2 m 24 s. That is 24% of the
600-second CI session timeout this plan warned about, and 34% of spec 11's 420-second
trigger for its deferred sharding change.

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

These surfaced during the audit and are **not** test problems. Both are now fixed under
[specs/13](specs/13-production-defects.md); the second one turned out to be much larger
than described here.

1. **Child entities are not scoped to their parent.**
   `AdminDeleteArticleCommentHandler` looks up the comment, then separately looks up
   the article and discards the result — it never checks that the comment belongs to
   that article. A moderator can delete any comment under any article id. The same
   shape appears in the package-slot removal handler. No test can catch this today
   because every test passes a matching pair.

   **Fixed, and it was thirteen handlers rather than two.** Four more of the same shape
   were found while implementing the fix, and seven more during spec 04's exact-detail
   sweep — the payment trio, both category-pricing handlers, package-slot and item-tier.
   Pinning the exact error detail is what exposed them: a guard that answers with the
   wrong entity's message cannot survive an exact-equality assertion.

2. **Culture-sensitive `ToLower()` in a status filter.** `SessionQueryBuilder`
   lowercases a status string with the current culture before an
   invariant-culture comparison. Under a Turkish locale `"ACTIVE".ToLower()` yields
   a dotless `ı`, the comparison fails, and the filter silently becomes null. The
   `ToLower()` is redundant — the comparison below it is already case-insensitive.

   **Fixed.** `WithStatus` now compares each arm directly with
   `StringComparison.InvariantCultureIgnoreCase` and there is no lowering step left to
   be culture-sensitive. A second instance of the same bug survives elsewhere — see the
   follow-ups below.

Three more production defects were fixed under the same spec's mandate: the three
un-localized `ForceUnpromote` guards, the bodiless 400 on nine of eleven file-upload
endpoints, and the `TimeProvider` seams spec 09's decision authorised.

## What the behavioural sweep changed

[specs/14](specs/14-verification-checklist.md)'s Section D is the only part of this
remediation that proves the suite *fails* when production behaviour changes. Running it
produced one test fix and three corrections to the sweep's own table.

**One test could not fail, and now can.** D2 collapses
`SessionQueryBuilder.CombineSpecification` so each filter overwrites the previous one
instead of composing. It names two integration tests; only one failed.
`GetAllSessions_FilterByStatusAndIpAddress_ReturnsFilteredResults` seeded a single
non-matching session that differed from the filter in both dimensions at once — a
different IP and revoked — so dropping either filter still excluded it. Fixed in
`tests/Integration/Modules/Identity/Application/Session/UseCases/Admin/Queries/GetAllSessions/V1/AdminGetAllSessionsEndpointV1Tests.cs:226-248`,
which now seeds a same-IP-but-revoked session and an active-on-another-IP session
alongside the matching one. Both named tests fail under the mutation and the file is
12 of 12 green without it. Written up in
[specs/13](specs/13-production-defects.md), because it is the integration-side coverage
of the builder that spec's change 3 fixed.

This is the shape to watch for in any multi-condition test: a single negative case that
violates every condition at once proves only that the filter is not absent entirely. A
test with N filters needs N negatives, each excluded by exactly one of them.

**Three rows of the mutation table were wrong, and are corrected in spec 14.**

| Row | What was wrong | Correction |
| --- | --- | --- |
| D3 | Named `Identity/…/Messages/NotFoundErrorMessage.fr.resx`, which does not exist. That folder holds only the authentication, authorization, conflict and validation catalogues | Retargeted at `src/Shared/Shared/Application/Exceptions/Messages/SharedExceptionMessage.fr.resx`, where the French not-found strings live. Against the real file the mutation produced both named failures |
| D7 | Expected all seven `tr-TR` rows of the `WithStatus` theory to fail. Only `ACTIVE` and `EXPIRED` can — Turkish lowercasing differs from invariant only on `I` → `ı`, and the other five spellings have no capital `I` | Count corrected to two, in spec 14 and in spec 13's guidance. The row also now states the faithful mutation: restoring `ToLower()` alone changes nothing, because the `switch`'s arms are all `_ when status.Equals(...)` and ignore the subject |
| D8 | Changed `AuthenticationRateLimitConstants.PermitLimit`, which production registers from and the test reads its expectation from, so nothing could fail. Not a behavioural mutation | Rewritten to decouple the registration instead — `RateLimitPolicies.Authentication`'s permit argument becomes `AuthenticationRateLimitConstants.PermitLimit + 1`. Exactly the `Authentication` row then fails |

None of the three is a suite weakness. D3 and D7 were stale or arithmetically wrong
descriptions of sound tests; D8 was a mutation that changed a shared source of truth
rather than a behaviour, which is a mistake worth naming because spec 03 spent a whole
phase making tests read production constants — and a test that reads the constant is by
construction immune to the constant changing.

## Open follow-ups

Surfaced during implementation, deliberately not fixed. None blocks the remediation
being called done; all of them are real. Items 1 to 8 came out of specs 01 to 13;
items 9 to 15 came out of the last four specs and the verification sweep; items 16 and
17 came out of spec 10's strategy-contract theory, which is the first thing in this
codebase to read all 20 exception strategies side by side.

1. **~9 entity names still reach the client without an `Entity_*` label.** The
   repository path formats them from the type name instead of resolving a localized
   label, so a French client sees an English entity noun inside an otherwise French
   sentence. Spec 04's checklist asked for a ticket covering seven; the real count is
   about nine. No ticket is filed.

2. **`PublicGetArtistBySlugHandler` has two guards a test cannot tell apart.** Both
   raise `i18n.Artist.NotFound(...)` — the slug miss at `:45` with `Guid.Empty`, and
   the has-no-content case at `:58` with the real `artist.Id`. Same status, same
   `Title`, same sentence; they differ only by an interpolated guid, which is not a
   discriminator a test can assert without knowing the id in advance. The second is
   also wrong on its own terms: it tells the client an artist was not found by
   quoting the id of an artist that was found. This is the exact shape spec 04 exists
   to eliminate, surviving because the fix has to happen in `src/` rather than in the
   assertion. Either give them distinct messages or merge them.

3. **`FormatExceptionStrategy` has zero integration coverage.** By this codebase's own
   dead-code rule that means it is not wired into any reachable path. It is either
   unreachable and should be deleted, or reachable and needs the endpoint test that
   proves it. Do not close this by constructing the strategy inside
   `tests/Integration/`.

4. **Two thumbnail upload endpoints still answer a bodiless 400.**
   `AdminUploadVideoThumbnailEndpointV1` and `AdminUploadShortVideoThumbnailEndpointV1`
   were the two of eleven left unfixed, because neither has a file validation rule of
   any kind — there is no validator to attach a message to, and adding one naively
   turns the 400 into a 500. Fixing them means writing the missing validation first,
   which is a feature change.

5. **`LookupRepository.cs:203` repeats the culture-sensitive `ToLower()` bug.**
   `t => t.Name.ToLower() == name.ToLower()` is the same defect the session status
   filter had, in a different module. It survived because the audit only looked at
   `SessionQueryBuilder`. Under a Turkish locale a lookup by name silently misses.

6. **No Mailer integration test sends an `Accept-Language` header.** The Identity and
   Content catalogues are each exercised end to end under an explicit culture; the
   Mailer one is not, so nothing proves its localized strings are reachable through a
   real request. See [specs/06](specs/06-localization-testing.md). **Re-measured
   2026-08-24 and still true**: `grep -rln "Accept-Language" tests/Integration/Modules/Mailer`
   returns nothing. Spec 06's resource-completeness theory proves the Mailer strings
   *exist* in `en` and `fr`; nothing proves they are *reachable*.

7. **`EmailDeliveryFlowTests` still reads a baseline.** One assertion remains
   `stub.Sent.Count.Should().BeGreaterThan(alreadySent)` rather than an exact count —
   the last survivor of spec 02's isolation work, and an assertion that passes whether
   one email or fifty were sent.

8. **`CombineSpecification` is called with a null specification** when
   `SessionQueryBuilder.WithStatus` receives an unrecognised status. Carried forward
   unchanged from spec 13.

9. **97 of 329 public mock helpers (~30%) have zero call sites outside
   `tests/Unit/Common/Mocks/`.** Spec 07 change 5 was not attempted, so the dead
   fixture surface it measured is still there, and it is still the sufficient
   explanation for the 155 raw `new Mock<` declarations elsewhere in `tests/Unit` —
   including 26 `new Mock<IFormFile>` against a helper that exists. An author who
   cannot find a helper writes four lines and moves on.

10. **Only 2 mock helpers still accept an entity while matching its id with
    `It.IsAny`** — `MockAddItemTierFactory.SetupAttachTierAsync` and
    `MockAddOrderItemFactory.SetupCreateItemAsync`. Spec 07 change 1 has since landed
    and left these two behind; they are factory helpers rather than repository reads,
    so the change's grep invariant does not cover them. The forty files of fallout that
    change budgeted for came to twenty tests, which is the number to plan around.

11. **`CloudinaryService.ValidateVideoFile`'s doc comment says 100 MB;
    `FileConstants.MaxVideoFileSizeBytes` is 350 MB.** The comment at
    `src/Modules/Core/Core/Infrastructure/Services/CloudinaryService.cs:418`
    contradicts `src/BuildingBlocks/Constants/FileConstants.cs:91`. This is not
    cosmetic: it is where `UploadVideoAsync_AtExactMaxSize` got its 100 MB literal, so
    a boundary test spent its life 250 MB away from the boundary. The test now reads
    the constant; the comment still lies.

12. **`FileValidation` rejects a 0-byte avatar with "file too large."**
    `BeValidFileSize` at
    `src/Modules/Identity/Identity/Application/Auth/Validators/FileValidation.cs:70`
    is `file?.Length is > 0 and <= FileConstants.MaxAvatarFileSizeBytes`, and one
    message — `AvatarFileTooLarge` — is attached to both ends of that range. An empty
    upload tells the user their file is too large.

13. **`FileErrors.FileTooLarge(actualSize, maxSize, maxSizeMB)` ignores its first two
    parameters.** `src/Modules/Core/Core/Application/Shared/Errors/FileErrors.cs:66-67`
    passes only `maxSizeMB` to `validation.FileTooLargeWithLimit`. Every caller
    computes and hands over an actual size that never reaches the client. Either use
    them in the message or drop them from the signature.

14. **`FileValidationTests` covers unreachable null guards by reflection, which
    `CLAUDE.md` forbids.** `FileValidationTests.cs:287-293` resolves the private static
    `BeValidImageType` through `BindingFlags.NonPublic` and invokes it with `null` to
    reach a branch no caller can produce. The sanctioned route for a provably
    unreachable line is `[ExcludeFromCodeCoverage]` with a reason. This is a policy
    conflict to settle, not a test to quietly delete — the guard itself is fine.

15. **The four standards documents and `CLAUDE.md` were not re-read against the code**
    during the verification sweep. They are the part of this doc set that outlives the
    remediation, so an example in them that no longer compiles is a defect that will
    be copied. This is the highest-value item on this list.

16. **`DefaultExceptionHandler` builds its ProblemDetails inline, and the base helper's
    trace stamping is redundant with the middleware.**
    `src/Shared/Shared/Application/Exceptions/Handlers/Strategies/DefaultExceptionHandler.cs:19-25`
    constructs a `ProblemDetails` directly instead of calling
    `CreateStandardProblemDetails`, so it is the only strategy of 20 that emits no
    `traceId` and no `timestamp`. It does not reach clients that way, because
    `ExceptionHandler.EnrichProblemDetails`
    (`src/Shared/Shared/Application/Exceptions/Handlers/ExceptionHandler.cs:108-112`)
    re-stamps both on every response — which is the more interesting half of the
    finding: the base helper and the middleware both set the same two extensions, so
    one of them is dead weight. Needs a decision either way. Drop the duplication in
    the base, or route the fallback through the helper; the contract theory currently
    encodes the exemption as data (`ExceptionStrategyContractTests.cs:118-124`) and a
    fact at `:284` asserts that the exemption list holds exactly this one name, so
    whichever way it is settled, the test fails until it is updated deliberately.

17. **`FormatExceptionStrategy` reports a title that is not its exception's name.**
    `src/Shared/Shared/Application/Exceptions/Handlers/Strategies/FormatExceptionStrategy.cs:21`
    passes `title: nameof(InvalidFormatException)` while the strategy handles
    `FormatException`. It reads as deliberate — the strategy rewrites a raw parse
    failure into a domain-shaped error, message and all — but nothing says so, and it
    is the sole strategy of 20 whose `Title` is not `ExceptionType.Name`. Either
    document the intent on the class or make the title match. Related to item 3: the
    same strategy has no integration coverage, so nothing proves a client ever sees
    this title.

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
