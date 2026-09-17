using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Core.Contracts.Application.Services;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using MapsterMapper;
using ContentMappingRegistration = _116.Content.Application.Shared.Mappers.MappingRegistration;

namespace _116.Integration.Tests.Modules.Content.Mappers;

/// <summary>
/// Integration tests for <see cref="PlaylistMapper" />.
/// Verifies entity-to-DTO mapping with video navigation properties from PostgreSQL.
/// </summary>
[Collection("Database")]
public class PlaylistMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = new Mapper(ContentMappingRegistration.CreateConfiguration());

    [Fact]
    public async Task ToPlaylistDtosAsync_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var playlist = PlaylistFactory.Create(User.VisitorId);
        seedContext.Playlists.Add(playlist);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<PlaylistEntity> loaded = await readContext.Playlists.Include(p => p.Videos).ToListAsync();

        var fileStorage = Resolve<IFileStorageService>();
        IReadOnlyList<PlaylistDto> dtos = await loaded.ToPlaylistDtosAsync(_mapper, fileStorage);

        dtos.Should().ContainSingle();
        dtos[0].Id.Should().Be(playlist.Id);
        dtos[0].Name.Should().Be(playlist.Name);
        dtos[0].VideoCount.Should().Be(0);
        dtos[0].ThumbnailUrls.Should().BeEmpty();
    }

    [Fact]
    public async Task ToPlaylistDetailDtoAsync_WithVideos_ShouldMapVideoCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        var video = VideoFactory.Create(category.Id);
        video.WithShareCount(1);
        seedContext.Videos.Add(video);
        await seedContext.SaveChangesAsync();

        var playlist = PlaylistFactory.Create(User.VisitorId);
        seedContext.Playlists.Add(playlist);
        await seedContext.SaveChangesAsync();

        seedContext.PlaylistVideos.Add(PlaylistVideoEntity.Create(Guid.NewGuid(), playlist.Id, video.Id, sortOrder: 1));
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        PlaylistEntity loaded = await readContext
            .Playlists.Include(p => p.Videos)
                .ThenInclude(pv => pv.Video)
            .FirstAsync(p => p.Id == playlist.Id);

        var fileStorage = Resolve<IFileStorageService>();
        PlaylistDetailDto dto = await loaded.ToPlaylistDetailDtoAsync(_mapper, fileStorage);

        dto.Id.Should().Be(playlist.Id);
        dto.Videos.Should().ContainSingle();
        dto.Videos[0].Title.Should().Be(video.Title);
        dto.Videos[0].ShareCount.Should().Be(1);
    }

    [Fact]
    public async Task ToPlaylistDetailDtoAsync_WithNoVideos_ShouldMapEmptyCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var playlist = PlaylistFactory.Create(User.VisitorId);
        seedContext.Playlists.Add(playlist);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        PlaylistEntity loaded = await readContext
            .Playlists.Include(p => p.Videos)
                .ThenInclude(pv => pv.Video)
            .FirstAsync(p => p.Id == playlist.Id);

        var fileStorage = Resolve<IFileStorageService>();
        PlaylistDetailDto dto = await loaded.ToPlaylistDetailDtoAsync(_mapper, fileStorage);

        dto.Videos.Should().BeEmpty();
    }
}
