using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Outbox;
using _116.Tests.Fixtures.Factories.Core;

namespace _116.Integration.Tests.Shared.Infrastructure.Outbox;

/// <summary>
/// Integration tests proving the dispatch interceptor is wired into the real module contexts
/// and stamps every outbox row's occurrence time, now that the domain no longer reads a clock.
/// </summary>
[Collection("Database")]
public class OutboxOccurredOnStampTests(PostgresFixture db) : BaseRepositoryTest(db)
{
    /// <summary>
    /// Writes a file row through the real repository so a later delete raises a domain event.
    /// </summary>
    /// <returns>The persisted file id.</returns>
    private async Task<Guid> SeedFileAsync()
    {
        await using CoreDbContext context = CreateDbContext<CoreDbContext>();

        FileEntity file = FileFactory.CreateImage();
        context.Files.Add(file);
        await context.SaveChangesAsync();

        return file.Id;
    }

    /// <summary>
    /// Reads the outbox row captured for a file's soft deletion.
    /// </summary>
    /// <param name="fileId">The file whose deletion raised the event.</param>
    /// <returns>The outbox row, or null when none was written.</returns>
    private async Task<OutboxEventEntity?> ReadOutboxRowAsync(Guid fileId)
    {
        await using CoreDbContext context = CreateDbContext<CoreDbContext>();

        return await context
            .Set<OutboxEventEntity>()
            .Where(row => row.Payload.Contains(fileId.ToString()))
            .OrderByDescending(row => row.OccurredOn)
            .FirstOrDefaultAsync();
    }

    [Fact]
    public async Task SoftDeletingAFile_ShouldWriteAnOutboxRowCarryingAnOccurrenceStamp()
    {
        // Arrange
        Guid fileId = await SeedFileAsync();
        DateTime beforeDelete = DateTime.UtcNow.AddMinutes(-1);
        var (repository, context) = CreateScopedRepository<IFileRepository, CoreDbContext>();

        // Act
        await repository.SoftDeleteByIdAsync(fileId);
        await context.SaveChangesAsync();

        // Assert
        OutboxEventEntity? row = await ReadOutboxRowAsync(fileId);
        row.Should().NotBeNull();
        row!.OccurredOn.Should().NotBe(default);
        row.OccurredOn.Should().BeAfter(beforeDelete);
        row.OccurredOn.Should().BeBefore(DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public async Task SoftDeletingAFile_ShouldWriteAnOutboxRowTheReplayScanCanOrderBy()
    {
        // Arrange
        Guid firstFile = await SeedFileAsync();
        Guid secondFile = await SeedFileAsync();
        var (repository, context) = CreateScopedRepository<IFileRepository, CoreDbContext>();

        // Act
        await repository.SoftDeleteByIdAsync(firstFile);
        await context.SaveChangesAsync();

        await repository.SoftDeleteByIdAsync(secondFile);
        await context.SaveChangesAsync();

        // Assert
        OutboxEventEntity? firstRow = await ReadOutboxRowAsync(firstFile);
        OutboxEventEntity? secondRow = await ReadOutboxRowAsync(secondFile);

        firstRow.Should().NotBeNull();
        secondRow.Should().NotBeNull();
        secondRow!.OccurredOn.Should().BeOnOrAfter(firstRow!.OccurredOn);
    }
}
