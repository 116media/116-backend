using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Content.Unit.Tests.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="ArtistClaimRequestRepository" />.
/// </summary>
public class ArtistClaimRequestRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly ArtistClaimRequestRepository _repository;

    public ArtistClaimRequestRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(new CreatedAtStampingInterceptor())
            .Options;
        _context = new ContentDbContext(options);
        _repository = new ArtistClaimRequestRepository(_context);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistTheClaimRequest()
    {
        // Arrange
        var request = ArtistClaimRequestEntity.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        await _repository.AddAsync(request);
        await _context.SaveChangesAsync();

        // Assert
        (await _context.ArtistClaimRequests.FindAsync(request.Id))
            .Should()
            .NotBeNull();
    }

    [Fact]
    public async Task ExistsForArtistAndUserAsync_ShouldOnlyMatchTheExactArtistAndUserPair()
    {
        // Arrange
        var artistId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _context.ArtistClaimRequests.Add(ArtistClaimRequestEntity.Create(Guid.NewGuid(), artistId, userId));
        await _context.SaveChangesAsync();

        // Act
        bool samePair = await _repository.ExistsForArtistAndUserAsync(artistId, userId);
        bool otherUser = await _repository.ExistsForArtistAndUserAsync(artistId, Guid.NewGuid());
        bool otherArtist = await _repository.ExistsForArtistAndUserAsync(Guid.NewGuid(), userId);

        // Assert
        samePair.Should().BeTrue();
        otherUser.Should().BeFalse();
        otherArtist.Should().BeFalse();
    }
}
