using _116.Identity.Application.User.UseCases.Admin.Commands.DeactivateUser.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.DeactivateUser.V1;

/// <summary>
/// Integration tests for the AdminDeactivateUser endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeactivateUserEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string DeactivateUrl(Guid userId) => $"{ApiRoutes.Admin.Users}/{userId}/deactivate";

    [Fact]
    public async Task AdminDeactivateUser_AsSuperAdmin_DeactivatesAndRevokesEverySession()
    {
        // Arrange — an active user holding two live sessions
        UserEntity user = await SeedAsync<IdentityDbContext, UserEntity>(context =>
        {
            UserEntity created = UserFactory.CreateVerifiedActive();
            context.Users.Add(created);
            context.Sessions.Add(SessionFactory.Create(created.Id));
            context.Sessions.Add(SessionFactory.Create(created.Id));
            return created;
        });

        Client.AuthenticateAsSuperAdmin();

        // Act
        var response = await Client.PatchAsync(DeactivateUrl(user.Id), content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AdminDeactivateUserResponse body = await response.ReadAsAsync<AdminDeactivateUserResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        (await verifyContext.Users.FirstAsync(u => u.Id == user.Id)).IsActive.Should().BeFalse();

        // The deactivation event revoked every live session of the account
        (await verifyContext.Sessions.Where(s => s.UserId == user.Id).ToListAsync())
            .Should()
            .OnlyContain(s => s.IsRevoked);

        // The deactivation event bumped the token version, so outstanding JWTs die on refresh
        (await verifyContext.UserTokenStates.SingleAsync(s => s.Id == user.Id))
            .TokenVersion.Should()
            .Be(1);
    }

    [Fact]
    public async Task AdminDeactivateUser_CalledTwice_StaysIdempotent()
    {
        // Arrange
        UserEntity user = await SeedAsync<IdentityDbContext, UserEntity>(context =>
        {
            UserEntity created = UserFactory.CreateVerifiedActive();
            context.Users.Add(created);
            return created;
        });

        Client.AuthenticateAsSuperAdmin();
        (await Client.PatchAsync(DeactivateUrl(user.Id), content: null)).StatusCode.Should().Be(HttpStatusCode.OK);

        // Act — a second deactivation succeeds without transitioning anything
        var response = await Client.PatchAsync(DeactivateUrl(user.Id), content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        (await verifyContext.Users.FirstAsync(u => u.Id == user.Id)).IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task AdminDeactivateUser_WithUnknownUser_Returns404()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(DeactivateUrl(Guid.NewGuid()), content: null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("User"))
        );
    }

    [Fact]
    public async Task AdminDeactivateUser_AsAdmin_Returns403()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync(DeactivateUrl(Guid.NewGuid()), content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminDeactivateUser_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(DeactivateUrl(Guid.NewGuid()), content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
