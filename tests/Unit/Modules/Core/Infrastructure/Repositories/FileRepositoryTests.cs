using _116.Core.Application.Shared.Errors.Facade;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Core.Infrastructure.Repositories;
using _116.Tests.Fixtures.Factories.Core;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="FileRepository"/>.
/// </summary>
public class FileRepositoryTests : IDisposable
{
    private static readonly DateTime StartInstant = new(2026, 9, 11, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeTimeProvider _time = new(new DateTimeOffset(StartInstant));
    private readonly CoreDbContext _context;
    private readonly FileRepository _repository;

    public FileRepositoryTests()
    {
        DbContextOptions<CoreDbContext> options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CoreDbContext(options);
        _repository = new FileRepository(_context, _time);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenFileExists_ShouldReturnFile()
    {
        // Arrange
        FileEntity file = FileFactory.Create();
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        // Act
        FileEntity? result = await _repository.GetByIdAsync(file.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(file.Id);
        result.FileName.Should().Be(file.FileName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFileDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        // Act
        FileEntity? result = await _repository.GetByIdAsync(fileId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenFileIsDeleted_ShouldReturnNull()
    {
        // Arrange
        FileEntity file = FileFactory.CreateDeleted();
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        // Act
        FileEntity? result = await _repository.GetByIdAsync(file.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddFileToContext()
    {
        // Arrange
        FileEntity file = FileFactory.Create();

        // Act
        await _repository.AddAsync(file);
        await _context.SaveChangesAsync();

        // Assert
        FileEntity? savedFile = await _context.Files.FirstOrDefaultAsync(f => f.Id == file.Id);
        savedFile.Should().NotBeNull();
        savedFile.Id.Should().Be(file.Id);
    }

    #endregion

    #region UpdateAsync Tests


    #endregion

    #region Remove Tests

    [Fact]
    public void Remove_ShouldRemoveFileFromContext()
    {
        // Arrange
        FileEntity file = FileFactory.Create();
        _context.Files.Add(file);
        _context.SaveChanges();

        // Act
        _repository.Remove(file);
        _context.SaveChanges();

        // Assert
        FileEntity? removedFile = _context.Files.FirstOrDefault(f => f.Id == file.Id);
        removedFile.Should().BeNull();
    }

    #endregion

    #region GetAvatarFileAsync Tests

    [Fact]
    public async Task GetAvatarFileAsync_WhenAvatarFileIdHasValueAndExists_ShouldReturnFile()
    {
        // Arrange
        FileEntity file = FileFactory.CreatePng();
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        // Act
        FileEntity? result = await _repository.GetAvatarFileAsync(file.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(file.Id);
    }

    [Fact]
    public async Task GetAvatarFileAsync_WhenAvatarFileIdIsNull_ShouldReturnNull()
    {
        // Act
        FileEntity? result = await _repository.GetAvatarFileAsync(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAvatarFileAsync_WhenAvatarFileIdHasValueButDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        // Act
        FileEntity? result = await _repository.GetAvatarFileAsync(fileId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SaveChangesAsync Tests


    #endregion
}
