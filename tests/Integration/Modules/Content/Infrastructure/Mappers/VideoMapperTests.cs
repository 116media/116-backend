using _116.Content.Application.Editorial.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Core.Contracts.Application.Services;
using _116.Tests.Fixtures.Factories.Content;
using MapsterMapper;
using ContentMappingRegistration = _116.Content.Application.Shared.Mappers.MappingRegistration;

namespace _116.Integration.Tests.Modules.Content.Mappers;

/// <summary>
/// Integration tests for <see cref="VideoMapper" />.
/// Verifies entity-to-DTO mapping with navigation properties and file resolution.
/// </summary>
[Collection("Database")]
public class VideoMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = new Mapper(ContentMappingRegistration.CreateConfiguration());

    [Fact]
    public async Task CreateManyAsync_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id, "Music", "music");
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var video = VideoFactory.Create(category.Id);
        seedContext.Videos.Add(video);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        VideoEntity loaded = await readContext.Videos.Include(v => v.Category).FirstAsync(v => v.Id == video.Id);

        var videoDtoFactory = Resolve<IVideoDtoFactory>();
        IReadOnlyList<VideoEntity> videos = [loaded];
        VideoSummaryDto dto = (await videoDtoFactory.CreateManyAsync(videos)).Single();

        dto.Id.Should().Be(loaded.Id);
        dto.CategoryId.Should().Be(category.Id);
        dto.CategoryName.Should().Be("Music");
        dto.Title.Should().Be(loaded.Title);
        dto.Slug.Should().Be(loaded.Slug);
        dto.Status.Should().Be(loaded.Status);
    }

    [Fact]
    public async Task CreateManyAsync_WithNoThumbnail_ShouldMapThumbnailUrlAsNull()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var video = VideoFactory.Create(category.Id);
        seedContext.Videos.Add(video);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        VideoEntity loaded = await readContext.Videos.Include(v => v.Category).FirstAsync(v => v.Id == video.Id);

        var videoDtoFactory = Resolve<IVideoDtoFactory>();
        IReadOnlyList<VideoEntity> videos = [loaded];
        VideoSummaryDto dto = (await videoDtoFactory.CreateManyAsync(videos)).Single();

        dto.ThumbnailUrl.Should().BeNull();
    }

    [Fact]
    public async Task CreateDetailAsync_ShouldMapAllDetailFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id, "Culture", "culture");
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var video = VideoFactory.Create(category.Id);
        seedContext.Videos.Add(video);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        VideoEntity loaded = await readContext
            .Videos.Include(v => v.Category)
            .Include(v => v.Tags)
                .ThenInclude(vt => vt.Tag)
            .Include(v => v.PromotionLevel)
            .Include(v => v.Customer)
            .FirstAsync(v => v.Id == video.Id);

        var videoDtoFactory = Resolve<IVideoDtoFactory>();
        VideoDetailDto dto = await videoDtoFactory.CreateDetailAsync(loaded);

        dto.Id.Should().Be(loaded.Id);
        dto.CategoryId.Should().Be(category.Id);
        dto.CategoryName.Should().Be("Culture");
        dto.Title.Should().Be(loaded.Title);
        dto.Slug.Should().Be(loaded.Slug);
    }

    [Fact]
    public async Task CreateManyAsync_ShouldMapCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var v1 = VideoFactory.Create(category.Id);
        var v2 = VideoFactory.Create(category.Id);
        seedContext.Videos.AddRange(v1, v2);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<VideoEntity> loaded = await readContext.Videos.Include(v => v.Category).ToListAsync();

        var videoDtoFactory = Resolve<IVideoDtoFactory>();
        IReadOnlyList<VideoSummaryDto> dtos = await videoDtoFactory.CreateManyAsync(loaded);

        dtos.Should().HaveCount(2);
    }
}
