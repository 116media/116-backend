using _116.Identity.Application.Session.UseCases.Admin.Commands.CleanupExpiredSessions.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Admin.Commands.CleanupExpiredSessions.V1;

/// <summary>
/// Integration tests for the AdminCleanupExpiredSessions endpoint.
/// </summary>
[Collection("Database")]
public class AdminCleanupExpiredSessionsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AdminCleanup_AsSuperAdmin_Returns200()
    {
        SessionEntity expiredSession = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
        {
            SessionEntity session = SessionFactory.CreateExpired(TestUser.SuperAdminId);
            ctx.Sessions.Add(session);
            return session;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsync(Routes.Admin.Sessions.Cleanup(), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminCleanupExpiredSessionsResponse body = await response.ReadAsAsync<AdminCleanupExpiredSessionsResponse>();
        body.DeletedCount.Should().Be(1);

        await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
        SessionEntity? persisted = await context.Sessions.FirstOrDefaultAsync(s => s.Id == expiredSession.Id);
        persisted.Should().NotBeNull();
        persisted!.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task AdminCleanup_AsAdmin_Returns403()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsync(Routes.Admin.Sessions.Cleanup(), null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
