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
/// Unit tests for <see cref="ArtistRepository"/>.
/// </summary>
public class ArtistRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly ArtistRepository _repository;

    public ArtistRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new ArtistRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetBySlugAsync Tests

    [Fact(
        Skip = "ArtistBySlugSpecification uses EF.Functions.ILike which is not supported by InMemoryDatabase — tested in integration tests"
    )]
    public async Task GetBySlugAsync_WhenArtistExists_ShouldReturnArtist()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.CreateWithSlug("fally-ipupa");
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        // Act
        ArtistEntity? result = await _repository.GetBySlugAsync("fally-ipupa");

        // Assert
        result.Should().NotBeNull();
        result.Slug.Should().Be("fally-ipupa");
    }

    [Fact]
    public async Task GetBySlugAsync_WhenNoArtistsExist_ShouldReturnNull()
    {
        // Act
        ArtistEntity? result = await _repository.GetBySlugAsync("nonexistent-slug");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenArtistExists_ShouldReturnArtist()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        // Act
        ArtistEntity? result = await _repository.GetByIdAsync(artist.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(artist.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenArtistDoesNotExist_ShouldReturnNull()
    {
        // Act
        ArtistEntity? result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdOrThrowAsync Tests

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenArtistExists_ShouldReturnArtist()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        // Act
        ArtistEntity result = await _repository.GetByIdOrThrowAsync(artist.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(artist.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenArtistDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetByIdOrThrowAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetByUserIdAsync Tests

    [Fact]
    public async Task GetByUserIdAsync_WhenProfileIsClaimed_ShouldReturnArtist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        ArtistEntity claimed = ArtistFactory.CreateClaimed(userId);
        _context.Artists.AddRange(claimed, ArtistFactory.Create());
        await _context.SaveChangesAsync();

        // Act
        ArtistEntity? result = await _repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(claimed.Id);
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenNoProfileIsClaimedByUser_ShouldReturnNull()
    {
        // Arrange
        _context.Artists.Add(ArtistFactory.Create());
        await _context.SaveChangesAsync();

        // Act
        ArtistEntity? result = await _repository.GetByUserIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithNoSearch_ShouldReturnAllArtistsOrderedByName()
    {
        // Arrange
        _context.Artists.AddRange(
            ArtistFactory.Create("Zao", "zao"),
            ArtistFactory.Create("Awilo", "awilo"),
            ArtistFactory.Create("Koffi", "koffi")
        );
        await _context.SaveChangesAsync();

        // Act
        (List<ArtistEntity> artists, int totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null
        );

        // Assert
        totalCount.Should().Be(3);
        artists.Select(artist => artist.Name).Should().ContainInOrder("Awilo", "Koffi", "Zao");
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnRequestedPage()
    {
        // Arrange
        _context.Artists.AddRange(ArtistFactory.CreateMany(5));
        await _context.SaveChangesAsync();

        // Act
        (List<ArtistEntity> artists, int totalCount) = await _repository.GetAllAsync(
            page: 2,
            pageSize: 2,
            search: null
        );

        // Assert
        artists.Should().HaveCount(2);
        totalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoArtists_ShouldReturnEmptyList()
    {
        // Act
        (List<ArtistEntity> artists, int totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: null
        );

        // Assert
        artists.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    [Fact(
        Skip = "ArtistSearchSpecification uses EF.Functions.ILike which is not supported by InMemoryDatabase — tested in integration tests"
    )]
    public async Task GetAllAsync_WithSearch_ShouldFilterByNameOrBio()
    {
        // Arrange
        _context.Artists.AddRange(ArtistFactory.Create("Awilo", "awilo"), ArtistFactory.Create("Koffi", "koffi"));
        await _context.SaveChangesAsync();

        // Act
        (List<ArtistEntity> artists, int totalCount) = await _repository.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: "Awilo"
        );

        // Assert
        artists.Should().ContainSingle();
        totalCount.Should().Be(1);
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddArtistToContext()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();

        // Act
        await _repository.AddAsync(artist);

        // Assert
        _context.Entry(artist).State.Should().Be(EntityState.Added);

        await _context.SaveChangesAsync();
        ArtistEntity? saved = await _context.Artists.FirstOrDefaultAsync(a => a.Id == artist.Id);
        saved.Should().NotBeNull();
        saved.Name.Should().Be(artist.Name);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ShouldMarkArtistAsModified()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        // Act
        _repository.Update(artist);

        // Assert
        _context.Entry(artist).State.Should().Be(EntityState.Modified);
    }

    #endregion
}
