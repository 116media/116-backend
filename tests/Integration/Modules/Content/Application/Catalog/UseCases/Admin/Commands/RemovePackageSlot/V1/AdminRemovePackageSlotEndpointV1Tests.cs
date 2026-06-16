using _116.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot.V1;

/// <summary>
/// Integration tests for the AdminRemovePackageSlot endpoint.
/// </summary>
[Collection("Database")]
public class AdminRemovePackageSlotEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RemovePackageSlot_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(Routes.Admin.Packages.Slot(Guid.NewGuid(), Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemovePackageSlot_AsAdmin_ReturnsForbidden()
    {
        PackageEntity package = null!;
        PackageSlotEntity slot = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            package = PackageFactory.Create();
            ctx.Packages.Add(package);
            slot = PackageSlotFactory.Create(package.Id);
            ctx.PackageSlots.Add(slot);
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.DeleteAsync(Routes.Admin.Packages.Slot(package.Id, slot.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemovePackageSlot_AsSuperAdmin_NonExistentPackage_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(Routes.Admin.Packages.Slot(Guid.NewGuid(), Guid.NewGuid()));

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that removing a package slot that does not exist on a valid package
    /// returns a 404 Not Found response.
    /// </summary>
    [Fact]
    public async Task RemovePackageSlot_NonExistentSlot_ReturnsNotFound()
    {
        PackageEntity package = await SeedAsync<ContentDbContext, PackageEntity>(ctx =>
        {
            PackageEntity pkg = PackageFactory.Create();
            ctx.Packages.Add(pkg);
            return pkg;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(Routes.Admin.Packages.Slot(package.Id, Guid.NewGuid()));

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemovePackageSlot_AsSuperAdmin_WithExistingSlot_ReturnsOk()
    {
        PackageEntity package = null!;
        PackageSlotEntity slot = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            package = PackageFactory.Create();
            ctx.Packages.Add(package);
            slot = PackageSlotFactory.Create(package.Id, category.Id);
            ctx.PackageSlots.Add(slot);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(Routes.Admin.Packages.Slot(package.Id, slot.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminRemovePackageSlotResponse>();
        body.IsSuccess.Should().BeTrue();
        body.Package.Slots.Should().NotContain(s => s.Id == slot.Id);

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        (await verifyContext.PackageSlots.AnyAsync(s => s.Id == slot.Id)).Should().BeFalse();
    }
}
