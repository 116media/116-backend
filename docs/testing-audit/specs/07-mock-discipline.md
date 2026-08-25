# Spec 07 — Mock discipline

> **Status: landed, with changes 5 and 6 outstanding.** Changes 1 through 4 are in the
> code, and a defect class this spec did not anticipate — a swallowed exception in the
> Act phase — was found and fixed across 27 sites. Change 1 landed against the identity
> lookups: 48 blanket read defaults were removed across 18 repository mocks, and
> `grep -rn "GetByIdAsync(It.IsAny<Guid>()" tests/Unit/Common/Mocks/` now returns
> nothing. It landed by default removal with loose mocks retained, which is the only
> route this spec's own Scope section leaves open — changing `MockBehavior` in the same
> pass is ruled out there. The limit that follows from that choice, and the aggregate
> reads deliberately kept and sequenced as change 6, are argued for in the
> implementation notes. Change 5's dead-helper deletion was not attempted; change 6 is
> gated on `MockBehavior.Strict` and has not started. See the implementation notes
> below and [00-index.md](00-index.md) for global progress.

## Goal

The repository mock factories install a default return value for every read method,
matching any argument. A handler that asks for the wrong identifier is not caught by
that arrangement — it falls through to the blanket setup, receives `null`, takes its
not-found branch, and satisfies a test that was asserting the not-found branch. This
spec restricts mock defaults to write and void members so that a not-found
arrangement has to be stated, gives every read-setup helper the identifier it is
arranging for, inverts the password service's `Verify` default from accept to
reject, replaces all-`It.IsAny` verifications with argument matching where the
argument carries meaning, and clears the dead helper surface that drove authors to
hand-roll 188 raw mocks instead.

## Scope

In scope:

- `SetupDefaults` in `MockArticleRepository`, `MockFileRepository`,
  `MockAuthRepository`, and the remaining repository mocks under
  `tests/Unit/Common/Mocks/Repositories/`.
- New `SetupXNotFound(id)` helpers so absence is stated rather than defaulted.
- `MockPasswordService.SetupDefaults`.
- The 108 verifications that match two or more meaningful arguments with
  `It.IsAny<>` in every position.
- The 108 uncalled helper methods under `tests/Unit/Common/Mocks/`.
- The 188 raw `new Mock<>` sites in `tests/Unit`, starting with the 62 duplicating
  `FileTestHelpers.CreateMockFormFile`.

Not in this spec:

- The 6 assertion-free tests and the 658 verification-only tests. Giving those real
  outcome assertions is spec 05.
- `VerifyUpdateCalled` taking the expected entity. That is spec 05's change 1, and
  it lands first so this spec does not edit the same helper twice.
- Entity builders, factories, and the fixture layering rule. That is spec 08.
- Introducing `MockBehavior.Strict`. Every mock in the suite is loose today, and
  loose mocks are the reason a missing arrangement usually surfaces as a
  `NullReferenceException` inside the handler rather than as a silent pass. Changing
  the behaviour mode at the same time as tightening the defaults would make the
  fallout from this spec impossible to attribute. Revisit separately.

## Prerequisites

- **Spec 05 (outcome assertions)** must land first. It rewrites `VerifyUpdateCalled`
  across the repository mocks and rewrites 24 handler test files. Running this spec
  first means those 24 files get rewritten twice, and the missing arrangements this
  spec surfaces would be mixed in with the state assertions spec 05 adds, making
  each failure ambiguous.
- **Spec 02 (test isolation)** for the stub reset contract, so that a failure
  surfaced here is attributable to a missing arrangement rather than to state left
  behind by a previous test.

## Changes

### 1. Restrict repository mock defaults to write and void members

**Files:** `tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs`,
`MockFileRepository.cs`, `MockAuthRepository.cs`, and the remaining repository mocks
in that directory.

`MockArticleRepository.SetupDefaults` (lines 461-600) installs 35 setups, every one
matching `It.IsAny<>` in every position. `MockArticleRepository.Create()` has 41
call sites.

The write members are fine. `AddAsync`, `AddImageAsync`, `AddTagAsync`,
`AddLikeAsync`, `AddBookmarkAsync`, `AddShareAsync`, `AddCommentAsync`,
`AddCommentLikeAsync`, `RemoveLikeAsync`, `RemoveBookmarkAsync` and
`RemoveCommentLikeAsync` return `Task.CompletedTask`, which is what a loose mock
would return anyway; stating it costs nothing and removes a class of null-reference
noise.

The read members are the problem. `GetByIdAsync(any) → null` is not a neutral
default; it is a claim that any identifier the code under test produces is a miss,
installed before the test says anything.

```csharp
// tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs:461-600 — before (extract)
private static void SetupDefaults(Mock<IArticleRepository> mock)
{
    mock.Setup(x => x.AddAsync(It.IsAny<ArticleEntity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((ArticleEntity?)null);
    mock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((ArticleEntity?)null);
    mock.Setup(x => x.GetCommentByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((ArticleCommentEntity?)null);
    mock.Setup(x => x.GetPromotedAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<ArticleEntity>());
    // ... 30 more
}
```

```csharp
// after
/// <summary>
/// Installs defaults for write and void members only, so that a test which does not
/// care how many rows were added is not obliged to say so. Read members are left
/// unconfigured: a lookup the test did not arrange returns Moq's loose default, and
/// the test that depended on it has to state which identifier it expects.
/// </summary>
/// <param name="mock">The repository mock to configure.</param>
private static void SetupDefaults(Mock<IArticleRepository> mock)
{
    mock.Setup(x => x.AddAsync(It.IsAny<ArticleEntity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.AddImageAsync(It.IsAny<ArticleImageEntity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.AddTagAsync(It.IsAny<ArticleTagEntity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.AddLikeAsync(It.IsAny<ArticleLikeEntity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.AddBookmarkAsync(It.IsAny<ArticleBookmarkEntity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.AddShareAsync(It.IsAny<ArticleShareEntity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.AddCommentAsync(It.IsAny<ArticleCommentEntity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.AddCommentLikeAsync(It.IsAny<ArticleCommentLikeEntity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.RemoveLikeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.RemoveBookmarkAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.RemoveCommentLikeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
}
```

Apply the same rule to `MockFileRepository.SetupDefaults` (`:564-576`): keep
`AddAsync`, `UpdateAsync` and `SaveChangesAsync`; remove the
`GetByIdsAsync(any) → empty dictionary` default, which is a read.
`MockAuthRepository.SetupDefaults` (`:416-422`) already configures only `AddAsync`
and `AssignVisitorRoleAsync` and needs no change — verify and move on.

Then give the read-setup helpers the identifier they match on. Several accept the
entity but not the id:

```csharp
// tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs:245-252 — before
public static Mock<IArticleRepository> SetupGetCommentByIdAsync(
    this Mock<IArticleRepository> mock,
    ArticleCommentEntity? comment
)
{
    mock.Setup(x => x.GetCommentByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(comment);
    return mock;
}
```

```csharp
// after
/// <summary>
/// Arranges the comment returned for <paramref name="commentId" />. Other ids stay
/// unconfigured, so a handler that looks up the wrong one is not silently satisfied.
/// </summary>
/// <param name="mock">The repository mock to configure.</param>
/// <param name="commentId">The identifier this arrangement answers for.</param>
/// <param name="comment">The comment to return for that identifier.</param>
/// <returns>The same mock, for chaining.</returns>
public static Mock<IArticleRepository> SetupGetCommentByIdAsync(
    this Mock<IArticleRepository> mock,
    Guid commentId,
    ArticleCommentEntity comment
)
{
    mock.Setup(x => x.GetCommentByIdAsync(commentId, It.IsAny<CancellationToken>())).ReturnsAsync(comment);
    return mock;
}
```

This matters wherever a handler performs several lookups against one method.
`CommentReplyAddedNotificationsHandler` calls `GetCommentByIdAsync` twice with
different ids and then `GetByIdAsync`
(`src/Modules/Content/Content/Application/Interactions/EventHandlers/CommentReplyAddedNotificationsHandler.cs:40-58`).
Its own test already arranges both ids exactly
(`tests/Unit/Modules/Content/Application/Interactions/EventHandlers/CommentReplyAddedNotificationsHandlerTests.cs:47-51`)
and is the model to copy.

`CancellationToken` stays `It.IsAny<>` everywhere. The rule is per-position, not
per-call: exact values for arguments that carry meaning, `It.IsAny<>` for the ones
that do not.

*If done wrong:* deleting the read defaults without adding the `SetupXNotFound`
helpers in change 2 leaves roughly 40 test files with no way to express a miss
except by writing raw `Setup` calls, which reintroduces the duplication this mock
layer exists to prevent. Do changes 1 and 2 together.

### 2. Add explicit not-found arrangements

**Files:** the same repository mock files.

Once the blanket read defaults are gone, absence has to be stated. Add a
`SetupXNotFound(id)` helper next to each `SetupX` helper, so that a not-found
arrangement reads as a deliberate act:

```csharp
/// <summary>
/// Arranges a miss for <paramref name="articleId" />. Stating the identifier is
/// what distinguishes "this specific article does not exist" from "no lookup this
/// handler could make will succeed", which is what a blanket default asserts.
/// </summary>
/// <param name="mock">The repository mock to configure.</param>
/// <param name="articleId">The identifier that must resolve to nothing.</param>
/// <returns>The same mock, for chaining.</returns>
public static Mock<IArticleRepository> SetupGetByIdNotFound(this Mock<IArticleRepository> mock, Guid articleId)
{
    mock.Setup(x => x.GetByIdAsync(articleId, It.IsAny<CancellationToken>())).ReturnsAsync((ArticleEntity?)null);
    return mock;
}
```

The pattern already exists in the suite and should be matched rather than
reinvented: `MockFileRepository.SetupGetByIdReturnsNull(mock, fileId)` at
`MockFileRepository.cs:42-46` and
`MockAuthRepository.SetupFindUserByIdOrThrowNotFound(mock, userId)` at
`MockAuthRepository.cs:44-49` both take the identifier already. Name the new helpers
`SetupXNotFound` and rename `SetupGetByIdReturnsNull` to match, so the suite has one
convention rather than three.

A test that previously arranged nothing and relied on the default becomes:

```csharp
// before — the arrangement is invisible, and lives in another folder
Guid missingId = Guid.NewGuid();
var command = new AdminGetArticleQuery(Id: missingId.ToString());

// after
Guid missingId = Guid.NewGuid();
var command = new AdminGetArticleQuery(Id: missingId.ToString());
_articleRepositoryMock.SetupGetByIdNotFound(missingId);
```

*If done wrong:* a `SetupXNotFound()` overload with no identifier defeats the whole
change. Every not-found helper takes the id.

### 3. Invert the password service default

**File:** `tests/Unit/Common/Mocks/Services/MockPasswordService.cs:144-149`

```csharp
// before
private static void SetupDefaults(Mock<IPasswordService> mock)
{
    mock.Setup(x => x.Hash(It.IsAny<string>())).Returns(TestConstants.User.DefaultPasswordHash);

    mock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string?>())).Returns(true);
}
```

```csharp
// after
/// <summary>
/// Hashing is defaulted because no test asserts the hash algorithm through this
/// mock. Verification defaults to <c>false</c>: a test that depends on a credential
/// being accepted names which credential with <see cref="SetupVerifyReturnsTrue" />,
/// because a password service that accepts everything makes a rejection test pass
/// with the rejection removed.
/// </summary>
/// <param name="mock">The password service mock to configure.</param>
private static void SetupDefaults(Mock<IPasswordService> mock)
{
    mock.Setup(x => x.Hash(It.IsAny<string>())).Returns(TestConstants.User.DefaultPasswordHash);

    mock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string?>())).Returns(false);
}
```

**Be precise about what this fixes and what it does not.** No vacuous authentication
test ships today. `MockPasswordService.Create()` has exactly six call sites:

| Test file | Overrides `Verify`? |
| --- | --- |
| `AdminChangePasswordHandlerTests.cs:34` | yes, at `:71-72`, with the exact password and hash |
| `AdminLoginAuthFactoryTests.cs:30` | yes, `SetupVerifyReturnsTrue()` / `SetupVerifyReturnsFalse()` per test |
| `PublicChangePasswordHandlerTests.cs:34` | yes, at `:71-72` |
| `PublicLoginAuthFactoryTests.cs:29` | yes, at `:49` and `:147` |
| `PublicSetPasswordHandlerTests.cs:33` | no |
| `PublicSignUpAuthFactoryTests.cs:41` | no |

The four that authenticate an existing credential all override the default. The two
that do not are set-password and sign-up, which hash a new password and never verify
an old one. So this change costs nothing today and breaks nothing. It is worth doing
because the seventh caller — a test for a flow that *does* verify a credential,
written by someone who reasonably assumes `Create()` returns a neutral mock — gets a
green "wrong password is rejected" test that would pass with the rejection removed.
The design is one careless call away from a vacuous auth test; the six current call
sites are not evidence that it is safe.

*If done wrong:* adding a `SetupVerifyAlwaysTrue()` convenience helper alongside the
inverted default recreates the hole with an extra step. Callers name the credential.

### 4. Match the arguments that carry meaning

**Files:** the 108 verification sites that match two or more real arguments and use
`It.IsAny<>` for all of them, found by reviewing the 186 multi-argument `Verify`
calls in `tests/Unit`.

```csharp
// tests/Unit/.../UpdateArticleTags/AdminUpdateArticleTagsHandlerTests.cs:117-121 — before
_lookupRepositoryMock.Verify(
    x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()),
    Times.Exactly(2)
);
```

The test is named `Handle_WhenTagNamesAreNew_ShouldCreateTagsAndReturnSuccess` and
arranges `TagNames: new List<string> { "Afrobeats", "Rumba" }`. The verification
would pass if the handler created "Afrobeats" twice, created two tags named after
the article's slug, or transposed the two names.

```csharp
// after
_lookupRepositoryMock.Verify(
    x => x.AddTagAsync(It.Is<TagEntity>(t => t.Name == "Afrobeats"), It.IsAny<CancellationToken>()),
    Times.Once
);
_lookupRepositoryMock.Verify(
    x => x.AddTagAsync(It.Is<TagEntity>(t => t.Name == "Rumba"), It.IsAny<CancellationToken>()),
    Times.Once
);
_lookupRepositoryMock.Verify(
    x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()),
    Times.Exactly(2)
);
```

One identity assertion per expected argument, plus the cardinality check so a third
tag fails. This is the shape already used at
`tests/Unit/Shared/Infrastructure/Interceptors/DispatchDomainEventsInterceptorTests.cs:120-123`,
which is the counter-example to copy.

Where the predicate would be long, capture instead and assert with the full
expressiveness of the assertion library:

```csharp
var created = new List<TagEntity>();
_lookupRepositoryMock
    .Setup(x => x.AddTagAsync(Capture.In(created), It.IsAny<CancellationToken>()))
    .Returns(Task.CompletedTask);

await _handler.Handle(command, CancellationToken.None);

created.Select(t => t.Name).Should().Equal("Afrobeats", "Rumba");
```

The selection rule for which of the 108 to change: **if the test's method name refers
to an argument, that argument may not be `It.IsAny<>`.** Identifiers never stay
`It.IsAny<>`. `CancellationToken` and `EventId` always do.

*If done wrong:* replacing a cardinality assertion with identity assertions and
dropping the count lets an extra call through unnoticed. Keep both.

### 5. Delete the dead helpers and adopt the live ones

**Files:** all 50 files under `tests/Unit/Common/Mocks/`, plus the 70 files in
`tests/Unit` holding raw `new Mock<>` declarations.

`tests/Unit/Common/Mocks/` publishes 546 public helper methods across 50 files and
108 of them (20%) are never called from any test. Named examples to start from:
`MockSessionRepository.SetupGetByRefreshTokenHash` (`:55`),
`MockSessionRepository.VerifyCreateCalled` (`:330`),
`MockFileRepository.SetupUploadAndStoreAvatar` (`:87`),
`MockArticleRepository.SetupGetAbandonedDraftsAsync` (`:113`). Delete them.

Do the deletion **after** changes 1 through 4, not before. Those changes create new
call sites for helpers that are currently dead, so a deletion pass run first would
remove helpers the tightened tests are about to need. Re-measure the uncalled set
immediately before deleting.

Then adopt the helpers that already exist. `tests/Unit` outside
`tests/Unit/Common` contains 188 raw `new Mock<` declarations across 70 files. The
largest single cluster is 62 occurrences of `new Mock<IFormFile>()`, against a
helper that exists and is already used by 54 call sites in 12 files:

```csharp
// tests/Fixtures/Helpers/FileTestHelpers.cs:15,39
public static IFormFile CreateMockFormFile()
public static IFormFile CreateMockFormFile(string fileName, string contentType, long length)
```

```csharp
// before — one of 62
var fileMock = new Mock<IFormFile>();
fileMock.Setup(f => f.FileName).Returns("avatar.jpg");
fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
fileMock.Setup(f => f.Length).Returns(1024);
IFormFile file = fileMock.Object;

// after
IFormFile file = FileTestHelpers.CreateMockFormFile("avatar.jpg", "image/jpeg", length: 1024);
```

A site that reads the uploaded stream needs `FileTestHelpers.CreateFormFileWithContent`
instead, which returns a real `FormFile` over the bytes. Check what the code under
test does with the file before substituting.

After the `IFormFile` cluster, work through the remaining raw sites in descending
order of duplication. Each one either has a helper equivalent and adopts it, or has
none and gains one in `tests/Unit/Common/Mocks/`.

*If done wrong:* substituting `CreateMockFormFile()` at a site that reads
`OpenReadStream()` produces a null stream and a failure that looks unrelated to the
change.

### 6. Retire the aggregate read defaults, after strict behaviour lands

**Files:** the same repository mock files.

Change 1 removed the identity lookups. What remains in `SetupDefaults` is the
aggregate surface: `GetAllAsync(any) → (empty, 0)`, `GetPromotedAsync(any) → empty
list`, `HasLikedAsync(any, any) → false`, `GetReplyCountsAsync(any) → empty
dictionary`, `GetTotalsAsync(any) → ArtistTotals(Songs: 1, …)`. These are unstated
arrangements too — a per-artist list query answered `empty` for every artist id lets a
"this artist has no videos" test pass with the wrong artist id — but they differ from
change 1 in two ways that make them a separate change.

First, removing them is not behaviour-preserving: Moq's loose provider returns an
empty sequence only for `IEnumerable<T>`, `IQueryable<T>` and arrays, so
`Task<List<T>>`, `Task<IReadOnlyList<T>>`, `Task<Dictionary<K,V>>`,
`Task<IReadOnlySet<T>>` and every `(List<T>, int)` tuple resolve to `null` and the
handler dereferences it. The failure is a `NullReferenceException` in the mapper,
which points at the collection, not at the identifier. Second, the blast radius is
the full ~470 `Create()` call sites rather than the fourteen change 1 produced.

Do this after `MockBehavior.Strict`, not before. With strict mocks the missing
arrangement names the member and the arguments it was called with, which is the
diagnostic that makes ~470 sites tractable; without it the same work is 470
null-reference stack traces. Sequence per repository mock, as change 1 did.

*If done wrong:* replacing a removed collection default with `SetupX(empty)` in a
shared test constructor recreates the blanket answer one folder closer and is harder
to grep for than the original.

## Expected fallout

**This is the important section of this spec.** Change 1 will surface missing
arrangements across roughly 40 article handler test files, and that surfacing is the
deliverable, not a side effect. Each failure is a test that was relying on a
permissive default it never declared.

Three failure shapes, and the correct response to each:

1. **`NullReferenceException` inside the handler.** The test never arranged a lookup
   it depends on, and the blanket default was answering it. Add the arrangement with
   the explicit id. This is the common case and it is a straightforward fix.
2. **A not-found test still passes, but for a different reason.** The handler was
   asking for an identifier the test never named. Add `SetupXNotFound(theRealId)`
   and confirm the test now fails when the id is changed. If it does not, the
   handler is looking up something the test does not understand, and that needs
   reading before it needs fixing.
3. **A test fails because the handler asks for the wrong id.** That is a production
   defect, previously masked. File it; do not relax the arrangement to make it green.

Budget accordingly: the arrangement work is mechanical but wide, and shape 3 is the
reason to do it.

**Change 3 breaks nothing today.** All six `MockPasswordService.Create()` call sites
still pass, for the reasons tabulated above. If one does fail, that is a genuinely
vacuous authentication test and the most valuable single result in this spec.

**Change 5 reduces the helper surface by roughly a fifth and should reduce it
further.** A 546-method surface with 108 unreachable members is not searchable, and
that is a sufficient explanation for the 188 raw mocks: an author who cannot find
`CreateMockFormFile` writes four lines and moves on.

## Testing

```bash
dotnet test tests/Unit
dotnet test tests/Integration
```

The unit suite is expected to be red after change 1 and green before the spec
closes. The integration suite must stay green throughout — it mocks nothing except
external services, so any integration failure here means something other than this
spec changed.

The new tests that prove the fix are not new files but new discrimination in
existing ones. Verify by mutation on three representative cases:

| Mutation to `src/` | Test that must now fail |
| --- | --- |
| swap `ParentCommentId` and `ReplyId` in `CommentReplyAddedNotificationsHandler` | `CommentReplyAddedNotificationsHandlerTests` |
| transpose the two tag names in `AdminUpdateArticleTagsHandler` | `Handle_WhenTagNamesAreNew_ShouldCreateTagsAndReturnSuccess` |
| make `IPasswordService.Verify` return `true` unconditionally | `AdminLoginAuthFactoryTests` rejection case |

Grep-provable invariants after this spec:

```bash
# no identity-lookup default survives in a mock factory
grep -rn "GetByIdAsync(It.IsAny<Guid>()" tests/Unit/Common/Mocks/     # → nothing
grep -rn "ReturnsAsync((.*Entity?)null)" tests/Unit/Common/Mocks/ \
  | grep -E "It.IsAny<(Guid|string|Guid\?)>"                          # → nothing

# the password default rejects
grep -n "Verify(It.IsAny<string>(), It.IsAny<string?>())).Returns(true)" \
  tests/Unit/Common/Mocks/Services/MockPasswordService.cs             # → nothing

# the IFormFile cluster is gone
grep -rn "new Mock<IFormFile>" tests/                                 # → nothing
```

For the dead-helper deletion, the check is a re-run of the measurement rather than a
grep: enumerate public methods under `tests/Unit/Common/Mocks/` and confirm every
one has at least one call site outside that directory.

## Risks

**Roughly 40 files turning red at once is hard to review and easy to fix
carelessly.** Mitigate by sequencing: change `MockArticleRepository` first, fix its
41 call sites, and land that as one commit before touching `MockFileRepository`.
Reviewing 40 files that all changed for the same reason is tractable; reviewing 100
that changed for three reasons is not.

**The pressure under a red suite is to re-add a default.** Someone will propose
restoring `GetByIdAsync(any) → null` "temporarily". That restores the exact defect
this spec exists to remove and does so invisibly, because the restored line looks
like an innocuous convenience. If the fallout cannot be absorbed in one change,
land change 1 per repository mock rather than partially per method.

**`It.Is<T>(predicate)` failures produce poor diagnostics.** Moq reports that no
matching invocation was found without saying which property differed. Where the
predicate covers more than one property, prefer the `Capture.In` form so the
assertion library produces a readable diff.

**Deleting 108 helpers can remove something a branch in flight depends on.** Run the
deletion as its own commit, immediately after the tightening work rather than before
it, and re-measure the uncalled set at that moment rather than reusing the audit's
number.

**Substituting `FileTestHelpers.CreateMockFormFile` changes the default file name,
length and content type** from whatever each site hand-rolled to `test.jpg`, 1024
bytes and `image/jpeg`. A validation test asserting a size or extension boundary
must use the three-argument overload with its original values, not the parameterless
one.

## Implementation notes

Implemented and verified 2026-08-24.

**Change 3 was already done, and this spec was stale about it.**
`MockPasswordService.SetupDefaults` at
`tests/Unit/Common/Mocks/Services/MockPasswordService.cs:129` arranges
`Verify(It.IsAny<string>(), It.IsAny<string?>())` to return `false`. The invert was
landed by an earlier spec; nothing was changed here beyond confirming the six call
sites still pass.

**Change 4 landed against a narrower, stated rule.** Seven mock helpers were
tightened across 14 argument positions, updating 34 call sites. The rule applied,
recorded here so the next author does not have to re-derive it:

1. An argument the assertion is about is matched by value or by `It.Is<T>`.
2. An identifier is never `It.IsAny<>`.
3. `CancellationToken` and `EventId` always stay `It.IsAny<>`.

`It.Is<` now appears 37 times under `tests/Unit/Common/Mocks/`. Zero assertion-free
tests remain.

**A separate defect class was found and fixed, outside this spec's change list.**
27 sites across 21 files wrapped the Act phase in `try { … } catch (SomeException) { }`
with the assertions inside the catch and nothing after it. Every one of them passed
if the handler stopped throwing entirely, which is the same failure shape as an
assertion-free test wearing a `[Fact]`. All 27 are now
`await act.Should().ThrowAsync<T>()`. The three `try/catch` blocks left in `tests/Unit`
(`Shared/Middleware/ResourceNotFoundMiddlewareTests.cs:74,106,166`) assert on the
caught exception and throw when nothing was thrown, so they cannot pass silently.

Three further corrections made while implementing:

1. One hollow `[Fact(Skip)]` with an empty expression body was deleted —
   `ArtistRepositoryTests`, the folded-name search case. The behaviour is covered by
   `PublicGetArtistsEndpointV1Tests`, which reaches it through a real request.
   `grep -rn "Fact(Skip" tests/` now returns nothing.
2. Six loose `Times` constraints were tightened to exact cardinalities.
3. `LoggingDecoratorTests.Handle_ShouldLogEndEvenIfHandlerThrows` was renamed. It
   asserted the opposite of the truth: END is **not** logged on the throwing path.

**Change 1 landed, scoped to identity lookups, by default removal rather than by
`MockBehavior.Strict`.** Every `SetupDefaults` that answered an identity question with
a miss for any argument had that answer deleted: 48 blanket read defaults across 18
repository mocks — the `ReturnsAsync((XEntity?)null)` family, the two
`GetItem…OrThrowAsync`/`GetTagByIdOrThrowAsync` `ThrowsAsync(new NotFoundException(…))`
defaults, and `MockFileRepository.GetByIdsAsync(any) → empty dictionary`, which the
spec names explicitly. Both of the spec's grep invariants now hold. Two single-user
lookups in `MockUserLookupService` were included for the same reason; its batch
`GetAuthorInfosByIdsAsync` default was kept.

**What was deliberately kept, and why.** The aggregate reads — empty lists, empty
tuples, `false`, `0`, and the batch dictionaries — remain. They do not answer an
identity question, so they cannot convert a wrong-identifier bug into a passing
not-found test, which is the defect this spec exists to remove. They are also the
defaults handler tests lean on hardest: under loose Moq, `Task<List<T>>`,
`Task<(List<T>, int)>`, `Task<IReadOnlyList<T>>`, `Task<Dictionary<K,V>>` and
`Task<IReadOnlySet<T>>` all resolve to `null`, not to an empty collection, so removing
them turns roughly 470 mock call sites into `NullReferenceException`s that say nothing
about identifiers. That work is worth doing, but it is a different change with a
different failure mode; it is recorded as change 6 below rather than smuggled into
change 1.

**The honest limit of change 1 under loose mocks, which the spec did not state.**
Deleting `GetByIdAsync(any) → null` does not, on its own, make an unarranged call
loud: Moq's loose default for a reference-typed read is already `null`, so the removal
is behaviour-preserving for the whole `ReturnsAsync((XEntity?)null)` family. What it
buys is that the arrangement is no longer *asserted in another folder* — a test that
depends on a miss now has to say so, and the invariant is grep-checkable. Making the
call actually fail needs the `MockBehavior.Strict` step this spec defers in its Scope
section; until that lands, change 1 is a precondition for loudness, not loudness
itself. The three defaults that were *not* behaviour-preserving are exactly the ones
that produced fallout, listed below.

**Fallout, and the arrangement each red test was given.** Twenty tests across six
files went red, and thirteen call sites in thirteen more stopped compiling once the
nullable-entity helper overloads were withdrawn. The `OrThrow` defaults: `AdminRemoveOrderItemHandlerTests` and
`AdminEditOrderItemHandlerTests` (`Handle_WhenItemNotFound_…`) and
`AdminRemoveItemTierHandlerTests` (`Handle_WhenTierNotFound_…`,
`Handle_WhenItemBelongsToAnotherOrder_…`) all asserted `NotFoundException` while
passing `Guid.NewGuid()` inline for the identifier they never named; each now hoists
that id and states `SetupGetItemByIdOrThrowNotFound(orderId, itemId)` or
`SetupGetItemTierByIdOrThrowNotFound(itemId, tierId)`, two new helpers added next to
their positive counterparts. The `GetByIdsAsync` default: `AdminGetAllShortsHandlerTests`,
`PublicGetShortsFeedHandlerTests`, `PublicGetPublicShortsHandlerTests`,
`ShortVideoMapperTests` (four cases) and `MapperExtensionTests` all batch-map short
videos, which always carry a `VideoFileId`, so each now states its file map with
`SetupGetByIds`. `PublicGetOwnShortVideoFavoritesHandlerTests` already stated it and is
the shape the others were made to match. No test was made green by restoring a default.

**Four helpers still arranged a miss for every identifier from inside the test, which
is the same defect one level down.** `MockArticleRepository.SetupGetCommentByIdAsync`,
`SetupGetCommentByIdInArticleAsync`, `MockVideoRepository.SetupGetRatingAsync` and
`MockPlaylistRepository.SetupGetByIdAsync`/`SetupGetByIdWithVideosAsync` took a
nullable entity and fell back to `It.IsAny<Guid>() → null` when handed `null`. The
nullable parameter is gone; each has a sibling `SetupXNotFound(id)` and the thirteen
call sites that passed `null` now name the identifier they are a miss for.
`SetupGetCommentByIdInArticleAsync` additionally matches the comment id, not just the
article id.

**No handler was caught asking for the wrong identifier.** The three comment handlers
the tightened article helpers now discriminate against
(`PublicEditArticleCommentHandler`, `PublicDeleteArticleCommentHandler`,
`AdminDeleteArticleCommentHandler`) all pass `command.CommentId` and
`command.ArticleId` in the right positions, and `PublicRateVideoHandler` passes
`userId`/`videoId` in the order `IVideoRepository.GetRatingAsync` declares.

**Change 5 was not attempted.** `new Mock<IFormFile>` still appears 26 times in
`tests/`, and 155 raw `new Mock<` declarations remain in `tests/Unit` outside
`tests/Unit/Common/`. The dead-helper measurement was re-run and is recorded as an
open follow-up in [../90-remediation-plan.md](../90-remediation-plan.md): 97 of 329
public mock helpers have no call site outside `tests/Unit/Common/Mocks/`.

**The ~40-file fallout this spec budgeted for no longer exists.** Only two mock
helpers still accept an entity while matching its identifier with `It.IsAny`:
`MockAddItemTierFactory.SetupAttachTierAsync` (`:17-23`) and
`MockAddOrderItemFactory.SetupCreateItemAsync` (`:18-26`). Spec 13's scoped-overload
work and spec 04's exact-detail sweep between them absorbed most of what the Expected
fallout section predicted, so the remaining exposure is two files, not forty.

## Checklist

- [x] 1 — `SetupDefaults` in every repository mock installs no identity-lookup default,
      and every read-setup helper takes the identifier it matches on — 48 defaults
      removed across 18 mocks, 5 helpers de-nullified with `SetupXNotFound(id)`
      siblings, 33 tests and call sites given the arrangement they were relying on. Aggregate reads
      are kept on purpose; retiring them is change 6
- [x] 2 — a `SetupXNotFound(id)` helper exists next to each read helper, and the
      naming is consistent across the mock files — landed as
      `SetupXOrThrowNotFound(id)`, 29 helpers
- [x] 3 — `MockPasswordService` defaults `Verify` to `false`, with the doc comment
      explaining why, and all six existing call sites still pass
- [x] 4 — no `Verify` matches two or more meaningful arguments entirely with
      `It.IsAny<>`; identifiers are matched by value, `CancellationToken` is not;
      cardinality assertions keep their accompanying identity assertions — 7 helpers,
      14 argument positions, 34 call sites, under the three-part rule in the notes
      above
- [ ] 5 — the uncalled helper methods under `tests/Unit/Common/Mocks/` are deleted
      after the tightening work, and `new Mock<IFormFile>` no longer appears in
      `tests/` — **not attempted**; 97 uncalled helpers and 26 `new Mock<IFormFile>`
      sites remain
- [ ] 6 — the aggregate read defaults are retired once `MockBehavior.Strict` is in
      place — **not started**; deliberately deferred, see change 6
