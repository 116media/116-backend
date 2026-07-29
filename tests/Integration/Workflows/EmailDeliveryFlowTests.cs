using _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword.V1;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp.V1;
using _116.Identity.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Stubs;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Mailer.Infrastructure.BackgroundJobs;
using _116.Mailer.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Quartz;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// End-to-end flows for email delivery: real HTTP triggers persist outbox rows
/// atomically with their business change, and the real dispatcher drains them
/// through the stubbed provider.
/// </summary>
[Collection("Database")]
public class EmailDeliveryFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SignUp_OverRealHttp_EnqueuesTheVerificationOtpEmail()
    {
        await SeedAsync<IdentityDbContext>(context =>
            context.Roles.Add(RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor"))
        );
        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        string email = $"mail-{Guid.NewGuid():N}@test.com";
        var response = await Client.PostAsJsonAsync(
            Routes.Public.Auth.SignUp(),
            new PublicSignUpRequest(
                Email: email,
                UserName: $"m{Guid.NewGuid():N}"[..10],
                Password: TestAuth.ValidPassword
            )
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        await using IdentityDbContext identityContext = CreateDbContext<IdentityDbContext>();

        var outbox = await mailerContext.OutboxEmails.Where(o => o.RecipientAddress == email).ToListAsync();
        outbox.Should().ContainSingle(o => o.Template == "EmailVerificationOtp");

        // The email carries the exact code that was persisted for the user.
        string otpCode = await identityContext.Otps.Where(o => o.User.Email == email).Select(o => o.Code).SingleAsync();
        outbox[0].TextBody.Should().Contain(otpCode);
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_Returns200AndEnqueuesNothing()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Public.Auth}/forgot-password",
            new PublicForgotPasswordRequest("ghost@nowhere.example")
        );

        // Enumeration-safety: the neutral 200 comes with zero outbox rows.
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using MailerDbContext ctx = CreateDbContext<MailerDbContext>();
        (await ctx.OutboxEmails.CountAsync(o => o.RecipientAddress == "ghost@nowhere.example")).Should().Be(0);
    }

    [Fact]
    public async Task Dispatcher_DrainsAPendingRowThroughTheProviderAndMarksItSent()
    {
        Guid emailId = Guid.NewGuid();
        await SeedAsync<MailerDbContext>(ctx =>
            ctx.OutboxEmails.Add(
                OutboxEmailEntity.Enqueue(
                    id: emailId,
                    recipientAddress: "drain@example.com",
                    recipientName: null,
                    subject: "Drain me",
                    htmlBody: "<p>x</p>",
                    textBody: "x",
                    template: "Welcome",
                    now: DateTime.UtcNow.AddSeconds(-5)
                )
            )
        );

        StubEmailSender stub = Api.Services.GetRequiredService<StubEmailSender>();
        int alreadySent = stub.Sent.Count;

        // The real job, driven once: claims with skip-locked, delivers through
        // the (stubbed) provider, records the outcome.
        var job = new OutboxEmailDispatcherJob(
            Api.Services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<OutboxEmailDispatcherJob>.Instance
        );
        await job.Execute(new Moq.Mock<IJobExecutionContext>().Object);

        stub.Sent.Count.Should().BeGreaterThan(alreadySent);
        stub.Sent.Should().Contain(m => m.To.Address == "drain@example.com" && m.Subject == "Drain me");

        await using MailerDbContext verify = CreateDbContext<MailerDbContext>();
        OutboxEmailEntity drained = await verify.OutboxEmails.SingleAsync(o => o.Id == emailId);
        drained.Status.Should().Be(EnumOutboxEmailStatus.Sent);
        drained.SentAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Dispatcher_TransientFailure_SchedulesARetryInsteadOfFailing()
    {
        Guid emailId = Guid.NewGuid();
        await SeedAsync<MailerDbContext>(ctx =>
            ctx.OutboxEmails.Add(
                OutboxEmailEntity.Enqueue(
                    id: emailId,
                    recipientAddress: "retry@example.com",
                    recipientName: null,
                    subject: "Retry me",
                    htmlBody: "<p>x</p>",
                    textBody: "x",
                    template: "Welcome",
                    now: DateTime.UtcNow.AddSeconds(-5)
                )
            )
        );

        StubEmailSender stub = Api.Services.GetRequiredService<StubEmailSender>();
        stub.NextFailure = new _116.Mailer.Application.Shared.Exceptions.EmailDeliveryException("smtp down");

        var job = new OutboxEmailDispatcherJob(
            Api.Services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<OutboxEmailDispatcherJob>.Instance
        );
        await job.Execute(new Moq.Mock<IJobExecutionContext>().Object);

        await using MailerDbContext verify = CreateDbContext<MailerDbContext>();
        OutboxEmailEntity retried = await verify.OutboxEmails.SingleAsync(o => o.Id == emailId);
        retried.Status.Should().Be(EnumOutboxEmailStatus.Pending);
        retried.AttemptCount.Should().Be(1);
        retried.NextAttemptAt.Should().BeAfter(DateTime.UtcNow);
        retried.LastError.Should().Contain("smtp down");
    }
}
