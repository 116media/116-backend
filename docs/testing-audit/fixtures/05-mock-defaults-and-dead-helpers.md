# High — Blanket mock defaults absorb wrong arguments, and a fifth of the mock helpers are dead

The repository mock helpers install a default setup for every read method that matches any
argument. Combined with Moq's loose behaviour, a handler that passes the wrong identifier
does not fail — it falls through to the default, receives `null`, and takes its not-found
branch, so an argument-swap bug reads as a passing not-found test. The password service
mock takes the pattern to its conclusion by defaulting credential verification to `true`.
Alongside that, 108 of the 546 helper methods these files publish are never called by any
test, while 188 raw `new Mock<>` instances elsewhere rebuild helpers that already exist.

## The problem

### Blanket defaults on read methods

```csharp
// tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs:17-22
public static Mock<IArticleRepository> Create()
{
    Mock<IArticleRepository> mock = new();
    SetupDefaults(mock);
    return mock;
}
```

```csharp
// tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs:461-474 (of 461-600)
private static void SetupDefaults(Mock<IArticleRepository> mock)
{
    mock.Setup(x => x.AddAsync(It.IsAny<ArticleEntity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.AddImageAsync(It.IsAny<ArticleImageEntity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.AddTagAsync(It.IsAny<ArticleTagEntity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.GetByOrderItemIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((ArticleEntity?)null);
    mock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((ArticleEntity?)null);
    mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((ArticleEntity?)null);
    // ... 29 more
}
```

35 setups, every one of them matching `It.IsAny<>` in every argument position.
`MockArticleRepository.Create()` has 41 call sites.

The write methods in that list are fine — `AddAsync`, `AddLikeAsync`, `RemoveBookmarkAsync`
return `Task.CompletedTask`, which is what a loose mock would do anyway, and stating it
costs nothing. The read methods are the problem. `GetByIdAsync(any) → null` is not a
neutral default. It is an assertion that any identifier the code under test produces is a
miss, and it is installed before the test says anything.

Some of the explicit helpers carry the same blindness:

```csharp
// tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs:245-252
public static Mock<IArticleRepository> SetupGetCommentByIdAsync(
    this Mock<IArticleRepository> mock,
    ArticleCommentEntity? comment
)
{
    mock.Setup(x => x.GetCommentByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(comment);
    return mock;
}
```

The helper takes the comment but not the id it should be returned for, so the test cannot
express which lookup it is arranging even if the author wanted to.

That matters where a handler performs several lookups against the same method:

```csharp
// src/Modules/Content/Content/Application/Interactions/EventHandlers/CommentReplyAddedNotificationsHandler.cs:40-58
ArticleCommentEntity? parent = await articleRepository.GetCommentByIdAsync(
    commentId: domainEvent.ParentCommentId,
    cancellationToken: cancellationToken
);
// ...
ArticleCommentEntity? reply = await articleRepository.GetCommentByIdAsync(
    commentId: domainEvent.ReplyId,
    cancellationToken: cancellationToken
);

ArticleEntity? article = await articleRepository.GetByIdAsync(
    id: domainEvent.ArticleId,
    cancellationToken: cancellationToken
);
```

Three lookups, three different identifiers, one method matched by `It.IsAny<Guid>()`. Swap
`ParentCommentId` and `ReplyId` in the handler and an id-blind arrangement observes
nothing.

That handler's own test does it correctly, and is the model:

```csharp
// tests/Unit/Modules/Content/Application/Interactions/EventHandlers/CommentReplyAddedNotificationsHandlerTests.cs:47-51
.Setup(x => x.GetCommentByIdAsync(_parent.Id, It.IsAny<CancellationToken>()))
    .ReturnsAsync(_parent);
.Setup(x => x.GetCommentByIdAsync(_reply.Id, It.IsAny<CancellationToken>()))
    .ReturnsAsync(_reply);
```

Two exact-id setups. Swapping the identifiers in the handler now returns the wrong entity
and the test fails. This is the pattern the defaults quietly opt every other test out of.

### The sharpest instance — a password service that accepts everything

```csharp
// tests/Unit/Common/Mocks/Services/MockPasswordService.cs:141-149
/// <summary>
/// Sets up default behaviors for the mock.
/// </summary>
private static void SetupDefaults(Mock<IPasswordService> mock)
{
    mock.Setup(x => x.Hash(It.IsAny<string>())).Returns(TestConstants.User.DefaultPasswordHash);

    mock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string?>())).Returns(true);
}
```

Any password, against any hash, verifies. Every test built on `MockPasswordService.Create()`
starts from an authentication service that cannot reject a credential.

**No vacuous authentication test ships today**, and the doc should be precise about that.
`MockPasswordService.Create()` has exactly six call sites:

| Test file | Overrides `Verify`? |
| --- | --- |
| `AdminChangePasswordHandlerTests.cs:34` | yes — `:71-72`, exact password and hash |
| `AdminLoginAuthFactoryTests.cs:30` | yes — `SetupVerifyReturnsTrue()` / `SetupVerifyReturnsFalse()` per test |
| `PublicChangePasswordHandlerTests.cs:34` | yes — `:71-72` |
| `PublicLoginAuthFactoryTests.cs:29` | yes — `:49`, `:147` |
| `PublicSetPasswordHandlerTests.cs:33` | no |
| `PublicSignUpAuthFactoryTests.cs:41` | no |

The four that authenticate an existing credential all override the default explicitly, and
the two that do not are set-password and sign-up — flows that hash a new password and never
verify an old one. The default is unused by every test that would be corrupted by it.

That is a description of today's six call sites, not of the design. The seventh caller —
a test for a flow that *does* verify a credential, written by someone who reasonably
assumes `Create()` returns a neutral mock — gets a green "wrong password is rejected" test
that would pass with the rejection removed.

### Dead helpers, and duplicated ones

The same helper files carry a large unused surface:

| Measurement | Value |
| --- | --- |
| Public helper methods under `tests/Unit/Common/Mocks/` | 546 across 50 files |
| Never called from any test file | 108 (20%) |

Examples: `MockSessionRepository.SetupGetByRefreshTokenHash` (`:55`),
`MockSessionRepository.VerifyCreateCalled` (`:330`),
`MockFileRepository.SetupUploadAndStoreAvatar` (`:87`),
`MockArticleRepository.SetupGetAbandonedDraftsAsync` (`:113`).

At the same time, tests bypass the helpers that do exist. Outside `tests/Unit/Common`,
`tests/Unit` contains **188 raw `new Mock<` declarations across 70 files**. The largest
single cluster is 62 occurrences of `new Mock<IFormFile>()`, against a helper that already
exists and is already used:

```csharp
// tests/Fixtures/Helpers/FileTestHelpers.cs:15
public static IFormFile CreateMockFormFile()

// tests/Fixtures/Helpers/FileTestHelpers.cs:39
public static IFormFile CreateMockFormFile(string fileName, string contentType, long length)
```

54 call sites across 12 files use it. 62 hand-rolled `IFormFile` mocks do not, each
re-establishing `FileName`, `Length` and `ContentType` in its own way.

## Why it matters

**A default return value is an unstated arrangement.** The Arrange section of a test is
supposed to enumerate what the code under test depends on. When 35 of those dependencies
are pre-arranged by a factory in another folder, the test file no longer records them,
and a reader cannot tell whether `null` came from the test's intent or from a default it
never saw.

**`It.IsAny<>` in an identifier position converts a wrong-argument bug into a passing
test.** This is the specific mechanism: the handler asks for the wrong id, the blanket
setup matches anyway, the mock hands back the not-found value, the handler takes the
not-found branch, and the test — which was asserting the not-found branch — goes green. It
would also have gone green with the correct id. The test proves the branch exists, not
that it is reached for the right reason.

**Not-found tests are the ones that inherit the defect.** A test that arranges a found
entity overrides the default and is safe. A test that arranges *absence* frequently
arranges nothing at all, because the default already says `null` — so the tests most
dependent on the mock being honest are the tests that never touch it.

**Dead helpers hide the live ones.** A 546-method surface with a fifth of it unreachable is
not searchable. That is a sufficient explanation for the 188 raw mocks: an author who
cannot find `CreateMockFormFile` in the noise writes `new Mock<IFormFile>()` in four lines
and moves on, and the sixty-third variant of `IFormFile` setup enters the suite.

## The fix

### 1. Restrict defaults to write and void methods

```csharp
// tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs — before
private static void SetupDefaults(Mock<IArticleRepository> mock)
{
    mock.Setup(x => x.AddAsync(It.IsAny<ArticleEntity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((ArticleEntity?)null);
    mock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((ArticleEntity?)null);
    // ... 32 more
}

// after
/// <summary>
/// Installs defaults for write and void members only, so that a test which does not
/// care how many rows were added is not obliged to say so. Read members are left
/// unconfigured: a lookup the test did not arrange returns Moq's loose default, and
/// the test that depended on it says which identifier it expects.
/// </summary>
private static void SetupDefaults(Mock<IArticleRepository> mock)
{
    mock.Setup(x => x.AddAsync(It.IsAny<ArticleEntity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.AddImageAsync(It.IsAny<ArticleImageEntity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    mock.Setup(x => x.AddTagAsync(It.IsAny<ArticleTagEntity>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    // write members only
}
```

Then give the explicit helpers the argument they were missing, so a not-found arrangement
names the id it is a miss for:

```csharp
// before
public static Mock<IArticleRepository> SetupGetCommentByIdAsync(
    this Mock<IArticleRepository> mock,
    ArticleCommentEntity? comment
)
{
    mock.Setup(x => x.GetCommentByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(comment);
    return mock;
}

// after
/// <summary>
/// Arranges the comment returned for <paramref name="commentId" />. Pass <c>null</c> to
/// arrange a miss for that specific id; other ids remain unconfigured so a handler that
/// looks up the wrong one is not silently satisfied.
/// </summary>
public static Mock<IArticleRepository> SetupGetCommentByIdAsync(
    this Mock<IArticleRepository> mock,
    Guid commentId,
    ArticleCommentEntity? comment
)
{
    mock.Setup(x => x.GetCommentByIdAsync(commentId, It.IsAny<CancellationToken>())).ReturnsAsync(comment);
    return mock;
}
```

`CancellationToken` stays `It.IsAny<>`. It is a parameter no assertion should depend on,
and pinning it is noise. The rule is per-position, not per-call: `It.IsAny<>` for arguments
that do not carry meaning, exact values for the ones that do.

### 2. Invert the password default

```csharp
// tests/Unit/Common/Mocks/Services/MockPasswordService.cs — before
mock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string?>())).Returns(true);

// after
/// <summary>
/// Defaults verification to <c>false</c>. A test that depends on a credential being
/// accepted must say which credential, with <see cref="SetupVerifySuccess" /> — an
/// unconfigured password service that accepts everything makes a rejection test pass
/// with the rejection removed.
/// </summary>
mock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string?>())).Returns(false);
```

All six existing call sites still pass: the four that authenticate already set up `Verify`
explicitly, and the two that do not never call it. The change costs nothing today and
closes the hole for the seventh caller.

### 3. Delete the dead, adopt the live

Delete the 108 uncalled helper methods. Then replace the duplicated mocks with the helper
that already exists:

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

## The principle

**A mock's default answer is part of the test's arrangement, and arrangements belong in the
test.** Defaults are legitimate for the members a test genuinely does not care about — void
returns, fire-and-forget writes, `Task.CompletedTask`. They are not legitimate for the
members whose answer the assertion depends on, because a value the test never chose cannot
be a value the test is checking.

Two rules follow, and they generalise past mocking:

1. **Match arguments that carry meaning, and only those.** `It.IsAny<Guid>()` where the
   identifier is the thing under test says "any id will do", which is never what the
   production code means.
2. **A helper's default must be the answer that makes a wrong implementation fail.** For a
   lookup that is "not found"; for a credential check that is "rejected". Defaulting to the
   permissive answer means the mock, not the code, is deciding the test's outcome.

## Checklist

- [x] `SetupDefaults` in every repository mock installs no identity-lookup default —
      48 removed across 18 mocks. The aggregate reads (empty lists, tuples, `false`,
      `0`, batch dictionaries) are kept on purpose: they answer no identity question,
      and under loose Moq removing them yields `null` collections rather than empty
      ones. Retiring them is spec 07 change 6, sequenced after `MockBehavior.Strict`
- [x] Every read-method setup helper takes the identifier it is arranging for — the
      four helpers that still fell back to `It.IsAny<Guid>() → null` when handed a
      null entity now have `SetupXNotFound(id)` siblings instead
- [x] `MockPasswordService` defaults `Verify` to `false`
- [ ] The 108 uncalled helper methods under `tests/Unit/Common/Mocks/` deleted
- [ ] `grep -rn "new Mock<IFormFile>" tests/` returns nothing —
      `FileTestHelpers.CreateMockFormFile` used instead
- [ ] Remaining raw `new Mock<>` sites either have no helper equivalent, or gain one
- [ ] Full unit suite green — expect failures where a test was relying on a blanket
      not-found default it never declared
