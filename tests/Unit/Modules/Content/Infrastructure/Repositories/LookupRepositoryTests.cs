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
/// Unit tests for <see cref="LookupRepository"/> using InMemory database.
/// </summary>
public class LookupRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly LookupRepository _repository;

    public LookupRepositoryTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new LookupRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region ContentType Tests

    [Fact]
    public async Task AddContentTypeAsync_ShouldPersistEntity()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Article");

        // Act
        await _repository.AddContentTypeAsync(contentType);
        await _context.SaveChangesAsync();

        // Assert
        ContentTypeEntity? retrieved = await _context.ContentTypes.FindAsync(contentType.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Article");
    }

    [Fact]
    public async Task GetContentTypeByIdOrThrowAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create("Video");
        _context.ContentTypes.Add(contentType);
        await _context.SaveChangesAsync();

        // Act
        ContentTypeEntity result = await _repository.GetContentTypeByIdOrThrowAsync(contentType.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(contentType.Id);
    }

    [Fact]
    public async Task GetContentTypeByIdOrThrowAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetContentTypeByIdOrThrowAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAllContentTypesAsync_ShouldReturnAllOrderedByName()
    {
        // Arrange
        _context.ContentTypes.Add(ContentTypeFactory.Create("Video"));
        _context.ContentTypes.Add(ContentTypeFactory.Create("Article"));
        _context.ContentTypes.Add(ContentTypeFactory.Create("Short"));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<ContentTypeEntity> result = await _repository.GetAllContentTypesAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Article");
        result[1].Name.Should().Be("Short");
        result[2].Name.Should().Be("Video");
    }

    // NOTE: ContentTypeExistsByNameAsync uses ILike which is not supported by InMemoryDatabase provider.
    // This method is tested in integration tests.

    #endregion

    #region PricingTier Tests

    [Fact]
    public async Task AddPricingTierAsync_ShouldPersistEntity()
    {
        // Arrange
        PricingTierEntity pricingTier = PricingTierFactory.Create("base_upload");

        // Act
        await _repository.AddPricingTierAsync(pricingTier);
        await _context.SaveChangesAsync();

        // Assert
        PricingTierEntity? retrieved = await _context.PricingTiers.FindAsync(pricingTier.Id);
        retrieved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPricingTierByIdOrThrowAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        PricingTierEntity pricingTier = PricingTierFactory.CreateDefault();
        _context.PricingTiers.Add(pricingTier);
        await _context.SaveChangesAsync();

        // Act
        PricingTierEntity result = await _repository.GetPricingTierByIdOrThrowAsync(pricingTier.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(pricingTier.Id);
    }

    [Fact]
    public async Task GetPricingTierByIdOrThrowAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetPricingTierByIdOrThrowAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAllPricingTiersAsync_ShouldReturnAllOrderedByName()
    {
        // Arrange
        _context.PricingTiers.Add(PricingTierFactory.Create("social_boost"));
        _context.PricingTiers.Add(PricingTierFactory.Create("base_upload"));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<PricingTierEntity> result = await _repository.GetAllPricingTiersAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("base_upload");
        result[1].Name.Should().Be("social_boost");
    }

    // NOTE: PricingTierExistsByNameAsync uses ILike — tested in integration tests.

    #endregion

    #region PromotionLevel Tests

    [Fact]
    public async Task AddPromotionLevelAsync_ShouldPersistEntity()
    {
        // Arrange
        PromotionLevelEntity promotionLevel = PromotionLevelFactory.Create();

        // Act
        await _repository.AddPromotionLevelAsync(promotionLevel);
        await _context.SaveChangesAsync();

        // Assert
        PromotionLevelEntity? retrieved = await _context.PromotionLevels.FindAsync(promotionLevel.Id);
        retrieved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPromotionLevelByIdOrThrowAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        PromotionLevelEntity promotionLevel = PromotionLevelFactory.CreateDefault();
        _context.PromotionLevels.Add(promotionLevel);
        await _context.SaveChangesAsync();

        // Act
        PromotionLevelEntity result = await _repository.GetPromotionLevelByIdOrThrowAsync(promotionLevel.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(promotionLevel.Id);
    }

    [Fact]
    public async Task GetPromotionLevelByIdOrThrowAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetPromotionLevelByIdOrThrowAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAllPromotionLevelsAsync_ShouldReturnAll()
    {
        // Arrange
        _context.PromotionLevels.Add(PromotionLevelFactory.Create());
        _context.PromotionLevels.Add(PromotionLevelFactory.Create());
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<PromotionLevelEntity> result = await _repository.GetAllPromotionLevelsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActivePromotionLevelsAsync_ShouldReturnOnlyActiveEntities()
    {
        // Arrange
        _context.PromotionLevels.Add(PromotionLevelFactory.CreateDefault());
        _context.PromotionLevels.Add(PromotionLevelFactory.CreateInactive());
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<PromotionLevelEntity> result = await _repository.GetActivePromotionLevelsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].IsActive.Should().BeTrue();
    }

    // NOTE: PromotionLevelExistsByNameAsync uses ILike — tested in integration tests.

    #endregion

    #region Tag Tests

    [Fact]
    public async Task AddTagAsync_ShouldPersistEntity()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();

        // Act
        await _repository.AddTagAsync(tag);
        await _context.SaveChangesAsync();

        // Assert
        TagEntity? retrieved = await _context.Tags.FindAsync(tag.Id);
        retrieved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTagBySlugAsync_WhenFound_ShouldReturnEntity()
    {
        // Arrange
        TagEntity tag = TagFactory.CreateDefault();
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        // Act
        TagEntity? result = await _repository.GetTagBySlugAsync(tag.Slug);

        // Assert
        result.Should().NotBeNull();
        result!.Slug.Should().Be(tag.Slug);
    }

    [Fact]
    public async Task GetTagBySlugAsync_WhenNotFound_ShouldReturnNull()
    {
        // Act
        TagEntity? result = await _repository.GetTagBySlugAsync("non-existent-slug");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllTagsAsync_WithNoSearch_ShouldReturnAllTagsOrderedByName()
    {
        // Arrange
        _context.Tags.Add(TagFactory.Create("Kinshasa", "kinshasa"));
        _context.Tags.Add(TagFactory.Create("Fally Ipupa", "fally-ipupa"));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<TagEntity> result = await _repository.GetAllTagsAsync(search: null);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Fally Ipupa");
        result[1].Name.Should().Be("Kinshasa");
    }

    #endregion
}
