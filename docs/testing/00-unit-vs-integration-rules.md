# Testing Rules — Unit vs Integration

The canonical rulebook for deciding **which suite a test belongs in** and **what makes a test
legitimate in that suite**. Read this before adding any test. It overrides habit and overrides
whatever a coverage report seems to be asking for.

Companion guides: [`how-to-tests/`](../how-to-tests/00-overview.md) (unit patterns),
[`integration-tests/`](integration-tests/00-overview.md) (integration infrastructure).

---

## 1. The one-sentence difference

| Suite | Answers the question |
| --- | --- |
| **Unit** | "Given these inputs, is this class's own logic correct?" |
| **Integration** | "Is this code actually reachable, wired up, and correct when the real system runs it?" |

A unit test proves a method **works**. An integration test proves the method is **used**.
Those are different claims, and only one of them catches dead code.

---

## 2. What problem each suite solves

### Unit tests solve: "is the logic right?"

They isolate one class, substitute its collaborators with mocks, and assert behaviour that is
expensive or awkward to reach end-to-end:

- Every branch of a domain guard, including the throw paths a validator would normally block
- Every rule of a validator, including edge lengths and formats
- Handler orchestration — that the right repository call happened, in the right order
- Pure functions, mappers, specifications' predicate logic
- Error factory methods that build a specific exception type

They are fast (no Docker, no HTTP, milliseconds), so they are the right place for **exhaustive
combinatorial coverage**. If you need 14 cases to cover a validation rule, they belong here.

### Integration tests solve: "is it wired up and does it behave under the real system?"

They run the **real** thing — real PostgreSQL via Testcontainers, real DI container, real HTTP
pipeline — and only stub genuinely external services (Cloudinary, YouTube). They catch the
entire class of bugs a unit test structurally cannot see:

- **Dead code.** A method with a green unit test but no caller is invisible to unit coverage and
  glaring in integration coverage. See §5 — this is the most valuable thing they do.
- DI wiring: a service registered with the wrong lifetime, or not registered at all
- Routing, auth, rate limiting, API versioning, middleware order
- EF Core reality: LINQ that does not translate, `ILike`, snake_case mapping, FK cascades,
  unique constraints, unflushed-insert semantics, navigation loading
- Exception → `ProblemDetails` conversion, and which status code actually reaches the client
- Cross-module flows (Identity token → Content endpoint → Core file)

They are slow. They are **not** the place for combinatorial edge cases.

---

## 3. The decision rule

Ask in order. The first "yes" wins.

1. **Does it need a real database, the DI container, or the HTTP pipeline to be meaningful?**
   → Integration.
2. **Am I asserting that this code path is reachable from a real caller?**
   → Integration.
3. **Otherwise** → Unit.

Corollary: if a test would pass identically with the database and web host deleted, it is a unit
test. Putting it in `tests/Integration/` does not change that — it just makes the suite slower
and the coverage report lie.

---

## 4. Hard rules

### 4.1 Integration tests

**MUST** reach the code under test through a real entry point. Exactly two are legitimate:

```csharp
// (a) Real HTTP — for anything behind an endpoint
[Collection("Database")]
public class AdminCreateAlbumEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Update_NonExistentAlbum_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Albums}/{Guid.NewGuid()}", request);
        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }
}

// (b) Real repository from DI — for query/persistence behaviour
[Collection("Database")]
public class LyricsRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task GetPublishedByAlbumAsync_WithSiblingTracks_ReturnsThemExcludingTheSource()
    {
        var repo = Resolve<ILyricsRepository>();
        List<LyricsEntity> tracks = await repo.GetPublishedByAlbumAsync(album.Id, source.Id);
        tracks.Should().ContainSingle(l => l.Id == sibling.Id);
    }
}
```

**MUST NOT**, inside `tests/Integration/`:

| Forbidden | Why |
| --- | --- |
| `new SomeValidator(...)` / `new SomeHandler(...)` / `new SomeSpecification(...)` | That is a unit test wearing an integration test's folder name |
| `new SomeEntity(...)` / `Entity.Create(...)` purely to assert a guard throws | Domain guards are unit-tested (§4.2) |
| Constructing an error factory to assert the exception it returns | Proves nothing about whether anything *calls* it |
| Reflection to invoke a `private` member | If it is unreachable, that is a finding — report it, do not fake it |
| Mocking a repository, service, or `DbContext` | Integration tests use real implementations |
| Building your own `ServiceCollection` to assert registrations | Assert through behaviour of the real host instead |

**Specifications are never referenced directly.** Cover them by calling the repository method
that uses them, and name the spec in the doc comment so intent is traceable:

```csharp
/// <summary>
/// Verifies that <see cref="ILyricsRepository.GetAllAsync" /> with a search query returns only
/// matching lyrics, exercising the search path in <c>LyricsRepository</c> via
/// <c>LyricsSearchSpecification</c>.
/// </summary>
```

**Folder layout mirrors `src/`.** An endpoint test lives at the path matching its use-case
folder, named `<UseCase>EndpointV1Tests.cs`. There is no `Domain/` folder and no
`Specifications/` folder under `tests/Integration/` — if you are creating one, you are writing a
unit test in the wrong place.

### 4.2 Unit tests

**MUST** live in `tests/Unit/`, mirroring the `src/` path.

**SHOULD** own these exhaustively, because integration tests deliberately skip them:

- Domain entity guards and every state transition, including no-op/early-return branches
- Validator rules — every boundary, format, and `When(...)` predicate
- Handler orchestration with mocked repositories
- Error factory methods and localized message construction
- Specification predicate logic (`.ToExpression().Compile()`), where DB translation is not the point

**MUST NOT** touch a real database, the HTTP pipeline, or the DI container.

**Never delete or weaken a defensive guard to chase a coverage number.** A guard with no
reachable caller is defense-in-depth; unit-test it and, if it is provably unreachable by
construction, mark it `[ExcludeFromCodeCoverage]` with a comment saying why.

---

## 5. Coverage is a signal, not a target

This is the rule that matters most, and the one most easily broken with good intentions.

> **A file with high unit coverage and near-zero integration coverage is telling you the code is
> not wired into the application. That is a defect in the source, not a gap in the tests.**

When you see that pattern, the correct response is, in order:

1. **Find the callers.** `grep` the whole of `src/` for the member. If the only hits are its own
   definition, its DI registration, and an i18n facade property — it is dead.
2. **Decide, with the author, between:**
   - **Wire it up** — usually the right fix. Example: a handler returning a generic
     `FirstDefaultOrThrowAsync` 404 should throw the domain-specific
     `i18n.Translation.RevisionNotFound(id)` so the client gets a localized, meaningful error.
     The integration test then covers the line *as a by-product of testing real behaviour*.
   - **Delete it** — if the member is genuinely surplus API surface with no intended consumer.
3. **Only then** write the test, driving it through the real entry point you just wired.

What you must **never** do is close the gap by constructing the object directly inside
`tests/Integration/`. That turns green a metric whose entire purpose was to warn you, and the
dead code ships.

### Worked example

`TranslationErrors.RevisionNotFound(Guid)` sat at 0% integration coverage.

- ❌ **Wrong:** `new TranslationErrors(msg).RevisionNotFound(id).Should().BeOfType<NotFoundException>()`
  placed in `tests/Integration/`. Coverage goes green; the method still has zero callers; the API
  still returns a generic 404.
- ✅ **Right:** discover it has no callers → change `PublicVoteOnTranslationRevisionHandler` to
  throw it instead of relying on the generic repository throw → add an endpoint test that votes
  on a non-existent revision and asserts a 404 with the localized message. Coverage goes green
  *because the code is now genuinely part of the application.*

---

## 6. Quick reference

| Concern | Unit | Integration |
| --- | :---: | :---: |
| Domain entity guards, transitions, no-op branches | ✅ own it | ❌ skip |
| Validator rules and boundaries | ✅ own it | ❌ skip |
| Handler orchestration (mocked deps) | ✅ own it | ❌ skip |
| Error factory / message construction | ✅ own it | via real error responses only |
| Specification predicate logic | ✅ own it | indirectly, via repository calls |
| Repository queries against real SQL | ❌ | ✅ own it |
| Endpoint routing, auth, rate limiting | ❌ | ✅ own it |
| `ProblemDetails` status + body | ❌ | ✅ own it |
| DB constraints, cascades, indexes | ❌ | ✅ own it |
| DI wiring / module registration | ❌ | ✅ own it |
| Interceptors, decorators | ❌ | ✅ own it |
| Cross-module flows | ❌ | ✅ own it |
| **Proving code is reachable at all** | ❌ | ✅ own it |

Because endpoints belong to integration, `*EndpointV1.cs` files are excluded from the
**unit** suite's coverage accounting (`tests/coverage.unit.runsettings` and the unit flags in
`scripts/run-tests-with-coverage.sh`). The integration suite keeps counting them, so an
unreached endpoint still shows up where it is actually a defect.

---

## 7. Review checklist

Before opening a PR that adds tests:

- [ ] Every file under `tests/Integration/` reaches its target via `BaseApiTest` + `Client`, or
      `BaseRepositoryTest` + `Resolve<T>()`. No `new` on a validator, handler, spec, entity, or
      error factory.
- [ ] No new `Domain/` or `Specifications/` folder under `tests/Integration/`.
- [ ] No reflection into private members anywhere.
- [ ] No mocks under `tests/Integration/` (external-service stubs excepted).
- [ ] Integration folder path mirrors the `src/` use-case path.
- [ ] Any coverage gap closed by **wiring code up or deleting it**, not by direct construction.
- [ ] Any line deemed genuinely unreachable is `[ExcludeFromCodeCoverage]` with a reason, and
      called out in the PR description — never covered by reflection.
- [ ] Assertions are meaningful: status code **and** persisted side effect, not just "not null".
