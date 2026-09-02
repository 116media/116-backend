using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="AlbumRepository"/>.
/// </summary>
public class AlbumRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly AlbumRepository _repository;

    public AlbumRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new AlbumRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenAlbumExists_ShouldReturnAlbum()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();
        _context.Albums.Add(album);
        await _context.SaveChangesAsync();

        // Act
        AlbumEntity? result = await _repository.GetByIdAsync(album.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(album.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAlbumDoesNotExist_ShouldReturnNull()
    {
        // Act
        AlbumEntity? result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdOrThrowAsync Tests

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenAlbumExists_ShouldReturnAlbum()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();
        _context.Albums.Add(album);
        await _context.SaveChangesAsync();

        // Act
        AlbumEntity result = await _repository.GetByIdOrThrowAsync(album.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(album.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenAlbumDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetByIdOrThrowAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithNoSearch_ShouldReturnAllAlbumsOrderedByName()
    {
        // Arrange
        _context.Albums.AddRange(
            AlbumFactory.CreateWithName("Zenith"),
            AlbumFactory.CreateWithName("Anthem"),
            AlbumFactory.CreateWithName("Midnight")
        );
        await _context.SaveChangesAsync();

        // Act
        (List<AlbumEntity> albums, int totalCount) = await _repository.GetAllAsync(page: 1, pageSize: 10, search: null);

        // Assert
        totalCount.Should().Be(3);
        albums.Select(album => album.Name).Should().ContainInOrder("Anthem", "Midnight", "Zenith");
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnRequestedPage()
    {
        // Arrange
        _context.Albums.AddRange(AlbumFactory.CreateMany(5));
        await _context.SaveChangesAsync();

        // Act
        (List<AlbumEntity> albums, int totalCount) = await _repository.GetAllAsync(page: 2, pageSize: 2, search: null);

        // Assert
        albums.Should().HaveCount(2);
        totalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoAlbums_ShouldReturnEmptyList()
    {
        // Act
        (List<AlbumEntity> albums, int totalCount) = await _repository.GetAllAsync(page: 1, pageSize: 10, search: null);

        // Assert
        albums.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddAlbumToContext()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();

        // Act
        await _repository.AddAsync(album);

        // Assert
        _context.Entry(album).State.Should().Be(EntityState.Added);

        await _context.SaveChangesAsync();
        AlbumEntity? saved = await _context.Albums.FirstOrDefaultAsync(a => a.Id == album.Id);
        saved.Should().NotBeNull();
        saved.Name.Should().Be(album.Name);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ShouldMarkAlbumAsModified()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();
        _context.Albums.Add(album);
        await _context.SaveChangesAsync();

        // Act
        _repository.Update(album);

        // Assert
        _context.Entry(album).State.Should().Be(EntityState.Modified);
    }

    #endregion
}
