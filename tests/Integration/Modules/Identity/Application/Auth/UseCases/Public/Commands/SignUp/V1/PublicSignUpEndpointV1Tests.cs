using System.Text.Json;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SignUp.V1;

/// <summary>
/// Integration tests for the PublicSignUp endpoint.
/// </summary>
[Collection("Database")]
public class PublicSignUpEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SignUp_WithValidData_ReturnsCreated()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        context.Roles.Add(visitorRole);
        await context.SaveChangesAsync();

        Client.ClearAuthentication();
        Client.DefaultRequestHeaders.Add("X-Device-Id", Guid.NewGuid().ToString());
        var email = $"s{Guid.NewGuid():N}@test.com";
        var userName = $"u{Guid.NewGuid():N}"[..10];
        var request = new
        {
            Email = email,
            UserName = userName,
            Password = "Test123!abc",
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/signup", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task SignUp_WithDuplicateEmail_ReturnsConflict()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = TestUser.SuperAdminEmail,
            UserName = $"u{Guid.NewGuid():N}"[..10],
            Password = "Test123!abc",
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/signup", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task SignUp_WithEmptyEmail_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = "",
            UserName = "validuser",
            Password = "Test123!abc",
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/signup", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SignUp_WithWeakPassword_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = $"s{Guid.NewGuid():N}@test.com",
            UserName = $"u{Guid.NewGuid():N}"[..10],
            Password = "abc",
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/signup", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SignUp_WithShortUsername_ReturnsValidationError()
    {
        Client.ClearAuthentication();
        var request = new
        {
            Email = $"s{Guid.NewGuid():N}@test.com",
            UserName = "ab",
            Password = "Test123!abc",
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Auth}/signup", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
