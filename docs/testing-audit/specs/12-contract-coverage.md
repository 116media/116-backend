# Spec 12 — Contract coverage

## Goal

Give the three pieces of published HTTP contract that are configured in
`src/Api/Program.cs` and asserted nowhere at least one test each: the header-based
API version reader, the CORS policy, and the registration of all ten named rate limit
policies. None of these is a correctness bug today. They are in the audit because two
of them are, by the codebase's own dead-code rule, indistinguishable from code that
could be deleted, and the third is a cheap addition to a design that is otherwise
right.

## Scope

In scope:

- `tests/Integration/Shared/Application/Versioning/ApiVersionReaderTests.cs` — new,
  or a documented decision to delete the header reader from `Program.cs`.
- `tests/Integration/Shared/Application/Cors/CorsPolicyTests.cs` — new, with the
  fixture and collection it needs.
- `tests/Integration/Shared/Application/Extensions/RateLimitingExtensionTests.cs` —
  the three hand-written algorithm-tier tests are **replaced** by one theory over all
  ten policies.
- `tests/Integration/Common/Fixtures/ApiFixture.cs` — restore the production
  `OnRejected` handler in `DisableRateLimiting`.
- `tests/Fixtures/Routes/Routes.cs` — route helpers for the endpoints the policy
  theory drives, where they do not already exist.

Not in this spec:

- Any change to the rate limit constants, the policy names, or `RateLimitingExtension`
  itself. This spec asserts what is configured; it does not change it.
- Widening `ApiFixture.DisableRateLimits`. The shared host must keep its no-op
  limiters; 1,879 tests on one host would trip the real limits constantly.
- The `Accept-Language` contract, which is already properly covered — see
  `tests/Integration/.../Login/V1/PublicLoginEndpointV1Tests.cs:274-315`, where an
  `fr` request asserts French literals and an emptied French resource file fails it.
  That is the pattern the three tests here copy.

## Prerequisites

- Spec 01 has landed. The CORS test reads its expectation from environment variables
  that `.env` currently clobbers on developer machines; until that is fixed the test
  would assert one thing on CI and another locally.
- Spec 11's Change 2 is understood before the policy theory lands. If the rate-limited
  collection now shares a database with the shared collection, the policy theory must
  continue to seed nothing and assert no persisted state. It does: it drives endpoints
  to rejection and never reads a row.
- Spec 04 has landed, so an error assertion pins the status, the ProblemDetails
  `Title` and the exact localized `Detail`. The rejection assertion here uses the
  existing `ShouldBeRateLimitRejectionAsync` helper rather than `ShouldBeProblem`, so
  no call site in this spec is affected — but note that 429 is reachable from two
  exception types (`RateLimitExceededException` and `OtpAttemptsLimitException`), so
  the helper must pin the title if it is ever used for both.

## Changes

### 1. Prove the `X-Api-Version` header reader, or delete it — decide first

`src/Api/Program.cs:35-38` combines two version readers:

```csharp
options.ApiVersionReader = ApiVersionReader.Combine(
    new UrlSegmentApiVersionReader(),
    new HeaderApiVersionReader("X-Api-Version")
);
```

`grep -rn "X-Api-Version" tests` returns nothing. Every integration test addresses
versions through the URL segment via `tests/Fixtures/Routes/Routes.cs`. The header
reader could be removed from the `Combine` call and the entire suite would stay green.
By the rule this codebase already applies to `src/`, that makes it indistinguishable
from dead code: wire it up or remove it.

**The decision point, with the evidence.** Every route in the application is mapped
through `MapApiVersionGroup`
(`src/Shared/Shared/Application/Extensions/ApiVersionExtension.cs:51-60`), which is
rooted at the group `app.MapGroup("api/v{version:apiVersion}")` created by
`UseApiVersioning` (`ApiVersionExtension.cs:30`). All 294 `ICarterModule`
implementations in `src/Modules/` use it; the only file mentioning `ICarterModule`
that does not is `CarterExtension.cs`, which registers them. **There is therefore no
endpoint a client can reach without a version in the URL segment.** The header reader
can never be the sole source of a version.

**Decision: Option A — the header reader is kept and tested.** Evidence gathered before
implementing:

- The reader is genuinely wired. `Program.cs:36-39` assigns the combined reader to
  `options.ApiVersionReader` inside `AddApiVersioning`, which is the single global reader
  `ApiVersioningFeature` consults on every request. It is configuration that runs, not an
  orphan.
- No client sends it. `grep -rn "X-Api-Version"` across the whole monorepo — `apps/backend`,
  `apps/frontend`, `apps/dashboard`, `apps/mobile` — returns only `Program.cs` and the audit
  documents. Nothing in any client depends on the header today.
- Deleting it is still the wrong call: it is a published contract in `CLAUDE.md`, consumers
  outside this repository cannot be enumerated, and this audit's ground rule is that behaviour
  does not change outside spec 13.
- One correction did come out of the investigation. `CLAUDE.md` claimed versions may be given
  by "URL path **or** header". That is false — every route sits under
  `api/v{version:apiVersion}`, so the segment is always required and the header can only agree
  or conflict with it. `CLAUDE.md` was corrected to say so; no `src/` behaviour changed.

Two options follow.

**Option A — assert the reader's one observable effect, and keep it.** With
`ApiVersionReader.Combine`, a request that supplies a version through both readers is
rejected as ambiguous when the two disagree. That is observable, and it fails if the
header reader is removed from the `Combine` call, which is exactly the property a test
of the header reader needs.

```csharp
// tests/Integration/Shared/Application/Versioning/ApiVersionReaderTests.cs
/// <summary>
/// Verifies that the header arm of the combined API version reader configured in Program.cs is
/// live, by asserting the two behaviours that distinguish a combined reader from a URL-segment
/// reader alone: a header agreeing with the URL segment resolves the endpoint, and a header
/// disagreeing with it is rejected as an ambiguous version rather than silently ignored.
/// </summary>
/// <param name="db">The shared Testcontainer database and application host.</param>
[Collection("Database")]
public class ApiVersionReaderTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AgreeingVersionHeader_ResolvesTheSameEndpointAsTheUrlSegment()
    {
        Client.AuthenticateAsVisitor();

        var request = new HttpRequestMessage(HttpMethod.Get, Routes.Public.Me.Profile());
        request.Headers.Add("X-Api-Version", "1.0");

        using HttpResponseMessage response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ConflictingVersionHeader_IsRejectedAsAmbiguous()
    {
        Client.AuthenticateAsVisitor();

        var request = new HttpRequestMessage(HttpMethod.Get, Routes.Public.Me.Profile());
        request.Headers.Add("X-Api-Version", "2.0");

        using HttpResponseMessage response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

The second fact is the load-bearing one. Delete `new HeaderApiVersionReader("X-Api-Version")`
from `Program.cs` and it turns green-to-red, because the conflicting header is no
longer read at all and the request succeeds on the URL segment.

**Two deviations from the sketch above, both forced by how `CombinedApiVersionReader` works.**
It collects raw strings into a case-insensitive `SortedSet<string>` and treats a count greater
than one as ambiguous, so `"1"` and `"1.0"` are *different* versions to it. The agreeing fact
therefore sends `X-Api-Version: 1`, matching the `v1` URL segment character for character —
which is also the form `CLAUDE.md` publishes. Sending `1.0` as the sketch did would itself be
ambiguous and return 400. The conflicting fact is named `ConflictingVersionHeader_IsRejected`
rather than `..._IsRejectedAsAmbiguous`: a conflicting header produces 400 either as
`AmbiguousApiVersionEndpoint` (internally named `"400 Ambiguous API Version"` in
`Asp.Versioning.Http` 8.1.0) or as an unsupported-version rejection, and the test asserts the
status, not which of the two produced it.

**Option B — delete the header reader and correct the documentation.** Remove the
reader from the `Combine` call, reducing it to `new UrlSegmentApiVersionReader()`, and
remove the "or header (`X-Api-Version: 1`)" claim from `CLAUDE.md`. This is the honest
option if the team's answer is that no client uses the header form.

Option A is recommended, because `CLAUDE.md` publishes the header as a supported
contract and Option A costs two tests. Option B is acceptable if the team decides the
contract was never real. What is not acceptable is leaving it as it is: the failure
mode is quiet, because `AssumeDefaultVersionWhenUnspecified` is `true` with a default
of `1.0` (`Program.cs:33-34`), so a broken header reader serves v1 to a client that
asked for something else and reports no error.

What breaks if done wrong: writing only the agreeing-header fact proves nothing.
`AssumeDefaultVersionWhenUnspecified` means the request resolves to v1 whether or not
the header is read, so that fact passes with the header reader deleted. The
conflicting-header fact is what makes the test a test.

### 2. Assert the CORS policy at the preflight

`src/Api/Program.cs:54-68` builds the default policy from
`AppEnvironment.CorsAllowedOrigins()`
(`src/Shared/Shared/Application/Configurations/Environment.cs:129-141`), which reads
`DASHBOARD_ORIGIN` and `WEBAPP_ORIGIN`:

```csharp
options.AddDefaultPolicy(policy =>
{
    if (allowedOrigins.Length > 0)
    {
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    }
    else
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    }
});
```

No test sends an `Origin` header and no test reads an `Access-Control-Allow-Origin`
response header. The two branches have opposite security postures — the populated
branch restricts origins and permits credentials, the empty branch permits any origin
and forbids them — and which one runs is decided by environment variables that
`ApiFixture.SetEnvironmentVariables` (`ApiFixture.cs:66-93`) does not set. Before
spec 01, a developer machine's `.env` overwrites the fixture's environment anyway, so
the branch under test is not merely untested, it is not deterministic across machines.
That is why this change is gated on spec 01.

Add a fixture that sets the origin before the host builds, following the pattern
`RateLimitedApiFixture` established:

```csharp
// tests/Integration/Common/Fixtures/CorsApiFixture.cs
/// <summary>
/// An <see cref="ApiFixture" /> that boots the application with a known CORS origin configured,
/// so the populated branch of the default policy in Program.cs is the one under test.
/// </summary>
/// <remarks>
/// The origin is set before the base fixture reads the environment, because
/// <c>AppEnvironment.CorsAllowedOrigins</c> is evaluated once during host construction. This host
/// must never be shared with the general suite: its policy restricts origins, and a test written
/// against the permissive branch would pass or fail depending on which host it landed on.
/// </remarks>
/// <param name="db">The Testcontainer database backing this host.</param>
public class CorsApiFixture(PostgresFixture db) : ApiFixture(db)
{
    /// <summary>
    /// The origin configured as allowed for the lifetime of this host.
    /// </summary>
    public const string AllowedOrigin = "https://dashboard.116.test";

    /// <inheritdoc />
    protected override void ConfigureEnvironment()
    {
        base.ConfigureEnvironment();
        Environment.SetEnvironmentVariable("DASHBOARD_ORIGIN", AllowedOrigin);
    }
}
```

**Deviation recorded.** Spec 01 as landed did not introduce the extension point: `ApiFixture`
still had a private `SetEnvironmentVariables`. This spec renamed it to
`protected virtual void ConfigureEnvironment()`; the body is unchanged. The `.env` half of
spec 01 *did* land (`Program.cs` loads with `Env.NoClobber()`), which is the part this change
depends on.

```csharp
// tests/Integration/Shared/Application/Cors/CorsPolicyTests.cs
/// <summary>
/// Verifies that the default CORS policy built from AppEnvironment.CorsAllowedOrigins admits a
/// configured origin with credentials and does not echo an unconfigured one.
/// </summary>
/// <param name="db">The dedicated Testcontainer database and CORS-configured application host.</param>
[Collection("Cors")]
public class CorsPolicyTests(CorsPostgresFixture db) : IDisposable
{
    private readonly HttpClient _client = db.Api.CreateClient();

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Preflight_FromConfiguredOrigin_EchoesAllowOriginAndAllowsCredentials()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, Routes.Public.Auth.Login());
        request.Headers.Add("Origin", CorsApiFixture.AllowedOrigin);
        request.Headers.Add("Access-Control-Request-Method", "POST");

        using HttpResponseMessage response = await _client.SendAsync(request);

        response.Headers.GetValues("Access-Control-Allow-Origin").Should().ContainSingle(CorsApiFixture.AllowedOrigin);
        response.Headers.GetValues("Access-Control-Allow-Credentials").Should().ContainSingle("true");
    }

    [Fact]
    public async Task Preflight_FromUnconfiguredOrigin_DoesNotEchoAllowOrigin()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, Routes.Public.Auth.Login());
        request.Headers.Add("Origin", "https://not-allowed.example");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        using HttpResponseMessage response = await _client.SendAsync(request);

        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
    }
}
```

`CorsPostgresFixture` mirrors `RateLimitedPostgresFixture`: it overrides
`CreateApiFixture` to return `CorsApiFixture` and, under spec 11's Change 2, shares the
container rather than starting its own. `[CollectionDefinition("Cors")]` goes beside
the existing collection definitions.

What breaks if done wrong: environment variables are process-global. If
`CorsApiFixture` sets `DASHBOARD_ORIGIN` and never clears it, every host built
afterwards in the same process inherits a restricted CORS policy. The fixture must
restore the previous value on dispose, and the `Cors` collection must be the only
consumer of that host. This is the same class of defect spec 02 addresses for
environment variables generally; if spec 02 introduced an env-var collection, join it.

### 3. Replace the three algorithm tests with one theory over all ten policies

`RateLimitingExtension` registers ten policies across three configure methods
(`src/Shared/Shared/Application/Extensions/RateLimitingExtension.cs:44-65`, `:70-85`,
`:90-118`). `RateLimitingExtensionTests` drives three of them, one per algorithm tier:
`Otp` for sliding window, `DataExport` for token bucket, `ContentContribution` for
fixed window. That class is well built and the algorithm coverage is complete.

The gap is narrower than "seven policies are untested". The seven not driven differ
from the three that are only by their constants. What nothing asserts is that all ten
are **registered**, under the names endpoints reference, with the limits their
constants declare. A policy name typo, or a policy dropped from
`ConfigureFixedWindowPolicies`, would surface as an endpoint silently running
unlimited, and no test would notice.

`RateLimiterOptions` exposes no public accessor for a registered policy, so
registration cannot be asserted directly without reflection. The only non-reflective
route is the one the existing tests already take: drive each policy through an
endpoint that declares it. That is affordable. The ten permit limits are 5, 3, 3, 10,
5, 100, 60, 30, 30 and 20, so exhausting all ten costs 266 admitted requests plus 10
rejections against an in-memory test server.

```csharp
/// <summary>
/// Supplies every named rate limit policy paired with the permit limit its constants declare and
/// a route whose endpoint carries that policy. Sourcing the limit from the production constant is
/// what makes the row prove the configured number rather than a copy of it.
/// </summary>
/// <returns>Policy name, permit limit, and a request against an endpoint declaring the policy.</returns>
public static TheoryData<string, int, HttpMethod, string> Policies() =>
    new()
    {
        {
            RateLimitPolicies.Authentication,
            AuthenticationRateLimitConstants.PermitLimit,
            HttpMethod.Post,
            Routes.Public.Auth.Login()
        },
        {
            RateLimitPolicies.Otp,
            OtpRateLimitConstants.PermitLimit,
            HttpMethod.Post,
            Routes.Public.Auth.VerifyOtp()
        },
        {
            RateLimitPolicies.PasswordManagement,
            PasswordManagementRateLimitConstants.PermitLimit,
            HttpMethod.Post,
            Routes.Public.Auth.ForgotPassword()
        },
        {
            RateLimitPolicies.FileUpload,
            FileUploadRateLimitConstants.TokenLimit,
            HttpMethod.Patch,
            Routes.Public.Me.Avatar()
        },
        {
            RateLimitPolicies.DataExport,
            DataExportRateLimitConstants.TokenLimit,
            HttpMethod.Get,
            Routes.Admin.Sessions.Export()
        },
        {
            RateLimitPolicies.ContentBrowsing,
            ContentBrowsingRateLimitConstants.PermitLimit,
            HttpMethod.Get,
            ApiRoutes.Public.Articles
        },
        {
            RateLimitPolicies.UserProfile,
            UserProfileRateLimitConstants.PermitLimit,
            HttpMethod.Get,
            Routes.Public.Me.Profile()
        },
        {
            RateLimitPolicies.SessionManagement,
            SessionManagementRateLimitConstants.PermitLimit,
            HttpMethod.Post,
            Routes.Public.Auth.SignOut()
        },
        {
            RateLimitPolicies.AdminMetrics,
            AdminMetricsRateLimitConstants.PermitLimit,
            HttpMethod.Get,
            Routes.Admin.Sessions.Metrics()
        },
        {
            RateLimitPolicies.ContentContribution,
            ContentContributionRateLimitConstants.PermitLimit,
            HttpMethod.Post,
            Routes.Public.LyricsSubmissionsAndRevisions.RevisionVotes(Guid.Empty)
        },
    };

/// <summary>
/// Drives every named policy to rejection through an endpoint that declares it, proving each is
/// registered under the name endpoints reference and enforced at the limit its constants declare.
/// All three algorithm tiers are covered, since the ten policies are split across sliding window,
/// token bucket, and fixed window configuration.
/// </summary>
/// <param name="policy">The policy name under test, used to identify a failing row.</param>
/// <param name="permitLimit">The number of requests the policy admits before rejecting.</param>
/// <param name="method">The HTTP method the endpoint declaring the policy accepts.</param>
/// <param name="route">A route whose endpoint declares the policy under test.</param>
[Theory]
[MemberData(nameof(Policies))]
public async Task EveryNamedPolicy_RejectsWithTooManyRequests_AtItsConfiguredLimit(
    string policy,
    int permitLimit,
    HttpMethod method,
    string route
)
{
    using HttpResponseMessage rejected = await ExhaustAsync(
        permitLimit,
        () => _client.SendAsync(new HttpRequestMessage(method, route))
    );

    await ShouldBeRateLimitRejectionAsync(rejected);
}
```

`ExhaustAsync` (`RateLimitingExtensionTests.cs:99-116`) and
`ShouldBeRateLimitRejectionAsync` (`:124-147`) are reused unchanged. `ExhaustAsync`
already does the load-bearing work: it asserts every request up to the limit is
admitted, which is what proves the constant, and returns the one past it, which is what
proves the policy exists.

Delete `SlidingWindowPolicy_RejectsWithTooManyRequests_WhenOtpPermitLimitExceeded`,
`TokenBucketPolicy_RejectsWithTooManyRequests_WhenDataExportTokensExhausted` and
`FixedWindowPolicy_RejectsWithTooManyRequests_WhenContentContributionPermitLimitExceeded`.
The theory **replaces** them rather than joining them. Each policy is a single
host-wide limiter, so two tests driving the same policy on the same host would steal
each other's permits and both would be flaky. The class remark at
`RateLimitingExtensionTests.cs:12-19` already states this constraint; update it to say
the theory is now the sole consumer of every policy on this host.

Keep the class's existing property that permitted requests may be unauthenticated or
carry an invalid body. The limiter runs before authentication and before model binding,
so a permit is consumed regardless of the outcome, which is what keeps these tests
focused on the limiter. `ExhaustAsync` asserts only that the status is not 429, so a
401 or 400 on the way up is expected and correct.

What breaks if done wrong: if a route in a row is served by an endpoint carrying a
*different* policy than the row names, the theory exhausts the wrong limiter and still
passes. Verify each row by opening the endpoint file and reading its
`RequireRateLimiting` call, and name the endpoint file in a comment beside the row.

### 4. Restore the production `OnRejected` handler in the test host

`ApiFixture.DisableRateLimiting` (`ApiFixture.cs:163-204`) removes every
`IConfigureOptions<RateLimiterOptions>` registration and installs a fresh
configuration with no-op limiters:

```csharp
services.Configure<RateLimiterOptions>(options =>
{
    options.RejectionStatusCode = 429;

    string[] policies = [ /* ten names */ ];

    foreach (var policy in policies)
    {
        options.AddPolicy(policy, _ => RateLimitPartition.GetNoLimiter("test"));
    }
});
```

It sets `RejectionStatusCode` but never restores `options.OnRejected`, which production
sets to `OnRateLimitRejected` (`RateLimitingExtension.cs:30`). That callback is what
throws `RateLimitExceededException`, which the global exception pipeline turns into a
429 ProblemDetails with a `Retry-After` header.

This is harmless today, because a no-op limiter never rejects. It is still a divergence
between the test host and production for no reason, and it means that if a future test
ever does trip a limit on the shared host, it sees a bare 429 with no body instead of
the contract clients actually receive. Restore it:

```csharp
services.Configure<RateLimiterOptions>(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = RateLimitingExtension.OnRateLimitRejected;

    // ... policy registration unchanged
});
```

`OnRateLimitRejected` is currently `private static` in `RateLimitingExtension`. Making
it `internal static` with `InternalsVisibleTo` for the integration test assembly is the
smallest change that avoids duplicating the handler in test code; duplicating it would
reintroduce exactly the drift this spec is closing.

**Visibility recorded.** `internal static`, with
`<InternalsVisibleTo Include="_116.Integration.Tests" />` added to
`src/Shared/Shared/Shared.csproj`. The handler is referenced, not copied.

What breaks if done wrong: copying the handler body into `ApiFixture` makes the test
host assert against a second implementation of the rejection contract, so a change to
the production handler would not be reflected. Reference the production member.

## Expected fallout

- Change 1 Option A may find the ambiguous-version response is not a 400. Assert the
  status the framework actually returns and record it; the point is that the header is
  read, not the specific code.
- Change 2 introduces the first fixture that varies an environment variable per host.
  If spec 02 introduced an environment-variable collection, the `Cors` collection must
  join it, and the two must not run in parallel.
- Change 3 adds roughly 276 in-memory HTTP requests to the run. Against a `TestServer`
  with no network, that is small; measure it as part of spec 11's timing step rather
  than assuming.
- Change 3 may fail on a row whose endpoint declares a policy the row does not name.
  That is the registration slip the change exists to detect. Fix the endpoint or the
  row, and record which.
- Change 4 changes nothing observable unless a shared-host test trips a limit, which
  none should.

## Testing

```bash
dotnet build
dotnet test tests/Integration --filter "FullyQualifiedName~ApiVersionReaderTests"
dotnet test tests/Integration --filter "FullyQualifiedName~CorsPolicyTests"
dotnet test tests/Integration --filter "FullyQualifiedName~RateLimitingExtensionTests"
dotnet test tests/Integration --settings tests/coverage.runsettings
```

The whole integration suite must be green, and the unit suite must be run once because
Change 4 may alter a member's visibility in `src/`.

What the new tests prove that nothing proved before:

- `ConflictingVersionHeader_IsRejectedAsAmbiguous` fails if
  `new HeaderApiVersionReader("X-Api-Version")` is removed from the `Combine` call in
  `Program.cs`. Prove it by removing the line locally and confirming red.
- `Preflight_FromConfiguredOrigin_EchoesAllowOriginAndAllowsCredentials` fails if the
  populated branch of the CORS policy stops calling `AllowCredentials()`, and
  `Preflight_FromUnconfiguredOrigin_DoesNotEchoAllowOrigin` fails if the branch flips
  to `AllowAnyOrigin()`. Prove both by swapping the branch condition locally.
- `EveryNamedPolicy_RejectsWithTooManyRequests_AtItsConfiguredLimit` fails if any
  `AddPolicy` call is removed from `RateLimitingExtension`, or if a policy name is
  misspelled. Prove it by deleting the `AdminMetrics` registration locally and
  confirming that row goes red while the other nine stay green.

## Risks

**Rate limiter state is host-wide and does not reset between tests.** Ten rows on one
host each consume a different policy's permits, which is safe, but only while the
theory is the sole consumer. Mitigation: the three hand-written tests are deleted, not
kept, and the class remark states the constraint explicitly so the next author does not
add an eleventh test against a policy a row already exhausted.

**A theory row can drive the wrong endpoint and still pass.** Exhausting `Otp` through
an endpoint that actually carries `Authentication` produces a rejection either way.
Mitigation: verify each row against the endpoint file's `RequireRateLimiting` call at
review time, and cite the endpoint file beside the row.

**The CORS fixture mutates a process-global environment variable.** Mitigation: it
restores the previous value on dispose, its collection is the only consumer, and it
joins spec 02's environment-variable collection so it cannot run concurrently with a
host that reads the same variables.

**Option B in Change 1 removes a published capability.** `CLAUDE.md` documents the
header form as supported, so deleting it is an API decision, not a test decision.
Mitigation: Option B requires the documentation change in the same commit and a note in
the PR description; it is not something to do quietly because it makes a test
unnecessary.

**Change 4 widens a production member's visibility for a test.** Mitigation: `internal`
plus `InternalsVisibleTo` rather than `public`, and it is done to avoid duplicating a
production behaviour in test code, which is a worse outcome.

## Checklist

- [x] 1 — Option A or Option B recorded here with the reasoning, before implementation
- [x] 1 — If Option A: both version-header facts added, and the conflicting-header fact
      confirmed to fail with the header reader removed from `Program.cs`
- [ ] 1 — If Option B: the reader removed from `Combine` and the `CLAUDE.md` claim
      corrected in the same commit — not applicable, Option A was taken
- [x] 2 — `CorsApiFixture`, `CorsPostgresFixture` and the `Cors` collection definition
      added, with the environment variable restored on dispose
- [x] 2 — Both preflight facts added and confirmed to fail when the policy branch is
      swapped
- [x] 3 — `Policies()` theory data added with all ten rows, each row's endpoint
      verified against its `RequireRateLimiting` call
- [x] 3 — The three hand-written algorithm-tier tests deleted, not kept
- [x] 3 — `ExhaustAsync` and `ShouldBeRateLimitRejectionAsync` reused unchanged
- [x] 3 — The class remark updated to state that the theory is the sole consumer of
      every policy on this host
- [x] 4 — `OnRejected` restored in `ApiFixture.DisableRateLimiting` by referencing the
      production handler, with the visibility choice recorded
