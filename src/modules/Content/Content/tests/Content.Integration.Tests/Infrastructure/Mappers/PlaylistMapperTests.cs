using _116.Content.Application.Interactions.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Tests.TestData.Helpers;
using MapsterMapper;

namespace _116.Content.Integration.Tests.Infrastructure.Mappers;

/// <summary>
/// Integration tests for <see cref="PlaylistMapper" />.
/// Verifies entity-to-DTO mapping with video navigation properties from PostgreSQL.
/// </summary>
[Collection("Database")]
public class PlaylistMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = ContentMapperFactory.Create();

    [Fact]
    public async Task CreateManyAsync_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var playlist = PlaylistFactory.Create(User.VisitorId);
        seedContext.Playlists.Add(playlist);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<PlaylistEntity> loaded = await readContext.Playlists.Include(p => p.Videos).ToListAsync();

        var playlistDtoService = Resolve<IPlaylistDtoService>();
        IReadOnlyList<PlaylistDto> dtos = await playlistDtoService.CreateManyAsync(loaded);

        dtos.Should().ContainSingle();
        dtos[0].Id.Should().Be(playlist.Id);
        dtos[0].Name.Should().Be(playlist.Name);
        dtos[0].VideoCount.Should().Be(0);
        dtos[0].ThumbnailUrls.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateDetailAsync_WithVideos_ShouldMapVideoCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        // Only published videos surface in a playlist projection, so the entry must be published.
        var video = VideoFactory.CreatePublished(category.Id);
        video.WithShareCount(1);
        seedContext.Videos.Add(video);
        await seedContext.SaveChangesAsync();

        var playlist = PlaylistFactory.Create(User.VisitorId);
        seedContext.Playlists.Add(playlist);
        await seedContext.SaveChangesAsync();

        seedContext.Playlists.Attach(playlist).Entity.AddVideo(videoId: video.Id, sortOrder: 1);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        PlaylistEntity loaded = await readContext.Playlists.Include(p => p.Videos).FirstAsync(p => p.Id == playlist.Id);

        var playlistDtoService = Resolve<IPlaylistDtoService>();
        PlaylistDetailDto dto = await playlistDtoService.CreateDetailAsync(loaded);

        dto.Id.Should().Be(playlist.Id);
        dto.Videos.Should().ContainSingle();
        dto.Videos[0].Title.Should().Be(video.Title);
        dto.Videos[0].ShareCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateDetailAsync_WithNoVideos_ShouldMapEmptyCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var playlist = PlaylistFactory.Create(User.VisitorId);
        seedContext.Playlists.Add(playlist);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        PlaylistEntity loaded = await readContext.Playlists.Include(p => p.Videos).FirstAsync(p => p.Id == playlist.Id);

        var playlistDtoService = Resolve<IPlaylistDtoService>();
        PlaylistDetailDto dto = await playlistDtoService.CreateDetailAsync(loaded);

        dto.Videos.Should().BeEmpty();
    }
}
