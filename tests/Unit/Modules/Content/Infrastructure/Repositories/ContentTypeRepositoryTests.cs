using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="ContentTypeRepository"/> using InMemory database.
/// </summary>
public class ContentTypeRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly ContentTypeRepository _repository;

    public ContentTypeRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(new CreatedAtStampingInterceptor())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new ContentTypeRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistEntity()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Article");

        // Act
        await _repository.AddAsync(contentType);
        await _context.SaveChangesAsync();

        // Assert
        ContentTypeEntity? retrieved = await _context.ContentTypes.FindAsync(contentType.Id);
        retrieved.Should().NotBeNull();
        retrieved.Name.Should().Be("Article");
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Video");
        _context.ContentTypes.Add(contentType);
        await _context.SaveChangesAsync();

        // Act
        ContentTypeEntity result = await _repository.GetByIdOrThrowAsync(contentType.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(contentType.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetByIdOrThrowAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllOrderedByName()
    {
        // Arrange
        _context.ContentTypes.Add(ContentTypeFactory.Create("Video"));
        _context.ContentTypes.Add(ContentTypeFactory.Create("Article"));
        _context.ContentTypes.Add(ContentTypeFactory.Create("Short"));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<ContentTypeEntity> result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Article");
        result[1].Name.Should().Be("Short");
        result[2].Name.Should().Be("Video");
    }

    // NOTE: ExistsByNameAsync and the search filter use ILike
    // which is not supported by InMemoryDatabase provider — tested in integration tests.
}
