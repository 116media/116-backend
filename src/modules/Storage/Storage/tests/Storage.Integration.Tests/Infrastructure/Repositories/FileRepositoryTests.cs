using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.Application.Shared.Repositories;
using _116.Storage.Domain.Entities;
using _116.Storage.Infrastructure.Persistence;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;

namespace _116.Storage.Integration.Tests.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IFileRepository" /> verifying file CRUD,
/// soft delete, and avatar operations against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class FileRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task GetByIdAsync_ExistingFile_ReturnsFile()
    {
        await using var context = CreateDbContext<StorageDbContext>();
        var file = FileFactory.Create();
        context.Files.Add(file);
        await context.SaveChangesAsync();

        var (repo, _) = CreateScopedRepository<IFileRepository, StorageDbContext>();

        var result = await repo.GetByIdAsync(file.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(file.Id);
        result.FileName.Should().Be(file.FileName);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentFile_ReturnsNull()
    {
        var (repo, _) = CreateScopedRepository<IFileRepository, StorageDbContext>();

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_DeletedFile_ReturnsNull()
    {
        await using var context = CreateDbContext<StorageDbContext>();
        var file = FileFactory.CreateDeleted();
        context.Files.Add(file);
        await context.SaveChangesAsync();

        var (repo, _) = CreateScopedRepository<IFileRepository, StorageDbContext>();

        var result = await repo.GetByIdAsync(file.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_NewFile_PersistsToDatabase()
    {
        var file = FileFactory.Create();
        var (repo, db) = CreateScopedRepository<IFileRepository, StorageDbContext>();

        await repo.AddAsync(file);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<StorageDbContext>();
        var persisted = await verifyContext.Files.FindAsync(file.Id);
        persisted.Should().NotBeNull();
        persisted!.OriginalFileName.Should().Be(file.OriginalFileName);
    }

    [Fact]
    public async Task SoftDeleteByIdAsync_ExistingFile_MarksAsDeleted()
    {
        await using var context = CreateDbContext<StorageDbContext>();
        var file = FileFactory.Create();
        context.Files.Add(file);
        await context.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IFileRepository, StorageDbContext>();

        var result = await repo.SoftDeleteByIdAsync(file.Id);
        await db.SaveChangesAsync();

        result.Should().BeTrue();

        await using var verifyContext = CreateDbContext<StorageDbContext>();
        FileEntity? deleted = await verifyContext.Files.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == file.Id);
        deleted.Should().NotBeNull();
        deleted!.IsDeleted.Should().BeTrue();
        deleted.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task SoftDeleteByIdAsync_NonExistentFile_ReturnsFalse()
    {
        var (repo, _) = CreateScopedRepository<IFileRepository, StorageDbContext>();

        var result = await repo.SoftDeleteByIdAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDeleteByIdAsync_AlreadyDeletedFile_ReturnsFalse()
    {
        await using var context = CreateDbContext<StorageDbContext>();
        var file = FileFactory.CreateDeleted();
        context.Files.Add(file);
        await context.SaveChangesAsync();

        var (repo, _) = CreateScopedRepository<IFileRepository, StorageDbContext>();

        var result = await repo.SoftDeleteByIdAsync(file.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAvatarFileAsync_WithValidId_ReturnsFile()
    {
        await using var context = CreateDbContext<StorageDbContext>();
        var file = FileFactory.Create();
        context.Files.Add(file);
        await context.SaveChangesAsync();

        var (repo, _) = CreateScopedRepository<IFileRepository, StorageDbContext>();

        var result = await repo.GetAvatarFileAsync(file.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(file.Id);
    }

    [Fact]
    public async Task GetAvatarFileAsync_WithNullId_ReturnsNull()
    {
        var (repo, _) = CreateScopedRepository<IFileRepository, StorageDbContext>();

        var result = await repo.GetAvatarFileAsync(null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAvatarFileAsync_WithNonExistentId_ReturnsNull()
    {
        var (repo, _) = CreateScopedRepository<IFileRepository, StorageDbContext>();

        var result = await repo.GetAvatarFileAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task Remove_ExistingFile_DeletesFromDatabase()
    {
        await using var context = CreateDbContext<StorageDbContext>();
        var file = FileFactory.Create();
        context.Files.Add(file);
        await context.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IFileRepository, StorageDbContext>();
        var toRemove = await db.Files.FindAsync(file.Id);
        repo.Remove(toRemove!);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<StorageDbContext>();
        var removed = await verifyContext.Files.FindAsync(file.Id);
        removed.Should().BeNull();
    }

    [Fact]
    public async Task SoftDeleteByIdAsync_PersistsTheSoftDeleteAcrossContexts()
    {
        await using var context = CreateDbContext<StorageDbContext>();
        var file = FileFactory.Create();
        context.Files.Add(file);
        await context.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IFileRepository, StorageDbContext>();
        bool deleted = await repo.SoftDeleteByIdAsync(file.Id);
        await db.SaveChangesAsync();

        deleted.Should().BeTrue();

        await using var verifyContext = CreateDbContext<StorageDbContext>();
        FileEntity? updated = await verifyContext.Files.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == file.Id);
        updated!.IsDeleted.Should().BeTrue();
    }
}
