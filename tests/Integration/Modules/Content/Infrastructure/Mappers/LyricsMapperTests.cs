using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Tests.Fixtures.Factories.Content;
using MapsterMapper;

namespace _116.Integration.Tests.Modules.Content.Mappers;

/// <summary>
/// Integration tests for <see cref="LyricsMapper" />.
/// Verifies entity-to-DTO mapping for the summary and detail lyrics views with real PostgreSQL entities.
/// </summary>
[Collection("Database")]
public class LyricsMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task ToLyricsSummaryDto_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id, "Lyrics Category", "lyrics-category");
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var lyrics = LyricsFactory.Create(category.Id, "Indépendance Cha Cha", "Grand Kallé");
        seedContext.Lyrics.Add(lyrics);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        LyricsEntity loaded = await readContext.Lyrics.Include(l => l.Category).FirstAsync(l => l.Id == lyrics.Id);

        var fileRepository = Resolve<IFileRepository>();
        LyricsSummaryDto dto = await loaded.ToLyricsSummaryDtoAsync(fileRepository);

        dto.Id.Should().Be(loaded.Id);
        dto.CategoryId.Should().Be(category.Id);
        dto.CategoryName.Should().Be("Lyrics Category");
        dto.SongTitle.Should().Be("Indépendance Cha Cha");
        dto.ArtistName.Should().Be("Grand Kallé");
        dto.Slug.Should().Be(loaded.Slug);
        dto.Language.Should().Be(loaded.Language);
        dto.Status.Should().Be(loaded.Status);
    }

    [Fact]
    public async Task ToLyricsSummaryDto_ForVideo_ShouldMapVideoId()
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

        var lyrics = LyricsFactory.CreateForVideo(category.Id, video.Id);
        seedContext.Lyrics.Add(lyrics);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        LyricsEntity loaded = await readContext.Lyrics.FirstAsync(l => l.Id == lyrics.Id);

        var fileRepository = Resolve<IFileRepository>();
        LyricsSummaryDto dto = await loaded.ToLyricsSummaryDtoAsync(fileRepository);

        dto.VideoId.Should().Be(video.Id);
    }

    [Fact]
    public async Task ToLyricsSummaryDtos_ShouldMapCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var l1 = LyricsFactory.Create(category.Id, "Song One", "Artist A");
        var l2 = LyricsFactory.Create(category.Id, "Song Two", "Artist B");
        seedContext.Lyrics.AddRange(l1, l2);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<LyricsEntity> loaded = await readContext.Lyrics.Include(l => l.Category).ToListAsync();

        var fileRepository = Resolve<IFileRepository>();
        IReadOnlyList<LyricsSummaryDto> dtos = await loaded.AsReadOnly().ToLyricsSummaryDtosAsync(fileRepository);

        dtos.Should().HaveCount(2);
        dtos.Select(d => d.SongTitle).Should().BeEquivalentTo(["Song One", "Song Two"]);
    }

    [Fact]
    public async Task ToLyricsDetailDtoAsync_ShouldMapAllDetailFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id, "Culture", "culture-lyrics");
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var lyrics = LyricsFactory.Create(category.Id);
        seedContext.Lyrics.Add(lyrics);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        LyricsEntity loaded = await readContext
            .Lyrics.Include(l => l.Category)
            .Include(l => l.Customer)
            .FirstAsync(l => l.Id == lyrics.Id);

        var mapper = Resolve<IMapper>();
        var userLookup = Resolve<IUserLookupService>();
        var fileRepository = Resolve<IFileRepository>();
        LyricsDetailDto dto = await loaded.ToLyricsDetailDtoAsync(mapper, userLookup, fileRepository);

        dto.Id.Should().Be(loaded.Id);
        dto.CategoryName.Should().Be("Culture");
        dto.SongTitle.Should().Be(loaded.SongTitle);
        dto.ArtistName.Should().Be(loaded.ArtistName);
        dto.LyricsText.Should().Be(loaded.LyricsText);
        dto.Language.Should().Be(loaded.Language);
    }

    [Fact]
    public async Task ToLyricsDetailDtosAsync_ShouldMapCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var l1 = LyricsFactory.Create(category.Id);
        var l2 = LyricsFactory.Create(category.Id);
        seedContext.Lyrics.AddRange(l1, l2);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<LyricsEntity> loaded = await readContext.Lyrics.Include(l => l.Category).ToListAsync();

        var mapper = Resolve<IMapper>();
        var userLookup = Resolve<IUserLookupService>();
        var fileRepository = Resolve<IFileRepository>();
        IReadOnlyList<LyricsDetailDto> dtos = await loaded
            .AsReadOnly()
            .ToLyricsDetailDtosAsync(mapper, userLookup, fileRepository);

        dtos.Should().HaveCount(2);
    }
}
