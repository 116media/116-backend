using _116.Identity.Application.Roles.Constants;
using _116.Identity.Application.User.UseCases.Admin.Commands.AssignRoleToUser.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// End-to-end flows for the identity security reactions: a password reset invalidates sessions, a
/// role change bumps the target user's token version without killing sessions, and an email change
/// produces the dual alert and confirmation emails while preserving the acting session.
/// </summary>
[Collection("Database")]
public class IdentitySecurityEventFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ResetPassword_OverRealHttp_RevokesTheSeededSession()
    {
        var userId = Guid.NewGuid();
        string email = $"reset-revoke-{userId:N}@test.com";
        var sessionId = Guid.NewGuid();

        await SeedAsync<IdentityDbContext>(context =>
        {
            var user = UserFactory.CreateWithId(userId, email);
            user.MarkAsVerified();
            user.Activate();

            context.Users.Add(user);
            context.Otps.Add(OtpFactory.CreateUsed(userId, Otp.ValidCode, EnumOtpPurpose.PasswordReset));
            context.Sessions.Add(SessionFactory.CreateWithId(sessionId, userId));
        });

        Client.ClearAuthentication();

        var request = new PublicResetPasswordRequestBuilder()
            .WithEmail(email)
            .WithCode(Otp.ValidCode)
            .WithNewPassword(TestAuth.ResetNewPassword)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Auth.ResetPassword(), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using IdentityDbContext identityContext = CreateDbContext<IdentityDbContext>();
        SessionEntity session = await identityContext.Sessions.SingleAsync(s => s.Id == sessionId);
        session.IsRevoked.Should().BeTrue();

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        var outbox = await mailerContext.OutboxEmails.Where(o => o.RecipientAddress == email).ToListAsync();
        outbox.Should().ContainSingle(o => o.Template == "PasswordResetCompleted");

        List<NotificationEntity> notifications = await mailerContext
            .Notifications.Where(n => n.UserId == userId)
            .ToListAsync();
        notifications.Should().ContainSingle(n => n.Type == EnumNotificationType.PasswordResetCompleted);
    }

    [Fact]
    public async Task RemoveRoleFromUser_OverRealHttp_BumpsTheTokenVersionAndKeepsTheSession()
    {
        var sessionId = Guid.NewGuid();
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(context =>
        {
            RoleEntity created = RoleFactory.Create();
            context.Roles.Add(created);
            context.UserRoles.Add(UserRoleFactory.Create(TestUser.VisitorId, created.Id));
            context.Sessions.Add(SessionFactory.CreateWithId(sessionId, TestUser.VisitorId));
            return created;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(
            $"{ApiRoutes.Admin.Users}/{TestUser.VisitorId}/{RoleRouteConstants.Endpoint}/{role.Id}"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // The revocation lands through the token-version bump, not a session kill.
        await using IdentityDbContext identityContext = CreateDbContext<IdentityDbContext>();
        SessionEntity session = await identityContext.Sessions.SingleAsync(s => s.Id == sessionId);
        session.IsRevoked.Should().BeFalse();

        UserTokenStateEntity tokenState = await identityContext.UserTokenStates.SingleAsync(s =>
            s.Id == TestUser.VisitorId
        );
        tokenState.TokenVersion.Should().Be(1);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        var outbox = await mailerContext
            .OutboxEmails.Where(o => o.RecipientAddress == TestUser.VisitorEmail)
            .ToListAsync();
        outbox.Should().ContainSingle(o => o.Template == "RoleChanged");

        List<NotificationEntity> notifications = await mailerContext
            .Notifications.Where(n => n.UserId == TestUser.VisitorId)
            .ToListAsync();
        notifications.Should().ContainSingle(n => n.Type == EnumNotificationType.RoleChanged);
    }

    [Fact]
    public async Task AssignRoleToUser_OverRealHttp_BumpsTheTokenVersionAndKeepsTheSession()
    {
        var sessionId = Guid.NewGuid();
        RoleEntity role = await SeedAsync<IdentityDbContext, RoleEntity>(context =>
        {
            RoleEntity created = RoleFactory.Create();
            context.Roles.Add(created);
            context.Sessions.Add(SessionFactory.CreateWithId(sessionId, TestUser.VisitorId));
            return created;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Users}/{TestUser.VisitorId}/{RoleRouteConstants.Endpoint}",
            new AdminAssignRoleToUserRequest(RoleId: role.Id)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // The grant lands through the token-version bump, not a session kill.
        await using IdentityDbContext identityContext = CreateDbContext<IdentityDbContext>();
        SessionEntity session = await identityContext.Sessions.SingleAsync(s => s.Id == sessionId);
        session.IsRevoked.Should().BeFalse();

        UserTokenStateEntity tokenState = await identityContext.UserTokenStates.SingleAsync(s =>
            s.Id == TestUser.VisitorId
        );
        tokenState.TokenVersion.Should().Be(1);
    }

    [Fact]
    public async Task ProfileEmailChange_OverRealHttp_ProducesTheDualEmailsAndPreservesTheActingSession()
    {
        var sessionId = Guid.NewGuid();
        var otherSessionId = Guid.NewGuid();
        string newEmail = $"fresh-{Guid.NewGuid():N}@example.com";

        await SeedAsync<IdentityDbContext>(context =>
        {
            context.Sessions.Add(SessionFactory.CreateWithId(sessionId, TestUser.VisitorId));
            context.Sessions.Add(SessionFactory.CreateWithId(otherSessionId, TestUser.VisitorId));
        });

        Client.AuthenticateAs(TestUser.VisitorId, "Visitor", sessionId);

        var request = new PublicUpdateOwnProfileRequestBuilder().WithEmail(newEmail).Build();

        var response = await Client.PatchAsJsonAsync(Routes.Public.Me.Profile(), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        var oldAddressRows = await mailerContext
            .OutboxEmails.Where(o => o.RecipientAddress == TestUser.VisitorEmail)
            .ToListAsync();
        oldAddressRows.Should().ContainSingle(o => o.Template == "EmailChangedAlertOld");

        var newAddressRows = await mailerContext.OutboxEmails.Where(o => o.RecipientAddress == newEmail).ToListAsync();
        newAddressRows.Should().ContainSingle(o => o.Template == "EmailChangedConfirmNew");

        List<NotificationEntity> notifications = await mailerContext
            .Notifications.Where(n => n.UserId == TestUser.VisitorId)
            .ToListAsync();
        notifications.Should().ContainSingle(n => n.Type == EnumNotificationType.EmailChanged);

        await using IdentityDbContext identityContext = CreateDbContext<IdentityDbContext>();
        SessionEntity acting = await identityContext.Sessions.SingleAsync(s => s.Id == sessionId);
        SessionEntity other = await identityContext.Sessions.SingleAsync(s => s.Id == otherSessionId);
        acting.IsRevoked.Should().BeFalse();
        other.IsRevoked.Should().BeTrue();
    }
}
