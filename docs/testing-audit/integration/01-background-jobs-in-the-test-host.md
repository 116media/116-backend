# Critical — Background jobs run live inside the test host

A real Quartz scheduler starts with the integration test host and keeps running
for the entire suite, mutating the same four schemas every test asserts against.
Four tests already race it. This is the only finding in the audit that makes
currently-green tests fail non-deterministically rather than merely fail to catch
things.

## The problem

`AddQuartzHostedService` is registered by production module wiring:

```csharp
// src/Shared/Shared/Application/Extensions/QuartzExtension.cs:38
services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
```

Four jobs are scheduled on it:

| Job | Cron | Registered in |
| --- | --- | --- |
| `OutboxEmailDispatcherJob` | `0/15 * * * * ?` — **every 15 seconds** | `MailerModule.cs:73` |
| `ExpiredOtpCleanupJob` | hourly | `IdentityModule.cs:178` |
| `AbandonedDraftCleanupJob` | hourly | `ContentModule.cs:239` |
| `ShortVideoViewEventCleanupJob` | daily | `ContentModule.cs:240` |

`ApiFixture.ConfigureTestServices` replaces DbContexts, external services, JWT
validation and rate limits. It removes **no** `IHostedService`. Verified: the
string `IHostedService` does not appear in `ApiFixture.cs`.

The suite itself proves the scheduler is live — this assertion only passes because
it is:

```csharp
// tests/Integration/Modules/Content/Infrastructure/BackgroundJobs/AbandonedDraftCleanupJobTests.cs:40-50
var schedulerFactory = Api.Services.GetRequiredService<ISchedulerFactory>();
IScheduler scheduler = await schedulerFactory.GetScheduler(TestContext.Current.CancellationToken);
bool exists = await scheduler.CheckExists(new JobKey(nameof(AbandonedDraftCleanupJob)), ...);
exists.Should().BeTrue();
```

## Why it matters

A full integration run takes about three minutes, so the outbox dispatcher fires
roughly **twelve times per run**. `OutboxEmailDispatcherJob.Execute` opens a
transaction, claims any due pending row with `FOR UPDATE SKIP LOCKED`, sends it
through the singleton `StubEmailSender`, and commits — concurrently with whatever
test is executing.

Three concrete races exist today.

**The seeded row is stolen before the manual run.**

```csharp
// tests/Integration/Workflows/EmailDeliveryFlowTests.cs:288-324
// seeds a row due 5 seconds ago, then:
int alreadySent = stub.Sent.Count;
await job.Execute(new TestJobExecutionContext());
stub.Sent.Count.Should().BeGreaterThan(alreadySent);
```

If the background trigger fires between the seed and the manual `Execute`, the row
is already `Sent`, the manual run claims an empty batch, and `BeGreaterThan` fails.

**The one-shot failure is consumed by the wrong caller.**

```csharp
// tests/Integration/Workflows/EmailDeliveryFlowTests.cs:326-360
stub.NextFailure = new EmailDeliveryException("smtp down");
// ... expects the row to remain Pending for retry
retried.Status.Should().Be(EnumOutboxEmailStatus.Pending);
```

`NextFailure` is a one-shot flag on a **singleton**. A background tick between
arming and asserting consumes the failure, marks the row `Sent`, and the test fails
with a message that points nowhere near the cause.

**Cleanup jobs delete rows mid-test.** The hourly `ExpiredOtpCleanupJob` purges
expired OTP rows. Any run that crosses the top of an hour can have rows deleted out
from under the OTP flow tests — a failure that reproduces roughly once a day and
never locally.

Beyond the named races, every background tick competes with Respawn's truncation
of the `mailer` schema for locks.

## The fix

Remove the hosted service, keep the scheduler object.

```csharp
// tests/Integration/Common/Fixtures/ApiFixture.cs — ConfigureTestServices

builder.ConfigureTestServices(services =>
{
    ReplaceDbContexts(services);
    StubExternalServices(services);
    OverrideJwtAuthentication(services);
    DisableScheduledJobs(services);
    if (DisableRateLimits) DisableRateLimiting(services);
});

/// <summary>
/// Removes the Quartz hosted service so no scheduled trigger runs concurrently
/// with a test. Job behaviour stays covered: the job tests resolve the real
/// collaborators and invoke Execute once, and assert registration through
/// ISchedulerFactory, which still reports every job key.
/// </summary>
private static void DisableScheduledJobs(IServiceCollection services)
{
    foreach (
        ServiceDescriptor descriptor in services
            .Where(d =>
                d.ServiceType == typeof(IHostedService)
                && d.ImplementationType?.Name == "QuartzHostedService"
            )
            .ToList()
    )
    {
        services.Remove(descriptor);
    }
}
```

The existing registration assertions keep working, because `ISchedulerFactory`
still knows about every scheduled job — only the thing that *fires* triggers is
gone.

## The principle

**A test host must not contain anything that mutates state on a timer.** Test
isolation assumes that between arrange and assert, nothing happens except the act.
A background scheduler breaks that assumption globally and silently, and the
resulting failures are attributed to whatever test happened to be running.

Scheduled work is still testable, and this suite already does it correctly: resolve
the job's real collaborators from the container, invoke `Execute` once,
deterministically, and assert the outcome. Separately assert that the module
scheduled the job. Those two tests together prove more than a live scheduler ever
could, and they prove it reproducibly.

## Checklist

- [ ] `DisableScheduledJobs` added to `ApiFixture`
- [ ] `EmailDeliveryFlowTests` no longer reads `stub.Sent.Count` as a baseline
      (it exists only to tolerate background sends)
- [ ] Job registration assertions still pass via `ISchedulerFactory`
- [ ] Full integration suite run twice back to back with identical results
