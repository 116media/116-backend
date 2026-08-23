using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.DeactivatePackage.V1;

/// <summary>
/// Integration tests for the AdminDeactivatePackage endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeactivatePackageEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task DeactivatePackage_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(Routes.Admin.Packages.Deactivate(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeactivatePackage_AsVisitor_ReturnsForbidden()
    {
        PackageEntity package = await SeedPackageAsync(active: true);

        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync(Routes.Admin.Packages.Deactivate(package.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeactivatePackage_AsSuperAdmin_WithExistingActivePackage_ReturnsOk()
    {
        PackageEntity package = await SeedPackageAsync(active: true);

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Packages.Deactivate(package.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await IsPackageActiveAsync(package.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task DeactivatePackage_AsSuperAdmin_NonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Packages.Deactivate(Guid.NewGuid()), null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Package"))
        );
    }

    [Fact]
    public async Task DeactivatePackage_AsSuperAdmin_AlreadyInactive_ReturnsConflict()
    {
        PackageEntity package = await SeedPackageAsync(active: false);

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(Routes.Admin.Packages.Deactivate(package.Id), null);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<PackageErrorMessage>(m => m.AlreadyInactive())
        );
        (await IsPackageActiveAsync(package.Id)).Should().BeFalse();
    }
}
