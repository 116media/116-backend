using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IPackageRepository" /> verifying package CRUD,
/// slot management, pagination, and filtering against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class PackageRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task GetAllAsync_WithPackages_ReturnsPaginatedResults()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        context.Packages.AddRange(PackageFactory.CreateMany(3));
        await context.SaveChangesAsync();

        var repo = Resolve<IPackageRepository>();

        var (packages, totalCount) = await repo.GetAllAsync(1, 10, null);

        totalCount.Should().Be(3);
        packages.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_FilterByActive_ReturnsOnlyActivePackages()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        context.Packages.Add(PackageFactory.Create());
        context.Packages.Add(PackageFactory.CreateInactive());
        await context.SaveChangesAsync();

        var repo = Resolve<IPackageRepository>();

        var (packages, _) = await repo.GetAllAsync(1, 100, isActive: true);

        packages.Should().AllSatisfy(p => p.IsActive.Should().BeTrue());
    }

    [Fact]
    public async Task GetAllAsync_FilterByInactive_ReturnsOnlyInactivePackages()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        context.Packages.Add(PackageFactory.CreateInactive());
        await context.SaveChangesAsync();

        var repo = Resolve<IPackageRepository>();

        var (packages, _) = await repo.GetAllAsync(1, 100, isActive: false);

        packages.Should().AllSatisfy(p => p.IsActive.Should().BeFalse());
    }

    [Fact]
    public async Task GetByIdWithSlotsAsync_ExistingPackage_ReturnsPackage()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var package = PackageFactory.Create();
        context.Packages.Add(package);
        await context.SaveChangesAsync();

        var repo = Resolve<IPackageRepository>();

        var result = await repo.GetByIdAsync(package.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(package.Id);
    }

    [Fact]
    public async Task GetByIdWithSlotsAsync_NonExistentPackage_ReturnsNull()
    {
        var repo = Resolve<IPackageRepository>();

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithSlotsOrThrowAsync_NonExistentPackage_ThrowsNotFoundException()
    {
        var repo = Resolve<IPackageRepository>();
        var id = Guid.NewGuid();

        var act = async () => await repo.GetByIdOrThrowAsync(id);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task AddAsync_NewPackage_PersistsToDatabase()
    {
        var package = PackageFactory.Create();
        var (repo, db) = CreateScopedRepository<IPackageRepository, ContentDbContext>();

        await repo.AddAsync(package);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.Packages.FindAsync(package.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task FindSlot_ScopedToAnotherPackage_ReturnsNull()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var owningPackage = PackageFactory.Create();
        var otherPackage = PackageFactory.Create();
        var slot = PackageSlotFactory.CreateOpen(owningPackage);
        context.Packages.AddRange(owningPackage, otherPackage);
        await context.SaveChangesAsync();

        var repo = Resolve<IPackageRepository>();

        PackageEntity owner = await repo.GetByIdOrThrowAsync(owningPackage.Id);
        PackageEntity other = await repo.GetByIdOrThrowAsync(otherPackage.Id);

        owner.FindSlot(slot.Id).Should().NotBeNull();
        other.FindSlot(slot.Id).Should().BeNull();
    }

    [Fact]
    public async Task AddSlot_ThroughTheRoot_PersistsToDatabase()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        var package = PackageFactory.Create();
        context.Packages.Add(package);
        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IPackageRepository, ContentDbContext>();
        PackageEntity tracked = await repo.GetByIdOrThrowAsync(package.Id);

        PackageSlotEntity slot = tracked.AddSlot(categoryId: category.Id, isRequired: true, quantity: 1);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.PackageSlots.FindAsync(slot.Id);
        persisted.Should().NotBeNull();
    }
}
