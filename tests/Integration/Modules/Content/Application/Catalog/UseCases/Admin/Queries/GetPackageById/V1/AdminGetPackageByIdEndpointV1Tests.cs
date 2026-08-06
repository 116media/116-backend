using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById.V1;

/// <summary>
/// Integration tests for the AdminGetPackageById endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetPackageByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<PackageEntity> SeedPackageAsync()
    {
        return await SeedAsync<ContentDbContext, PackageEntity>(ctx =>
        {
            PackageEntity package = PackageFactory.Create();
            ctx.Packages.Add(package);
            return package;
        });
    }

    [Fact]
    public async Task GetPackageById_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPackageById_AsVisitor_ReturnsForbidden()
    {
        PackageEntity package = await SeedPackageAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}/{package.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPackageById_AsSuperAdmin_WithExistingPackage_ReturnsOk()
    {
        PackageEntity package = await SeedPackageAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}/{package.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetPackageByIdResponse>();
        body.Package.Id.Should().Be(package.Id);
        body.Package.Name.Should().Be(package.Name);
        body.Package.Description.Should().Be(package.Description);
    }

    [Fact]
    public async Task GetPackageById_AsSuperAdmin_NonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}/{Guid.NewGuid()}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Package"))
        );
    }

    [Fact]
    public async Task GetPackageById_AsAdmin_WithExistingPackage_ReturnsOk()
    {
        PackageEntity package = await SeedPackageAsync();

        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Packages}/{package.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetPackageByIdResponse>();
        body.Package.Id.Should().Be(package.Id);
    }
}
