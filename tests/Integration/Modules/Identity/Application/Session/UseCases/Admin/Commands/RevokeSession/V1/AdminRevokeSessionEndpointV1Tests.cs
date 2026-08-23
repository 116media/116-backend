using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Session.UseCases.Admin.Commands.RevokeSession.V1;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Session.UseCases.Admin.Commands.RevokeSession.V1;

/// <summary>
/// Integration tests for the AdminRevokeSession endpoint.
/// </summary>
[Collection("Database")]
public class AdminRevokeSessionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string RevokeSessionBaseUrl =
        $"{ApiRoutes.Admin.Base}/{IdentityConstants.Me}/{SessionRouteConstants.Endpoint}/{SessionRouteConstants.Revoke}";

    [Fact]
    public async Task AdminRevokeSession_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"{RevokeSessionBaseUrl}/{Guid.NewGuid()}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminRevokeSession_WithNonExistentId_Returns404()
    {
        Client.AuthenticateAsSuperAdmin();

        Guid nonExistentId = Guid.NewGuid();
        var response = await Client.PostAsync($"{RevokeSessionBaseUrl}/{nonExistentId}", null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Session"))
        );
    }

    [Fact]
    public async Task AdminRevokeSession_AsSuperAdmin_WithExistingSession_ReturnsOk()
    {
        SessionEntity session = await SeedAsync<IdentityDbContext, SessionEntity>(ctx =>
        {
            SessionEntity entity = SessionFactory.Create(TestUser.SuperAdminId);
            ctx.Sessions.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsync($"{RevokeSessionBaseUrl}/{session.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminRevokeSessionResponse body = await response.ReadAsAsync<AdminRevokeSessionResponse>();
        body.IsSuccess.Should().BeTrue();

        await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
        SessionEntity? persisted = await context.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        persisted.Should().NotBeNull();
        persisted!.IsRevoked.Should().BeTrue();
    }
}
