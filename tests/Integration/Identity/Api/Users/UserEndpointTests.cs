using System.Text.Json;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Identity.Api.Users;

/// <summary>
/// Integration tests for the user management endpoints covering both admin
/// (<c>/api/v1/admin/me/profile</c>, <c>/api/v1/admin/me/avatar</c>,
/// <c>/api/v1/admin/users/{id}/roles</c>) and public
/// (<c>/api/v1/public/me/profile</c>, <c>/api/v1/public/me/avatar</c>) scopes,
/// verified against a real PostgreSQL database through the full API pipeline.
/// </summary>
[Collection("Database")]
public class UserEndpointTests(PostgresFixture db) : BaseApiTest(db)
{
    private const string AdminMeProfile = $"{ApiRoutes.Admin.Base}/me/profile";
    private const string AdminMeAvatar = $"{ApiRoutes.Admin.Base}/me/avatar";
    private const string PublicMeProfile = $"{ApiRoutes.Public.Me}/profile";
    private const string PublicMeAvatar = $"{ApiRoutes.Public.Me}/avatar";

    [Fact]
    public async Task AdminGetOwnProfile_AsSuperAdmin_Returns200WithUser()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync(AdminMeProfile);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("user", out var userProp).Should().BeTrue();
        userProp.GetProperty("id").GetString().Should().Be(User.SuperAdminId.ToString());
    }

    [Fact]
    public async Task AdminGetOwnProfile_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(AdminMeProfile);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminGetOwnProfile_AsVisitor_Returns403()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(AdminMeProfile);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminUpdateOwnProfile_AsSuperAdmin_WithoutValidSession_ReturnsForbidden()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { UserName = "newname123" };

        var response = await Client.PatchAsJsonAsync(AdminMeProfile, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AdminUpdateOwnProfile_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();
        var request = new { UserName = "noauth123" };

        var response = await Client.PatchAsJsonAsync(AdminMeProfile, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminUpdateAvatar_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "avatar.jpg");

        var response = await Client.PatchAsync(AdminMeAvatar, content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminAssignRole_AsSuperAdmin_WithValidData_Returns200()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { RoleId = role.Id };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Users}/{User.AdminId}/roles", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminAssignRole_AsAdmin_Returns403()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        Client.AuthenticateAsAdmin();
        var request = new { RoleId = role.Id };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Users}/{User.AdminId}/roles", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminAssignRole_WithNonExistentRole_Returns404()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { RoleId = Guid.NewGuid() };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Users}/{User.AdminId}/roles", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AdminRemoveRole_AsSuperAdmin_Returns200()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var userRole = UserRoleFactory.Create(User.AdminId, role.Id);
        context.UserRoles.Add(userRole);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Users}/{User.AdminId}/roles/{role.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminRemoveRole_AsAdmin_Returns403()
    {
        await using var context = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var userRole = UserRoleFactory.Create(User.AdminId, role.Id);
        context.UserRoles.Add(userRole);
        await context.SaveChangesAsync();

        Client.AuthenticateAsAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Users}/{User.AdminId}/roles/{role.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminGetUserRoles_AsSuperAdmin_Returns200()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Users}/{User.AdminId}/roles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("roles", out _).Should().BeTrue();
    }

    [Fact]
    public async Task AdminGetUserRoles_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Users}/{User.AdminId}/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminGetUserRoles_AsVisitor_Returns403()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Users}/{User.AdminId}/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublicGetOwnProfile_AsVisitor_Returns200()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(PublicMeProfile);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("user", out var userProp).Should().BeTrue();
        userProp.GetProperty("id").GetString().Should().Be(User.VisitorId.ToString());
    }

    [Fact]
    public async Task PublicGetOwnProfile_WithNoAuth_Returns401()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(PublicMeProfile);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PublicGetOwnProfile_AsAdmin_Returns403()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync(PublicMeProfile);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublicUpdateOwnProfile_AsVisitor_WithoutValidSession_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var request = new { UserName = "updated123" };

        var response = await Client.PatchAsJsonAsync(PublicMeProfile, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }
}
