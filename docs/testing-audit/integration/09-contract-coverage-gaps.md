# Low — Untested HTTP contract surface

Three pieces of the HTTP contract are configured in `Program.cs` and asserted
nowhere: the header-based API version reader, the CORS policy, and seven of the ten
named rate limit policies. None of these is a correctness bug today, which is why
this sits at the bottom of the severity table. The reason it is in the audit at all
is that the version reader and the CORS policy are, by the codebase's own dead-code
rule, indistinguishable from code that could be deleted — and the rate limit gap is
a small, cheap addition to a design that is otherwise right.

## The problem

### The header version reader is never exercised

```csharp
// src/Api/Program.cs:35-38
options.ApiVersionReader = ApiVersionReader.Combine(
    new UrlSegmentApiVersionReader(),
    new HeaderApiVersionReader("X-Api-Version")
);
```

`grep -rn "X-Api-Version" tests` returns zero results. Every integration test
addresses versions through the URL segment, via the route constants in
`tests/Fixtures/Routes/Routes.cs`. The header reader could be removed from the
`Combine` call and the entire suite would stay green.

The header form is documented as a supported contract —
`CLAUDE.md` states "Versions: URL path (`/api/v1/...`) or header
(`X-Api-Version: 1`)" — so this is a published capability with no test behind it.

### CORS is configured from unreliable input and never asserted

```csharp
// src/Api/Program.cs:54-68
string[] allowedOrigins = AppEnvironment.CorsAllowedOrigins();
builder.Services.AddCors(options =>
{
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
});
```

No test sends an `Origin` header and no test reads an `Access-Control-Allow-Origin`
response header — `grep -rn '"Origin"\|Access-Control-Allow' tests` returns zero.

Two things make this worth naming rather than shrugging at. First, the two branches
have opposite security postures: the populated branch restricts origins and permits
credentials, the empty branch permits any origin and forbids credentials. Which one
runs is decided by whether `DASHBOARD_ORIGIN` or `WEBAPP_ORIGIN` are set
(`src/Shared/Shared/Application/Configurations/Environment.cs:129-140`). Second,
`ApiFixture.SetEnvironmentVariables` (`ApiFixture.cs:66-93`) sets neither, and
[02-environment-divergence.md](02-environment-divergence.md) shows that a developer
machine's `.env` overwrites the fixture's environment anyway. So the CORS branch
under test is not merely untested, it is not even deterministic across machines.

### Seven of ten rate limit policies are unproven

The default host replaces every policy:

```csharp
// tests/Integration/Common/Fixtures/ApiFixture.cs:40
protected virtual bool DisableRateLimits => true;
```

```csharp
// tests/Integration/Common/Fixtures/ApiFixture.cs:181-203
services.Configure<RateLimiterOptions>(options =>
{
    options.RejectionStatusCode = 429;

    string[] policies =
    [
        RateLimitPolicies.Authentication,
        RateLimitPolicies.Otp,
        ...
        RateLimitPolicies.ContentContribution,
    ];

    foreach (var policy in policies)
    {
        options.AddPolicy(policy, _ => RateLimitPartition.GetNoLimiter("test"));
    }
});
```

This replacement is correct in intent and necessary in practice — 1,879 tests
sharing one host would otherwise trip the limits constantly. It does not restore the
production `OnRejected` callback
(`src/Shared/Shared/Application/Extensions/RateLimitingExtension.cs:30`), which is
harmless here because a no-op limiter never rejects, but it means the shared host's
rate limiting subsystem is entirely inert.

**The compensating design is the right one.** `RateLimitedApiFixture` overrides one
flag and inherits everything else:

```csharp
// tests/Integration/Common/Fixtures/RateLimitedApiFixture.cs:15-19
public class RateLimitedApiFixture(PostgresFixture db) : ApiFixture(db)
{
    /// <inheritdoc />
    protected override bool DisableRateLimits => false;
}
```

`RateLimitingExtensionTests` then drives one policy per algorithm tier — `Otp` for
sliding window, `DataExport` for token bucket, `ContentContribution` for fixed
window — in its own collection so permits cannot leak. Each test exhausts the exact
configured limit, asserts every request up to it was admitted, and asserts the
rejection carries a `Retry-After` header and a ProblemDetails body
(`RateLimitingExtensionTests.cs:124-147`). That is a genuinely well-built test class
and the algorithm coverage is complete: three tiers, three tests.

The gap is narrower than "seven policies are untested." `RateLimitingExtension`
registers ten policies across three configure methods (`:44-65`, `:70-85`, `:90-118`),
and the seven not driven differ from the three that are only by their constants.
What nothing asserts is that all ten are **registered**, under the names endpoints
reference, with the limits their constants declare. A policy name typo, or a policy
accidentally dropped from `ConfigureFixedWindowPolicies`, would surface as an
endpoint silently running unlimited — and no test would notice.

## Why it matters

The version reader and the CORS policy fall under a rule this codebase already
applies to `src/`: if integration coverage is near zero, the code is not wired into
anything a client reaches, and the response is to wire it up or delete it. Neither
is dead — both are reachable by a real client — but the suite cannot tell the
difference, which is the same as not knowing.

For the version header specifically, the failure mode is quiet. `AssumeDefaultVersionWhenUnspecified`
is `true` with a default of `1.0` (`Program.cs:33-34`), so a broken header reader
does not produce an error — it silently serves v1 to a client that asked for
something else. That is the kind of defect that surfaces as a client bug report
months later.

For CORS, the failure mode is a browser-only one that no server-side test or manual
`curl` will ever reproduce. It is also the config most likely to be changed under
time pressure during a deployment.

For rate limiting, the risk is a registration slip rather than an algorithm bug.
Algorithms are proven; names and numbers are not.

## What is already well covered

Worth stating plainly, because it is the same class of contract and the suite does
it properly. Request localization has 18 `Accept-Language` usages across 13 files,
including a genuine en/fr pair on the same endpoint and the same failure:

```csharp
// tests/Integration/.../Login/V1/PublicLoginEndpointV1Tests.cs:274-284
httpRequest.Headers.Add("Accept-Language", "en");

var response = await Client.SendAsync(httpRequest);

await response.ShouldBeProblem(HttpStatusCode.NotFound);

ProblemDetails problem = await response.ReadAsAsync<ProblemDetails>();
problem.Detail.Should().Contain("user account");
problem.Detail.Should().NotContain(email);
```

```csharp
// tests/Integration/.../Login/V1/PublicLoginEndpointV1Tests.cs:306-315
httpRequest.Headers.Add("Accept-Language", "fr");

var response = await Client.SendAsync(httpRequest);

await response.ShouldBeProblem(HttpStatusCode.NotFound);

ProblemDetails problem = await response.ReadAsAsync<ProblemDetails>();
problem.Detail.Should().Contain("Impossible de trouver");
problem.Detail.Should().Contain("compte utilisateur");
problem.Detail.Should().NotContain(email);
```

The French test asserts French literals against a French request. An emptied French
resource file fails it. That is the pattern the three gaps below should copy — and
notably, it is the opposite of the tautological localization tests documented in
[unit/01](../unit/01-assertions-that-cannot-fail.md), which compare the code's output
to the same localizer the code used.

## The fix

Three tests. None of them needs new infrastructure.

**One version-header test:**

```csharp
// tests/Integration/Shared/Application/Versioning/ApiVersionReaderTests.cs
/// <summary>
/// Verifies that the header arm of the combined API version reader configured in
/// Program.cs resolves a version, so that clients using X-Api-Version reach the same
/// endpoint the URL segment reaches.
/// </summary>
[Collection("Database")]
public class ApiVersionReaderTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task HeaderApiVersion_ResolvesTheSameEndpointAsTheUrlSegment()
    {
        Client.AuthenticateAsVisitor();

        var request = new HttpRequestMessage(HttpMethod.Get, Routes.Public.Me.ProfileUnversioned());
        request.Headers.Add("X-Api-Version", "1.0");

        HttpResponseMessage response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetOwnProfileResponse body = await response.ReadAsAsync<PublicGetOwnProfileResponse>();
        body.User.Id.Should().Be(TestUser.VisitorId);
    }
}
```

The route helper needs an unversioned form; if no endpoint is mapped without a URL
segment, then the header reader genuinely cannot be reached by any client and the
correct outcome is to delete it from the `Combine` call. Either answer is progress —
right now the question has not been asked.

**One CORS preflight test:**

```csharp
// tests/Integration/Shared/Application/Cors/CorsPolicyTests.cs
/// <summary>
/// Verifies that the default CORS policy built from AppEnvironment.CorsAllowedOrigins
/// admits a configured origin and refuses an unconfigured one.
/// </summary>
[Collection("Cors")]
public class CorsPolicyTests(CorsPostgresFixture db)
{
    private readonly HttpClient _client = db.Api.CreateClient();

    [Fact]
    public async Task Preflight_FromConfiguredOrigin_EchoesAllowOriginAndAllowsCredentials()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, Routes.Public.Auth.Login());
        request.Headers.Add("Origin", CorsPostgresFixture.AllowedOrigin);
        request.Headers.Add("Access-Control-Request-Method", "POST");

        HttpResponseMessage response = await _client.SendAsync(request);

        response.Headers.GetValues("Access-Control-Allow-Origin").Should().ContainSingle(CorsPostgresFixture.AllowedOrigin);
        response.Headers.GetValues("Access-Control-Allow-Credentials").Should().ContainSingle("true");
    }

    [Fact]
    public async Task Preflight_FromUnconfiguredOrigin_DoesNotEchoAllowOrigin()
    {
        var request = new HttpRequestMessage(HttpMethod.Options, Routes.Public.Auth.Login());
        request.Headers.Add("Origin", "https://not-allowed.example");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        HttpResponseMessage response = await _client.SendAsync(request);

        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
    }
}
```

This needs a dedicated fixture that sets `DASHBOARD_ORIGIN` before the host builds,
following the same pattern `RateLimitedApiFixture` uses. Its dependence on an
environment variable is precisely why it must wait for
[02-environment-divergence.md](02-environment-divergence.md) — until `.env` stops
clobbering the fixture, the test would assert one thing on CI and another locally.

**One data-driven policy test, replacing the three hand-written ones.**
`RateLimiterOptions` exposes no public accessor for a registered policy, so the only
way to assert registration without reflection is the way the existing tests already
do it — through HTTP. That is fine, because the numbers are small. The ten permit
limits are 3, 3, 5, 5, 10, 20, 30, 30, 60 and 100
(`src/BuildingBlocks/Constants/RateLimit/*RateLimitConstants.cs`), so exhausting all
ten costs 276 requests against an in-memory test server.

```csharp
// tests/Integration/Shared/Application/Extensions/RateLimitingExtensionTests.cs — after
/// <summary>
/// Drives every named policy to rejection through the endpoint that declares it, proving
/// each is registered under the name endpoints reference and enforced at the limit its
/// constants declare. Covers all three algorithm tiers, since the ten policies are split
/// across sliding window, token bucket, and fixed window configuration.
/// </summary>
/// <param name="permitLimit">The number of requests the policy admits before rejecting.</param>
/// <param name="route">A route whose endpoint declares the policy under test.</param>
[Theory]
[MemberData(nameof(Policies))]
public async Task EveryNamedPolicy_RejectsWithTooManyRequests_AtItsConfiguredLimit(
    int permitLimit,
    string route
)
{
    using HttpResponseMessage rejected = await ExhaustAsync(permitLimit, () => _client.GetAsync(route));

    await ShouldBeRateLimitRejectionAsync(rejected);
}
```

The existing `ExhaustAsync` helper (`RateLimitingExtensionTests.cs:99-116`) already
does the load-bearing work: it asserts every request up to the limit is admitted,
which is what proves the constant, and returns the one past it, which is what proves
the policy exists. The three current tests collapse into rows of `Policies`.

A `[Theory]` is the right tool here, and the audit notes only three `[MemberData]`
usages exist in a suite of 8,570 test methods — this is one of the places where the
data-driven form is unambiguously better than ten copy-pasted facts. Note that
policies are host-wide single limiters, so the theory must be the *only* consumer of
each policy on that host; that is why it replaces the three existing tests rather
than joining them.

## The principle

**A published contract needs at least one test, even when it is configuration.**
Headers, CORS, and rate limit policies are the parts of an API that clients depend on
and that no handler test touches. They are also the parts most often changed by
someone who is not thinking about tests.

**Judge coverage gaps by what a client can observe, not by line count.** The seven
unexercised rate limit policies are not seven untested features — the algorithms are
proven and the policies differ only in constants. Reporting them as seven gaps would
inflate the finding and waste effort on nine redundant exhaustion tests. The real
gap is one assertion wide, and naming it precisely is what makes it worth fixing.

## Checklist

- [ ] One test drives an endpoint through `X-Api-Version`, or the header reader is
      removed from the `Combine` call in `Program.cs`
- [ ] A CORS fixture sets `DASHBOARD_ORIGIN` before the host builds, after
      [02](02-environment-divergence.md) lands
- [ ] Preflight tests assert `Access-Control-Allow-Origin` is echoed for a
      configured origin and absent for an unconfigured one
- [ ] A `[Theory]` drives all ten `RateLimitPolicies` to rejection at their declared
      limits, replacing the three hand-written algorithm-tier tests
- [ ] `ExhaustAsync` and `ShouldBeRateLimitRejectionAsync` reused unchanged — they
      already assert the right things
