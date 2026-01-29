using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Unit.Tests.Common.Builders.Entities;
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
        var options = new DbContextOptionsBuilder<CoreDbContext>()
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
        var file1 = new FileBuilder().Build();
        var file2 = new FileBuilder().Build();

        _context.Files.Add(file1);
        _context.Files.Add(file2);

        // Act
        var result = await _unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(2);

        var savedFiles = await _context.Files.ToListAsync();
        savedFiles.Should().HaveCount(2);
    }

    [Fact]
    public async Task CommitAsync_WhenNoChanges_ShouldReturnZero()
    {
        // Act
        var result = await _unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var file = new FileBuilder().Build();
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
        var file = new FileBuilder().Build();
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        var newUrl = "https://new-storage-url.com/file.jpg";
        file.UpdateStorageUrl(newUrl);

        // Act
        var result = await _unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(1);

        var updatedFile = await _context.Files.FirstOrDefaultAsync(f => f.Id == file.Id);
        updatedFile.Should().NotBeNull();
        updatedFile!.StorageUrl.Should().Be(newUrl);
    }

    [Fact]
    public async Task CommitAsync_WithDeletes_ShouldSaveDeletesAndReturnCount()
    {
        // Arrange
        var file = new FileBuilder().Build();
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        _context.Files.Remove(file);

        // Act
        var result = await _unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(1);

        var deletedFile = await _context.Files.FirstOrDefaultAsync(f => f.Id == file.Id);
        deletedFile.Should().BeNull();
    }

    [Fact]
    public async Task CommitAsync_WithMultipleOperations_ShouldSaveAllChanges()
    {
        // Arrange
        var existingFile = new FileBuilder().Build();
        _context.Files.Add(existingFile);
        await _context.SaveChangesAsync();

        var newFile = new FileBuilder().Build();
        _context.Files.Add(newFile);

        existingFile.UpdateStorageUrl("https://updated-url.com/file.jpg");

        // Act
        var result = await _unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(2); // 1 insert + 1 update

        var files = await _context.Files.ToListAsync();
        files.Should().HaveCount(2);
    }
}
