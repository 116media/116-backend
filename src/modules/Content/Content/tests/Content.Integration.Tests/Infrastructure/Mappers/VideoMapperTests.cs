using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using MapsterMapper;

namespace _116.Content.Integration.Tests.Infrastructure.Mappers;

/// <summary>
/// Integration tests for <see cref="VideoMapper" />.
/// Verifies entity-to-DTO mapping with navigation properties and file resolution.
/// </summary>
[Collection("Database")]
public class VideoMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = ContentMapperFactory.Create();

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
        VideoEntity loaded = await readContext.Videos.FirstAsync(v => v.Id == video.Id);

        var videoDtoService = Resolve<IVideoDtoService>();
        IReadOnlyList<VideoEntity> videos = [loaded];
        VideoSummaryDto dto = (await videoDtoService.CreateManyAsync(videos)).Single();

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
        VideoEntity loaded = await readContext.Videos.FirstAsync(v => v.Id == video.Id);

        var videoDtoService = Resolve<IVideoDtoService>();
        IReadOnlyList<VideoEntity> videos = [loaded];
        VideoSummaryDto dto = (await videoDtoService.CreateManyAsync(videos)).Single();

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
        VideoEntity loaded = await readContext.Videos.Include(v => v.Tags).FirstAsync(v => v.Id == video.Id);

        var videoDtoService = Resolve<IVideoDtoService>();
        VideoDetailDto dto = await videoDtoService.CreateDetailAsync(loaded);

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
        List<VideoEntity> loaded = await readContext.Videos.ToListAsync();

        var videoDtoService = Resolve<IVideoDtoService>();
        IReadOnlyList<VideoSummaryDto> dtos = await videoDtoService.CreateManyAsync(loaded);

        dtos.Should().HaveCount(2);
    }
}
