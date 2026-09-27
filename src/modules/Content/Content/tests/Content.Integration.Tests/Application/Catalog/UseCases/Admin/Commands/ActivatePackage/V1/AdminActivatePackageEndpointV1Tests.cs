using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Messages;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;

namespace _116.Content.Integration.Tests.Application.Catalog.UseCases.Admin.Commands.ActivatePackage.V1;

/// <summary>
/// Integration tests for the AdminActivatePackage endpoint.
/// </summary>
[Collection("Database")]
public class AdminActivatePackageEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<PackageEntity> SeedPackageAsync(bool active)
    {
        return await SeedAsync<ContentDbContext, PackageEntity>(ctx =>
        {
            PackageEntity package = active ? PackageFactory.Create() : PackageFactory.CreateInactive();
            ctx.Packages.Add(package);
            return package;
        });
    }

    private async Task<bool> IsPackageActiveAsync(Guid id)
    {
        await using var ctx = CreateDbContext<ContentDbContext>();
        PackageEntity? package = await ctx.Packages.FindAsync(id);
        return package!.IsActive;
    }

    [Fact]
    public async Task ActivatePackage_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(Routes.Admin.Packages.Activate(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ActivatePackage_AsVisitor_ReturnsForbidden()
    {
        PackageEntity package = await SeedPackageAsync(active: false);

        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync(Routes.Admin.Packages.Activate(package.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ActivatePackage_AsSuperAdmin_WithExistingInactivePackage_ReturnsOk()
    {
        PackageEntity package = await SeedPackageAsync(active: false);

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Packages.Activate(package.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await IsPackageActiveAsync(package.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task ActivatePackage_AsSuperAdmin_NonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Packages.Activate(Guid.NewGuid()), null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Package"))
        );
    }

    [Fact]
    public async Task ActivatePackage_AsSuperAdmin_AlreadyActive_ReturnsConflict()
    {
        PackageEntity package = await SeedPackageAsync(active: true);

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Packages.Activate(package.Id), null);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<PackageErrorMessage>(m => m.AlreadyActive())
        );
        (await IsPackageActiveAsync(package.Id)).Should().BeTrue();
    }
}
