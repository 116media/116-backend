using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Infrastructure.Persistence;

/// <summary>
/// Unit tests for <see cref="CoreUnitOfWork"/>.
/// </summary>
public class CoreUnitOfWorkTests : IDisposable
{
    private readonly CoreDbContext _context;
    private readonly CoreUnitOfWork _unitOfWork;

    public CoreUnitOfWorkTests()
    {
        DbContextOptions<CoreDbContext> options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CoreDbContext(options);
        _unitOfWork = new CoreUnitOfWork(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CommitAsync_WhenChangesExist_ShouldSaveChangesAndReturnCount()
    {
        // Arrange
        FileEntity file1 = FileFactory.Create();
        FileEntity file2 = FileFactory.Create();

        _context.Files.Add(file1);
        _context.Files.Add(file2);

        // Act
        int result = await _unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(2);

        List<FileEntity> savedFiles = await _context.Files.ToListAsync();
        savedFiles.Should().HaveCount(2);
    }

    [Fact]
    public async Task CommitAsync_WhenNoChanges_ShouldReturnZero()
    {
        // Act
        int result = await _unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        FileEntity file = FileFactory.Create();
        _context.Files.Add(file);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await _unitOfWork.CommitAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task CommitAsync_WithUpdates_ShouldSaveUpdatesAndReturnCount()
    {
        // Arrange
        FileEntity file = FileFactory.Create();
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        string newUrl = "https://new-storage-url.com/file.jpg";
        file.UpdateStorageUrl(newUrl);

        // Act
        int result = await _unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(1);

        FileEntity? updatedFile = await _context.Files.FirstOrDefaultAsync(f => f.Id == file.Id);
        updatedFile.Should().NotBeNull();
        updatedFile!.StorageUrl.Should().Be(newUrl);
    }

    [Fact]
    public async Task CommitAsync_WithDeletes_ShouldSaveDeletesAndReturnCount()
    {
        // Arrange
        FileEntity file = FileFactory.Create();
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        _context.Files.Remove(file);

        // Act
        int result = await _unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(1);

        FileEntity? deletedFile = await _context.Files.FirstOrDefaultAsync(f => f.Id == file.Id);
        deletedFile.Should().BeNull();
    }

    [Fact]
    public async Task CommitAsync_WithMultipleOperations_ShouldSaveAllChanges()
    {
        // Arrange
        FileEntity existingFile = FileFactory.Create();
        _context.Files.Add(existingFile);
        await _context.SaveChangesAsync();

        FileEntity newFile = FileFactory.Create();
        _context.Files.Add(newFile);

        existingFile.UpdateStorageUrl("https://updated-url.com/file.jpg");

        // Act
        int result = await _unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(2); // 1 insert + 1 update

        List<FileEntity> files = await _context.Files.ToListAsync();
        files.Should().HaveCount(2);
    }
}
