using _116.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
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
    public async Task RemovePackageSlot_AsSuperAdmin_WithUnknownPackageId_ReturnsPackageNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(Routes.Admin.Packages.Slot(Guid.NewGuid(), Guid.NewGuid()));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Package"))
        );
    }

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

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("PackageSlot"))
        );
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

    [Fact]
    public async Task RemovePackageSlot_WithSlotBelongingToAnotherPackage_ReturnsNotFound()
    {
        PackageEntity owningPackage = null!;
        PackageEntity otherPackage = null!;
        PackageSlotEntity slot = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            owningPackage = PackageFactory.Create();
            ctx.Packages.Add(owningPackage);
            otherPackage = PackageFactory.Create();
            ctx.Packages.Add(otherPackage);
            slot = PackageSlotFactory.Create(owningPackage.Id, category.Id);
            ctx.PackageSlots.Add(slot);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync(Routes.Admin.Packages.Slot(otherPackage.Id, slot.Id));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("PackageSlot"))
        );

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        (await verifyContext.PackageSlots.AnyAsync(s => s.Id == slot.Id))
            .Should()
            .BeTrue("a slot belonging to another package must not be removed");
    }
}
