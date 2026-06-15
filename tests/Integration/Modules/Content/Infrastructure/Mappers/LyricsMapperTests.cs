using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using MapsterMapper;
using ContentMappingRegistration = _116.Content.Application.Shared.Mappers.MappingRegistration;

namespace _116.Integration.Tests.Modules.Content.Mappers;

/// <summary>
/// Integration tests for <see cref="LyricsMapper" />.
/// Verifies entity-to-DTO mapping with real PostgreSQL entities.
/// </summary>
[Collection("Database")]
public class LyricsMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = new Mapper(ContentMappingRegistration.CreateConfiguration());

    [Fact]
    public async Task ToLyricsDto_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var lyrics = LyricsFactory.Create("Indépendance Cha Cha", "Grand Kallé");
        seedContext.Lyrics.Add(lyrics);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        LyricsEntity loaded = await readContext.Lyrics.FirstAsync(l => l.Id == lyrics.Id);

        LyricsDto dto = loaded.ToLyricsDto(_mapper);

        dto.Id.Should().Be(loaded.Id);
        dto.SongTitle.Should().Be("Indépendance Cha Cha");
        dto.ArtistName.Should().Be("Grand Kallé");
        dto.LyricsText.Should().Be(loaded.LyricsText);
        dto.Language.Should().Be(loaded.Language);
    }

    [Fact]
    public async Task ToLyricsDto_ForVideo_ShouldMapVideoId()
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

        var lyrics = LyricsFactory.CreateForVideo(video.Id);
        seedContext.Lyrics.Add(lyrics);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        LyricsEntity loaded = await readContext.Lyrics.FirstAsync(l => l.Id == lyrics.Id);

        LyricsDto dto = loaded.ToLyricsDto(_mapper);

        dto.VideoId.Should().Be(video.Id);
    }

    [Fact]
    public async Task ToLyricsDtos_ShouldMapCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var l1 = LyricsFactory.Create("Song One", "Artist A");
        var l2 = LyricsFactory.Create("Song Two", "Artist B");
        seedContext.Lyrics.AddRange(l1, l2);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<LyricsEntity> loaded = await readContext.Lyrics.ToListAsync();

        IReadOnlyList<LyricsDto> dtos = loaded.ToLyricsDtos(_mapper);

        dtos.Should().HaveCount(2);
        dtos.Select(d => d.SongTitle).Should().BeEquivalentTo(["Song One", "Song Two"]);
    }
}
