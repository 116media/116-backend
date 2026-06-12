using System.Net.Http.Json;
using System.Text.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage.V1;

/// <summary>
/// Integration tests for the AdminCreatePackage endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreatePackageEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreatePackage_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Name = "Unauthorized Pkg", Description = "Should not be created" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Packages, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePackage_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var request = new { Name = "Forbidden Pkg", Description = "Admin cannot create" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Packages, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePackage_AsSuperAdmin_WithEmptyName_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "", Description = "Missing name" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Packages, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreatePackage_AsSuperAdmin_WithEmptyDescription_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "Valid Name", Description = "" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Packages, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreatePackage_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { Name = "Premium Bundle", Description = "A premium content package" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Packages, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var context = CreateDbContext<ContentDbContext>();
        var package = await context.Packages.FirstOrDefaultAsync(p => p.Name == "Premium Bundle");
        package.Should().NotBeNull();
        package!.Description.Should().Be("A premium content package");
    }
}
