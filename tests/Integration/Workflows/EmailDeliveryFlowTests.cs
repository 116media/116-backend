using System.Text.RegularExpressions;
using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword.V1;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResendOtp.V1;
using _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword.V1;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp.V1;
using _116.Identity.Application.Auth.UseCases.Public.Commands.VerifyOtp.V1;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Stubs;
using _116.Mailer.Application.Shared.Exceptions;
using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Domain.Constants;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Mailer.Infrastructure.BackgroundJobs;
using _116.Mailer.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;
using Microsoft.Extensions.Logging.Abstractions;

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
    public async Task SignUp_OverRealHttp_EnqueuesAVerificationCodeThatVerifiesWhileTheRowStaysHashed()
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

        await using (MailerDbContext mailerContext = CreateDbContext<MailerDbContext>())
        {
            var outbox = await mailerContext.OutboxEmails.Where(o => o.RecipientAddress == email).ToListAsync();
            outbox.Should().ContainSingle(o => o.Template == "EmailVerificationOtp");
        }

        string deliveredCode = await ExtractLatestOtpCodeAsync(email);

        await using (IdentityDbContext identityContext = CreateDbContext<IdentityDbContext>())
        {
            string codeHash = await identityContext
                .Otps.Where(o => o.User.Email == email)
                .Select(o => o.CodeHash)
                .SingleAsync();
            codeHash.Should().StartWith("h1:");
            codeHash.Should().NotBe(deliveredCode);
        }

        var verifyResponse = await Client.PostAsJsonAsync(
            Routes.Public.Auth.VerifyOtp(),
            new PublicVerifyOtpRequest(
                Email: email,
                Code: deliveredCode,
                Purpose: nameof(EnumOtpPurpose.EmailVerification)
            )
        );

        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await using IdentityDbContext verifyContext = CreateDbContext<IdentityDbContext>();
        UserEntity verifiedUser = await verifyContext.Users.SingleAsync(u => u.Email == email);
        verifiedUser.IsVerified.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyOtp_WithAWrongCode_ConsumesAnAttemptAndEventuallyLocksOut()
    {
        await SeedAsync<IdentityDbContext>(context =>
            context.Roles.Add(RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor"))
        );
        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        string email = $"wrong-{Guid.NewGuid():N}@test.com";
        await SignUpAsync(email);

        string deliveredCode = await ExtractLatestOtpCodeAsync(email);
        string wrongCode = deliveredCode == "000000" ? "111111" : "000000";

        var firstAttempt = await PostVerifyOtpAsync(email, wrongCode);
        await firstAttempt.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(m => m.InvalidOtpCode())
        );

        await using (IdentityDbContext afterFirst = CreateDbContext<IdentityDbContext>())
        {
            OtpEntity row = await afterFirst.Otps.SingleAsync(o => o.User.Email == email);
            row.AttemptCount.Should().Be(1);
        }

        var secondAttempt = await PostVerifyOtpAsync(email, wrongCode);
        await secondAttempt.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(m => m.InvalidOtpCode())
        );

        var thirdAttempt = await PostVerifyOtpAsync(email, wrongCode);
        await thirdAttempt.ShouldBeProblem<OtpAttemptsLimitException>(
            HttpStatusCode.TooManyRequests,
            Localized<ValidationErrorMessage>(m => m.MaxOtpAttemptsReached())
        );

        await using IdentityDbContext verifyContext = CreateDbContext<IdentityDbContext>();
        OtpEntity exhausted = await verifyContext.Otps.SingleAsync(o => o.User.Email == email);
        exhausted.AttemptCount.Should().Be(3);
    }

    [Fact]
    public async Task PasswordReset_ForgotThenVerifyThenReset_CompletesAgainstHashedRows()
    {
        await SeedAsync<IdentityDbContext>(context =>
            context.Roles.Add(RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor"))
        );
        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        string email = $"reset-{Guid.NewGuid():N}@test.com";
        await SignUpAsync(email);

        string verificationCode = await ExtractLatestOtpCodeAsync(email);
        var verified = await PostVerifyOtpAsync(email, verificationCode);
        verified.StatusCode.Should().Be(HttpStatusCode.OK);

        var forgotResponse = await Client.PostAsJsonAsync(
            Routes.Public.Auth.ForgotPassword(),
            new PublicForgotPasswordRequest(email)
        );
        forgotResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        string resetCode = await ExtractLatestOtpCodeAsync(email, "PasswordResetOtp");

        var verifyResetCode = await Client.PostAsJsonAsync(
            Routes.Public.Auth.VerifyOtp(),
            new PublicVerifyOtpRequest(Email: email, Code: resetCode, Purpose: nameof(EnumOtpPurpose.PasswordReset))
        );
        verifyResetCode.StatusCode.Should().Be(HttpStatusCode.OK);

        var resetResponse = await Client.PostAsJsonAsync(
            Routes.Public.Auth.ResetPassword(),
            new PublicResetPasswordRequest(Email: email, Code: resetCode, NewPassword: TestAuth.ResetNewPassword)
        );

        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var passwordService = Api.Services.GetRequiredService<IPasswordService>();
        await using IdentityDbContext verifyContext = CreateDbContext<IdentityDbContext>();
        UserEntity updated = await verifyContext.Users.SingleAsync(u => u.Email == email);
        passwordService.Verify(TestAuth.ResetNewPassword, updated.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task ResendOtp_InvalidatesThePreviouslyDeliveredCode()
    {
        await SeedAsync<IdentityDbContext>(context =>
            context.Roles.Add(RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor"))
        );
        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());

        string email = $"resend-{Guid.NewGuid():N}@test.com";
        await SignUpAsync(email);

        string firstCode = await ExtractLatestOtpCodeAsync(email);

        var resendResponse = await Client.PostAsJsonAsync(
            Routes.Public.Auth.ResendOtp(),
            new PublicResendOtpRequest(Email: email, Purpose: nameof(EnumOtpPurpose.EmailVerification))
        );
        resendResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        string secondCode = await ExtractLatestOtpCodeAsync(email);
        secondCode.Should().NotBe(firstCode);

        var staleAttempt = await PostVerifyOtpAsync(email, firstCode);
        await staleAttempt.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(m => m.InvalidOtpCode())
        );

        var freshAttempt = await PostVerifyOtpAsync(email, secondCode);
        freshAttempt.StatusCode.Should().Be(HttpStatusCode.OK);

        await using IdentityDbContext verifyContext = CreateDbContext<IdentityDbContext>();
        UserEntity verifiedUser = await verifyContext.Users.SingleAsync(u => u.Email == email);
        verifiedUser.IsVerified.Should().BeTrue();
    }

    /// <summary>
    /// Registers a user over real HTTP so the flow under test starts from a genuine signup.
    /// </summary>
    private async Task SignUpAsync(string email)
    {
        var response = await Client.PostAsJsonAsync(
            Routes.Public.Auth.SignUp(),
            new PublicSignUpRequest(
                Email: email,
                UserName: $"m{Guid.NewGuid():N}"[..10],
                Password: TestAuth.ValidPassword
            )
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>
    /// Reads the most recently enqueued OTP email for a recipient and pulls the six-digit code out
    /// of its rendered text body. The outbox body is the only remaining place the plaintext exists,
    /// which is precisely why a test that needs the code has to read it from here.
    /// </summary>
    private async Task<string> ExtractLatestOtpCodeAsync(string email, string template = "EmailVerificationOtp")
    {
        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        OutboxEmailEntity latest = await mailerContext
            .OutboxEmails.Where(o => o.RecipientAddress == email && o.Template == template)
            .OrderByDescending(o => o.CreatedAt)
            .FirstAsync();

        MatchCollection matches = Regex.Matches(latest.TextBody, @"\b\d{6}\b");
        matches.Should().ContainSingle("the OTP email body should carry exactly one six-digit code");
        return matches[0].Value;
    }

    /// <summary>
    /// Submits a code to the public email-verification endpoint.
    /// </summary>
    private Task<HttpResponseMessage> PostVerifyOtpAsync(string email, string code)
    {
        return Client.PostAsJsonAsync(
            Routes.Public.Auth.VerifyOtp(),
            new PublicVerifyOtpRequest(Email: email, Code: code, Purpose: nameof(EnumOtpPurpose.EmailVerification))
        );
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_Returns200AndEnqueuesNothing()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Public.Auth}/forgot-password",
            new PublicForgotPasswordRequest("ghost@nowhere.example")
        );

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

        var job = new OutboxEmailDispatcherJob(
            Api.Services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<OutboxEmailDispatcherJob>.Instance
        );
        await job.Execute(new TestJobExecutionContext());

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
        stub.NextFailure = new EmailDeliveryException("smtp down");

        var job = new OutboxEmailDispatcherJob(
            Api.Services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<OutboxEmailDispatcherJob>.Instance
        );
        await job.Execute(new TestJobExecutionContext());

        await using MailerDbContext verify = CreateDbContext<MailerDbContext>();
        OutboxEmailEntity retried = await verify.OutboxEmails.SingleAsync(o => o.Id == emailId);
        retried.Status.Should().Be(EnumOutboxEmailStatus.Pending);
        retried.AttemptCount.Should().Be(1);
        retried.NextAttemptAt.Should().BeAfter(DateTime.UtcNow);
        retried.LastError.Should().Contain("smtp down");
    }

    [Fact]
    public async Task Dispatcher_PermanentFailure_MarksTheRowFailedWithoutSchedulingARetry()
    {
        Guid emailId = Guid.NewGuid();
        DateTime enqueuedAt = DateTime.UtcNow.AddSeconds(-5);
        await SeedAsync<MailerDbContext>(ctx =>
            ctx.OutboxEmails.Add(
                OutboxEmailEntity.Enqueue(
                    id: emailId,
                    recipientAddress: "rejected@example.com",
                    recipientName: null,
                    subject: "Reject me",
                    htmlBody: "<p>x</p>",
                    textBody: "x",
                    template: "Welcome",
                    now: enqueuedAt
                )
            )
        );

        await using (MailerDbContext seeded = CreateDbContext<MailerDbContext>())
        {
            enqueuedAt = (await seeded.OutboxEmails.SingleAsync(o => o.Id == emailId)).NextAttemptAt;
        }

        StubEmailSender stub = Api.Services.GetRequiredService<StubEmailSender>();
        stub.NextFailure = new EmailDeliveryException("mailbox does not exist", isTransient: false);

        var job = new OutboxEmailDispatcherJob(
            Api.Services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<OutboxEmailDispatcherJob>.Instance
        );
        await job.Execute(new TestJobExecutionContext());

        await using MailerDbContext verify = CreateDbContext<MailerDbContext>();
        OutboxEmailEntity failed = await verify.OutboxEmails.SingleAsync(o => o.Id == emailId);
        failed.Status.Should().Be(EnumOutboxEmailStatus.Failed);
        failed.AttemptCount.Should().Be(1);
        failed.NextAttemptAt.Should().Be(enqueuedAt);
        failed.LastError.Should().Contain("mailbox does not exist");
        failed.SentAt.Should().BeNull();
    }

    [Fact]
    public async Task Dispatcher_OverlongProviderError_TruncatesItToTheStorageLimit()
    {
        Guid emailId = Guid.NewGuid();
        await SeedAsync<MailerDbContext>(ctx =>
            ctx.OutboxEmails.Add(
                OutboxEmailEntity.Enqueue(
                    id: emailId,
                    recipientAddress: "verbose@example.com",
                    recipientName: null,
                    subject: "Verbose failure",
                    htmlBody: "<p>x</p>",
                    textBody: "x",
                    template: "Welcome",
                    now: DateTime.UtcNow.AddSeconds(-5)
                )
            )
        );

        const string marker = "OVERLONG";
        string overlongError = marker + new string('e', MailerConstants.MaxLastErrorLength);

        StubEmailSender stub = Api.Services.GetRequiredService<StubEmailSender>();
        stub.NextFailure = new EmailDeliveryException(overlongError);

        var job = new OutboxEmailDispatcherJob(
            Api.Services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<OutboxEmailDispatcherJob>.Instance
        );
        await job.Execute(new TestJobExecutionContext());

        await using MailerDbContext verify = CreateDbContext<MailerDbContext>();
        OutboxEmailEntity truncated = await verify.OutboxEmails.SingleAsync(o => o.Id == emailId);
        truncated.LastError.Should().NotBeNull();
        truncated.LastError!.Length.Should().Be(MailerConstants.MaxLastErrorLength);
        truncated.LastError.Should().StartWith(marker);
        truncated.AttemptCount.Should().Be(1);
    }
}
