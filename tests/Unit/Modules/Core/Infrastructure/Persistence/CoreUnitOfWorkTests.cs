using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Core;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
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
        GC.SuppressFinalize(this);
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

        file.Delete(DateTime.UtcNow);

        // Act
        int result = await _unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(1);

        FileEntity? updatedFile = await _context.Files.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == file.Id);
        updatedFile.Should().NotBeNull();
        updatedFile.IsDeleted.Should().BeTrue();
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

        existingFile.Delete(DateTime.UtcNow);

        // Act
        int result = await _unitOfWork.CommitAsync();

        // Assert
        result.Should().Be(2); // 1 insert + 1 update

        List<FileEntity> files = await _context.Files.IgnoreQueryFilters().ToListAsync();
        files.Should().HaveCount(2);
        files.Should().ContainSingle(f => f.IsDeleted);
    }

    #region ExecuteInTransactionAsync

    /// <summary>
    /// Builds options that let the in-memory provider accept the transaction the seam opens.
    /// The provider ignores it, which is enough to drive the surrounding orchestration.
    /// </summary>
    /// <returns>Options with the transaction warning suppressed.</returns>
    private static DbContextOptions<CoreDbContext> CreateTransactionalOptions() =>
        new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldRunTheOperation()
    {
        // Arrange
        await using var context = new CoreDbContext(CreateTransactionalOptions());
        var unitOfWork = new CoreUnitOfWork(context);
        var ran = false;

        // Act
        await unitOfWork.ExecuteInTransactionAsync(_ =>
        {
            ran = true;
            return Task.CompletedTask;
        });

        // Assert
        ran.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldSaveWhatTheOperationTracked()
    {
        // Arrange
        // The seam commits once at the end, so the operation itself never saves.
        await using var context = new CoreDbContext(CreateTransactionalOptions());
        var unitOfWork = new CoreUnitOfWork(context);

        // Act
        await unitOfWork.ExecuteInTransactionAsync(_ =>
        {
            context.Files.Add(FileFactory.CreateImage());
            return Task.CompletedTask;
        });

        // Assert
        context.Files.Local.Should().ContainSingle();
        context.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WhenTheOperationThrows_ShouldSurfaceTheFailure()
    {
        // Arrange
        // A failure has to reach the caller; swallowing it would commit a half-applied change.
        await using var context = new CoreDbContext(CreateTransactionalOptions());
        var unitOfWork = new CoreUnitOfWork(context);
        var failure = new InvalidOperationException("operation failed");

        // Act
        Func<Task> act = async () => await unitOfWork.ExecuteInTransactionAsync(_ => throw failure);

        // Assert
        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Should()
            .BeSameAs(failure);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldPassTheTokenToTheOperation()
    {
        // Arrange
        await using var context = new CoreDbContext(CreateTransactionalOptions());
        var unitOfWork = new CoreUnitOfWork(context);
        using var cts = new CancellationTokenSource();
        CancellationToken observed = default;

        // Act
        await unitOfWork.ExecuteInTransactionAsync(
            token =>
            {
                observed = token;
                return Task.CompletedTask;
            },
            cts.Token
        );

        // Assert
        observed.Should().Be(cts.Token);
    }

    #endregion
}
