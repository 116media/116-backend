using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.BackgroundJobs;
using _116.Identity.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Stubs;
using _116.Tests.Fixtures.Factories.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Quartz;

namespace _116.Integration.Tests.Modules.Identity.Infrastructure.BackgroundJobs;

/// <summary>
/// Drives the real <see cref="ExpiredOtpCleanupJob" /> once against the real database and the
/// application's own dependency injection container. A scheduled job is reachable from neither an
/// HTTP route nor a repository method, so invoking <c>Execute</c> with the container's real
/// <see cref="IServiceScopeFactory" /> is its entry point: the OTP repository, the identity unit
/// of work and the interceptors the commit runs through are all resolved from the live host, and
/// nothing about the job's own collaborators is substituted. Quartz keeps job instantiation inside
/// its own job factory rather than registering the type in the container, so the registration
/// itself is asserted separately against the running scheduler.
/// </summary>
[Collection("Database")]
public class ExpiredOtpCleanupJobTests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Builds the job with the host's real scope factory, mirroring how Quartz instantiates it.
    /// </summary>
    private ExpiredOtpCleanupJob CreateJob() =>
        new(Api.Services.GetRequiredService<IServiceScopeFactory>(), NullLogger<ExpiredOtpCleanupJob>.Instance);

    [Fact]
    public async Task IdentityModule_SchedulesTheJobWithTheRunningScheduler()
    {
        var schedulerFactory = Api.Services.GetRequiredService<ISchedulerFactory>();
        IScheduler scheduler = await schedulerFactory.GetScheduler(TestContext.Current.CancellationToken);

        bool exists = await scheduler.CheckExists(
            new JobKey(nameof(ExpiredOtpCleanupJob)),
            TestContext.Current.CancellationToken
        );

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_PurgesEveryExpiredOtpAndLeavesTheLiveOnesAlone()
    {
        UserEntity user = UserFactory.CreateVerifiedActive();

        OtpEntity expiredUnusedVerification = OtpFactory.CreateExpired(user.Id, EnumOtpPurpose.EmailVerification);
        OtpEntity expiredUnusedReset = OtpFactory.CreateExpired(user.Id, EnumOtpPurpose.PasswordReset);
        OtpEntity expiredUsedRecovery = OtpFactory.CreateUsedAndExpired(
            user.Id,
            "111111",
            EnumOtpPurpose.AccountRecovery
        );
        OtpEntity liveUnused = OtpFactory.Create(user.Id, EnumOtpPurpose.TwoFactorAuthentication);
        OtpEntity liveUsed = OtpFactory.CreateUsed(user.Id, EnumOtpPurpose.EmailVerification);

        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Users.Add(user);
            ctx.Otps.AddRange(expiredUnusedVerification, expiredUnusedReset, expiredUsedRecovery, liveUnused, liveUsed);
        });

        ExpiredOtpCleanupJob job = CreateJob();

        await job.Execute(new TestJobExecutionContext(TestContext.Current.CancellationToken));

        await using IdentityDbContext identityCtx = CreateDbContext<IdentityDbContext>();
        (await identityCtx.Otps.FindAsync(expiredUnusedVerification.Id)).Should().BeNull();
        (await identityCtx.Otps.FindAsync(expiredUnusedReset.Id)).Should().BeNull();
        (await identityCtx.Otps.FindAsync(expiredUsedRecovery.Id)).Should().BeNull();
        (await identityCtx.Otps.FindAsync(liveUnused.Id)).Should().NotBeNull();
        (await identityCtx.Otps.FindAsync(liveUsed.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task Execute_WithNoExpiredOtp_RemovesNothing()
    {
        UserEntity user = UserFactory.CreateVerifiedActive();
        OtpEntity liveOtp = OtpFactory.CreateValid(user.Id);

        await SeedAsync<IdentityDbContext>(ctx =>
        {
            ctx.Users.Add(user);
            ctx.Otps.Add(liveOtp);
        });

        ExpiredOtpCleanupJob job = CreateJob();

        await job.Execute(new TestJobExecutionContext(TestContext.Current.CancellationToken));

        await using IdentityDbContext identityCtx = CreateDbContext<IdentityDbContext>();
        (await identityCtx.Otps.FindAsync(liveOtp.Id)).Should().NotBeNull();
    }
}
