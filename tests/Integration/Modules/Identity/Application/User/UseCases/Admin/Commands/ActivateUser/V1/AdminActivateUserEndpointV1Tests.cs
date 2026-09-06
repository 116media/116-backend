using _116.Identity.Application.User.UseCases.Admin.Commands.ActivateUser.V1;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.ActivateUser.V1;

/// <summary>
/// Integration tests for the AdminActivateUser endpoint.
/// </summary>
[Collection("Database")]
public class AdminActivateUserEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ActivateUrl(Guid userId) => $"{ApiRoutes.Admin.Users}/{userId}/activate";

    [Fact]
    public async Task AdminActivateUser_AsSuperAdmin_ReactivatesTheAccount()
    {
        // Arrange
        UserEntity user = await SeedAsync<IdentityDbContext, UserEntity>(context =>
        {
            UserEntity created = UserFactory.CreateInactive();
            context.Users.Add(created);
            return created;
        });

        Client.AuthenticateAsSuperAdmin();

        // Act
        var response = await Client.PatchAsync(ActivateUrl(user.Id), content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AdminActivateUserResponse body = await response.ReadAsAsync<AdminActivateUserResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        (await verifyContext.Users.FirstAsync(u => u.Id == user.Id)).IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task AdminActivateUser_CalledTwice_StaysIdempotent()
    {
        // Arrange
        UserEntity user = await SeedAsync<IdentityDbContext, UserEntity>(context =>
        {
            UserEntity created = UserFactory.CreateInactive();
            context.Users.Add(created);
            return created;
        });

        Client.AuthenticateAsSuperAdmin();
        (await Client.PatchAsync(ActivateUrl(user.Id), content: null)).StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var response = await Client.PatchAsync(ActivateUrl(user.Id), content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        (await verifyContext.Users.FirstAsync(u => u.Id == user.Id)).IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task AdminActivateUser_WithUnknownUser_Returns404()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(ActivateUrl(Guid.NewGuid()), content: null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("User"))
        );
    }

    [Fact]
    public async Task AdminActivateUser_AsAdmin_Returns403()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync(ActivateUrl(Guid.NewGuid()), content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminActivateUser_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(ActivateUrl(Guid.NewGuid()), content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
