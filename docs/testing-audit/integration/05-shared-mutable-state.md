# High — Shared mutable stubs leak between tests

The external-service stubs are singletons, and two of them carry one-shot failure
flags while three carry unbounded accumulators. Nothing resets any of it between
tests. `BaseApiTest.InitializeAsync` resets the database and three in-memory caches
and touches none of the stubs, so the isolation the fixture provides stops exactly
where the stubs begin.

## The problem

Two stubs are registered as singletons so that failure injection reaches the same
instance the request pipeline resolves:

```csharp
// tests/Integration/Common/Fixtures/ApiFixture.cs:230-231
services.AddSingleton<StubCloudinaryService>();
services.AddSingleton<ICloudinaryService>(sp => sp.GetRequiredService<StubCloudinaryService>());

// tests/Integration/Common/Fixtures/ApiFixture.cs:246-247
services.AddSingleton<StubEmailSender>();
services.AddSingleton<IEmailSender>(sp => sp.GetRequiredService<StubEmailSender>());
```

That registration choice is correct and deliberate — the doc comments at
`ApiFixture.cs:217-221` and `:234-237` explain why. The problem is what the
singletons hold.

**One-shot failure flags survive the test that armed them:**

```csharp
// tests/Integration/Common/Stubs/StubCloudinaryService.cs:19
public Exception? NextDeleteFailure { get; set; }

// tests/Integration/Common/Stubs/StubEmailSender.cs:24
public EmailDeliveryException? NextFailure { get; set; }
```

Both are consumed on first use and cleared (`StubCloudinaryService.cs:82-90`,
`StubEmailSender.cs:29-34`). If nothing consumes them, they stay armed.

**Accumulators grow for the whole run:**

```csharp
// tests/Integration/Common/Stubs/StubCloudinaryService.cs:28
public List<string> DeletedPublicIds { get; } = [];

// tests/Integration/Common/Stubs/StubEmailSender.cs:18
public List<EmailMessage> Sent { get; } = [];
```

**And one stub keeps its state in `static` fields**, which means it is shared even
more widely than a singleton — across every host, including the separate
rate-limited one:

```csharp
// tests/Integration/Common/Stubs/StubStreamingLinkResolutionService.cs:18,24
public static IReadOnlyDictionary<EnumStreamingPlatform, string> NextResult { get; set; } = DefaultResult();
public static StreamingLinkResolutionException? NextException { get; set; }
```

Meanwhile the per-test reset knows nothing about any of them:

```csharp
// tests/Integration/Common/Base/BaseApiTest.cs:117-126
/// <inheritdoc />
public async ValueTask InitializeAsync()
{
    await Db.ResetAsync();
    InvalidateTagCache();
    InvalidatePopularArticlesCache();
    InvalidatePopularVideosCache();
    await SeedTestUsersAsync();
    await SeedAsync();
}
```

Three caches get explicit invalidation, each with a doc comment explaining that the
shared `ApiFixture` singleton outlives `ResetAsync`. The same reasoning applies to
the stubs and was not carried across.

## Why it matters

**An armed one-shot outlives a failed test.** Six tests arm the Cloudinary failure,
none of them in a `try`/`finally` — `grep -n "finally" ExternalAssetCleanupFlowTests.cs`
returns nothing:

```csharp
// tests/Integration/Workflows/ExternalAssetCleanupFlowTests.cs:68, 101, 136, 165, 196, 238
CloudinaryStub.NextDeleteFailure = new InvalidOperationException("cloudinary down");
```

Each of those tests arms the flag and then expects the request under test to consume
it. If an assertion between the arming and the consumption fails — or if the request
takes a path that does not delete an asset — the exception stays queued on the
singleton. The next test in the collection that deletes any image gets an
`InvalidOperationException("cloudinary down")` from nowhere. The reported failure
names a test that is not the one at fault, and the message points at a provider that
does not exist.

This failure mode compounds with
[01-background-jobs-in-the-test-host.md](01-background-jobs-in-the-test-host.md):
the outbox dispatcher fires every 15 seconds against the same singleton
`StubEmailSender`, so `NextFailure` can be consumed by a background tick that no
test knows about.

**The suite has already absorbed the accumulation rather than fixing it.** This is
the tell:

```csharp
// tests/Integration/Workflows/EmailDeliveryFlowTests.cs:306-307
StubEmailSender stub = Api.Services.GetRequiredService<StubEmailSender>();
int alreadySent = stub.Sent.Count;
...
// tests/Integration/Workflows/EmailDeliveryFlowTests.cs:317
stub.Sent.Count.Should().BeGreaterThan(alreadySent);
```

The baseline exists because `Sent` is never cleared. With a per-test reset the
assertion becomes `stub.Sent.Should().ContainSingle()`, which is a far stronger
claim: it proves the dispatcher sent *one* message, not that the count went up by an
unspecified amount from an unknown starting point. Working around leaked state costs
assertion strength every time.

**The static stub is hand-managed at every call site.** Two test files touch
`StubStreamingLinkResolutionService` a combined 21 times, of which 13 are explicit
`Reset()` calls placed as the first line of a test's arrange block
(`AdminResolveSingleStreamingLinksEndpointV1Tests.cs:42, 83, 111, 129` and
`AdminResolveAlbumStreamingLinksEndpointV1Tests.cs:39, 53, 71, 99, 131, 170, 190, 209, 227`).
The stub's own doc comment concedes the arrangement:

```csharp
// tests/Integration/Common/Stubs/StubStreamingLinkResolutionService.cs:9-10
/// Behaviour is scripted per test via the static hooks and reset in
/// each test's arrange step — the same external-service stub pattern as Cloudinary.
```

That is a convention enforced by 13 remembered calls. The first test written without
one inherits the previous test's scripted exception, and the failure surfaces in a
different file.

## The fix

Give the stubs a common reset contract, register them under it, and drive it from
the base class that already owns per-test isolation.

```csharp
// tests/Integration/Common/Stubs/IResettableStub.cs
/// <summary>
/// Implemented by every external-service stub that carries state across requests.
/// The integration base classes clear all registered stubs before each test, so a
/// queued failure or a recorded call can never be observed by the next test.
/// </summary>
public interface IResettableStub
{
    /// <summary>
    /// Returns the stub to the state it had when the host was first built.
    /// </summary>
    void Reset();
}
```

```csharp
// tests/Integration/Common/Stubs/StubEmailSender.cs — after
public class StubEmailSender : IEmailSender, IResettableStub
{
    public List<EmailMessage> Sent { get; } = [];

    public EmailDeliveryException? NextFailure { get; set; }

    /// <inheritdoc />
    public void Reset()
    {
        Sent.Clear();
        NextFailure = null;
    }

    // SendAsync unchanged
}
```

`StubCloudinaryService` gets the same treatment for `DeletedPublicIds` and
`NextDeleteFailure`. `StubStreamingLinkResolutionService` converts its two `static`
properties to instance properties and drops its `static Reset()`; the fixture
already registers it as a service (`ApiFixture.cs:213`), so instance state is
reachable — it only needs to move from `AddScoped` to the singleton registration the
other two stubs use, for the same reason they use it.

Register them once, under the interface:

```csharp
// tests/Integration/Common/Fixtures/ApiFixture.cs — after
services.AddSingleton<StubCloudinaryService>();
services.AddSingleton<ICloudinaryService>(sp => sp.GetRequiredService<StubCloudinaryService>());
services.AddSingleton<IResettableStub>(sp => sp.GetRequiredService<StubCloudinaryService>());
```

Then one loop replaces every manual reset in the suite:

```csharp
// tests/Integration/Common/Base/BaseApiTest.cs — after
/// <inheritdoc />
public async ValueTask InitializeAsync()
{
    await Db.ResetAsync();
    ResetStubs();
    InvalidateTagCache();
    InvalidatePopularArticlesCache();
    InvalidatePopularVideosCache();
    await SeedTestUsersAsync();
    await SeedAsync();
}

/// <summary>
/// Clears every external-service stub before each test. The stubs are singletons in the
/// shared <see cref="ApiFixture" />, so queued one-shot failures and recorded calls outlive
/// <see cref="PostgresFixture.ResetAsync" /> exactly as the in-memory caches do.
/// </summary>
private void ResetStubs()
{
    using var scope = Api.Services.CreateScope();

    foreach (IResettableStub stub in scope.ServiceProvider.GetServices<IResettableStub>())
    {
        stub.Reset();
    }
}
```

`BaseRepositoryTest.InitializeAsync` gets the identical two lines. New stubs are
covered the moment they implement the interface, with no edit to either base class.

Then delete what the leak forced:

```csharp
// tests/Integration/Workflows/EmailDeliveryFlowTests.cs — before
int alreadySent = stub.Sent.Count;
await job.Execute(new TestJobExecutionContext());
stub.Sent.Count.Should().BeGreaterThan(alreadySent);

// after
await job.Execute(new TestJobExecutionContext());
stub.Sent.Should().ContainSingle(m => m.To.Address == "drain@example.com" && m.Subject == "Drain me");
```

and the 13 `StubStreamingLinkResolutionService.Reset()` calls, which the base class
now performs.

## The principle

**Anything that survives a test must be reset by the harness, not by the test.** A
per-test reset that covers the database and the caches but not the stubs is not a
partial solution — it is a worse one, because it establishes that isolation is
handled and stops anyone from checking.

The corollary applies to every reset convention enforced by comment: if the correct
behaviour is "call `Reset()` first," a test that forgets is indistinguishable from
one that passes, and the cost lands on some other file. Move the obligation into a
place that cannot be forgotten.

Finally, **a baseline read is a symptom worth chasing**. `int alreadySent = stub.Sent.Count`
is a test declaring in code that it does not control its own arrangement. Wherever
one appears, the fix is upstream of the assertion.

## Checklist

- [ ] `IResettableStub` added and implemented by `StubCloudinaryService`,
      `StubEmailSender`, and `StubStreamingLinkResolutionService`
- [ ] `StubStreamingLinkResolutionService` holds instance state, not `static`, and is
      registered as a singleton
- [ ] Each stub registered under `IResettableStub` in `ApiFixture`
- [ ] `ResetStubs()` called from both `BaseApiTest.InitializeAsync` and
      `BaseRepositoryTest.InitializeAsync`
- [ ] All 13 `StubStreamingLinkResolutionService.Reset()` calls removed from test bodies
- [ ] `EmailDeliveryFlowTests` asserts exact counts instead of reading a baseline
- [ ] The six `ExternalAssetCleanupFlowTests` arm sites need no `finally`, because a
      leaked one-shot can no longer reach the next test
