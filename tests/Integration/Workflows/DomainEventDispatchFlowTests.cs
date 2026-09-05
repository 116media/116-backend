using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Mailer.Contracts.Domain;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Requests.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// End-to-end proof of the post-commit domain event pipeline: a committed operation over real
/// HTTP produces the reaction rows its event handlers own (outbox email, in-app notification,
/// session invalidation), and a failed operation produces none of them.
/// </summary>
[Collection("Database")]
public class DomainEventDispatchFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    private const string KnownPassword = "OldPassword123!";

    [Fact]
    public async Task ChangePassword_Committed_ProducesEmailNotificationAndSessionInvalidation()
    {
        var passwordService = Api.Services.GetRequiredService<IPasswordService>();
        string hashedPassword = passwordService.Hash(KnownPassword);
        var errors = TestErrorsFactory.CreateUserErrors();

        var userId = Guid.NewGuid();
        string email = $"dispatch-ok-{userId:N}@test.com";
        var actingSessionId = Guid.NewGuid();
        var otherSessionId = Guid.NewGuid();

        await SeedAsync<IdentityDbContext>(context =>
        {
            var user = UserFactory.CreateWithId(userId, email);
            user.MarkAsVerified();
            user.Activate();
            user.InitializePasswordHash(hashedPassword);

            context.Users.Add(user);
            context.Sessions.Add(SessionFactory.CreateWithId(actingSessionId, userId));
            context.Sessions.Add(SessionFactory.CreateWithId(otherSessionId, userId));
        });

        Client.AuthenticateAs(userId, "Visitor", actingSessionId);

        var request = new PublicChangePasswordRequestBuilder()
            .WithOldPassword(KnownPassword)
            .WithNewPassword(TestAuth.ChangedPassword)
            .Build();

        var response = await Client.PatchAsJsonAsync(Routes.Public.Auth.ChangePassword(), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        var outbox = await mailerContext.OutboxEmails.Where(o => o.RecipientAddress == email).ToListAsync();
        outbox.Should().ContainSingle(o => o.Template == "PasswordChanged");

        List<NotificationEntity> notifications = await mailerContext
            .Notifications.Where(n => n.UserId == userId)
            .ToListAsync();
        notifications.Should().ContainSingle(n => n.Type == EnumNotificationType.PasswordChanged);

        await using IdentityDbContext identityContext = CreateDbContext<IdentityDbContext>();
        SessionEntity acting = await identityContext.Sessions.SingleAsync(s => s.Id == actingSessionId);
        SessionEntity other = await identityContext.Sessions.SingleAsync(s => s.Id == otherSessionId);
        acting.IsRevoked.Should().BeFalse();
        other.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePassword_Failed_ProducesNoReactionRows()
    {
        var passwordService = Api.Services.GetRequiredService<IPasswordService>();
        string hashedPassword = passwordService.Hash(KnownPassword);
        var errors = TestErrorsFactory.CreateUserErrors();

        var userId = Guid.NewGuid();
        string email = $"dispatch-fail-{userId:N}@test.com";
        var actingSessionId = Guid.NewGuid();
        var otherSessionId = Guid.NewGuid();

        await SeedAsync<IdentityDbContext>(context =>
        {
            var user = UserFactory.CreateWithId(userId, email);
            user.MarkAsVerified();
            user.Activate();
            user.InitializePasswordHash(hashedPassword);

            context.Users.Add(user);
            context.Sessions.Add(SessionFactory.CreateWithId(actingSessionId, userId));
            context.Sessions.Add(SessionFactory.CreateWithId(otherSessionId, userId));
        });

        Client.AuthenticateAs(userId, "Visitor", actingSessionId);

        var request = new PublicChangePasswordRequestBuilder()
            .WithOldPassword("WrongPassword123!")
            .WithNewPassword(TestAuth.ChangedPassword)
            .Build();

        var response = await Client.PatchAsJsonAsync(Routes.Public.Auth.ChangePassword(), request);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ValidationErrorMessage>(m => m.IncorrectCurrentPassword())
        );

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        (
            await mailerContext.OutboxEmails.CountAsync(o =>
                o.RecipientAddress == email && o.Template == "PasswordChanged"
            )
        )
            .Should()
            .Be(0);
        (await mailerContext.Notifications.CountAsync(n => n.UserId == userId)).Should().Be(0);

        await using IdentityDbContext identityContext = CreateDbContext<IdentityDbContext>();
        (await identityContext.Sessions.CountAsync(s => s.UserId == userId && s.IsRevoked)).Should().Be(0);
    }
}
