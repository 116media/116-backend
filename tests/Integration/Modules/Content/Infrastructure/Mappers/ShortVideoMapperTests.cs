using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Tests.Fixtures.Factories.Identity;
using MapsterMapper;
using ContentMappingRegistration = _116.Content.Application.Shared.Mappers.MappingRegistration;

namespace _116.Integration.Tests.Modules.Content.Mappers;

/// <summary>
/// Integration tests for <see cref="ShortVideoMapper" />.
/// Verifies entity-to-DTO mapping with file resolution from PostgreSQL.
/// </summary>
[Collection("Database")]
public class ShortVideoMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = new Mapper(ContentMappingRegistration.CreateConfiguration());

    [Fact]
    public async Task ToShortVideoDtoAsync_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var shortVideo = ShortVideoFactory.Create();
        seedContext.ShortVideos.Add(shortVideo);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        ShortVideoEntity loaded = await readContext.ShortVideos.FirstAsync(sv => sv.Id == shortVideo.Id);

        var fileRepository = Resolve<IFileRepository>();
        ShortVideoDto dto = await loaded.ToShortVideoDtoAsync(_mapper, fileRepository);

        dto.Id.Should().Be(loaded.Id);
        dto.Title.Should().Be(loaded.Title);
    }

    [Fact]
    public async Task ToShortVideoDtoAsync_WithVideoFile_ShouldResolveVideoUrlAndAutoThumbnail()
    {
        await using var contentContext = CreateDbContext<ContentDbContext>();
        var shortVideo = ShortVideoFactory.Create();
        contentContext.ShortVideos.Add(shortVideo);
        await contentContext.SaveChangesAsync();

        await using var coreContext = CreateDbContext<CoreDbContext>();
        FileEntity videoFile = FileFactory.CreateWithId(shortVideo.VideoFileId!.Value);
        coreContext.Files.Add(videoFile);
        await coreContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        ShortVideoEntity loaded = await readContext.ShortVideos.FirstAsync(sv => sv.Id == shortVideo.Id);

        var fileRepository = Resolve<IFileRepository>();
        ShortVideoDto dto = await loaded.ToShortVideoDtoAsync(_mapper, fileRepository);

        dto.VideoUrl.Should().Be(videoFile.StorageUrl);
        dto.ThumbnailUrl.Should().NotBeNull();
    }

    [Fact]
    public async Task ToShortVideoDtoAsync_WithUploadedThumbnail_ShouldResolveThumbnailUrl()
    {
        await using var contentContext = CreateDbContext<ContentDbContext>();
        var shortVideo = ShortVideoFactory.CreateWithThumbnail();
        contentContext.ShortVideos.Add(shortVideo);
        await contentContext.SaveChangesAsync();

        await using var coreContext = CreateDbContext<CoreDbContext>();
        FileEntity videoFile = FileFactory.CreateWithId(shortVideo.VideoFileId!.Value);
        FileEntity thumbnailFile = FileFactory.CreateWithId(shortVideo.ThumbnailFileId!.Value);
        coreContext.Files.AddRange(videoFile, thumbnailFile);
        await coreContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        ShortVideoEntity loaded = await readContext.ShortVideos.FirstAsync(sv => sv.Id == shortVideo.Id);

        var fileRepository = Resolve<IFileRepository>();
        ShortVideoDto dto = await loaded.ToShortVideoDtoAsync(_mapper, fileRepository);

        dto.ThumbnailUrl.Should().Be(thumbnailFile.StorageUrl);
    }

    [Fact]
    public async Task ToShortVideoDtosAsync_ShouldMapCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var sv1 = ShortVideoFactory.Create();
        var sv2 = ShortVideoFactory.Create();
        seedContext.ShortVideos.AddRange(sv1, sv2);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<ShortVideoEntity> loaded = await readContext.ShortVideos.ToListAsync();

        var fileRepository = Resolve<IFileRepository>();
        IReadOnlyList<ShortVideoDto> dtos = await loaded.ToShortVideoDtosAsync(_mapper, fileRepository);

        dtos.Should().HaveCount(2);
    }

    [Fact]
    public async Task ToShortVideoDtosAsync_WithAuthorAndFlags_ShouldBatchResolveFilesAndFlags()
    {
        await using var contentContext = CreateDbContext<ContentDbContext>();
        var sv1 = ShortVideoFactory.Create();
        var sv2 = ShortVideoFactory.Create();
        contentContext.ShortVideos.AddRange(sv1, sv2);
        await contentContext.SaveChangesAsync();

        await using var coreContext = CreateDbContext<CoreDbContext>();
        FileEntity file1 = FileFactory.CreateWithId(sv1.VideoFileId!.Value);
        FileEntity file2 = FileFactory.CreateWithId(sv2.VideoFileId!.Value);
        coreContext.Files.AddRange(file1, file2);
        await coreContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<ShortVideoEntity> loaded = await readContext.ShortVideos.ToListAsync();

        var fileRepository = Resolve<IFileRepository>();
        var userLookup = Resolve<IUserLookupService>();
        IReadOnlySet<Guid> liked = new HashSet<Guid> { sv1.Id };
        IReadOnlySet<Guid> bookmarked = new HashSet<Guid> { sv2.Id };

        IReadOnlyList<ShortVideoDto> dtos = await loaded.ToShortVideoDtosAsync(
            _mapper,
            userLookup,
            fileRepository,
            liked,
            bookmarked
        );

        dtos.Should().HaveCount(2);
        dtos.Should().OnlyContain(dto => dto.VideoUrl != null);
        dtos.Single(dto => dto.Id == sv1.Id).IsLiked.Should().BeTrue();
        dtos.Single(dto => dto.Id == sv2.Id).IsBookmarked.Should().BeTrue();
    }

    [Fact]
    public async Task ToShortVideoDtosAsync_WithResolvedAuthorAndUploadedThumbnail_ShouldResolveBoth()
    {
        // Seed a user that owns an avatar file so the author + its avatar resolve; a withThumb
        // short so the batch thumbnail-map branch fires.
        FileEntity avatarFile = FileFactory.Create();
        await using (var coreSeed = CreateDbContext<CoreDbContext>())
        {
            coreSeed.Files.Add(avatarFile);
            await coreSeed.SaveChangesAsync();
        }

        UserEntity author = UserFactory.Create();
        author.UpdateAvatar(avatarFile.Id, EnumAvatarSource.Manual);
        await using (var identitySeed = CreateDbContext<IdentityDbContext>())
        {
            identitySeed.Users.Add(author);
            await identitySeed.SaveChangesAsync();
        }

        var authored = ShortVideoFactory.CreateWithAuthorId(author.Id);
        var withThumb = ShortVideoFactory.CreateWithThumbnail();
        FileEntity thumbFile = FileFactory.CreateWithId(withThumb.ThumbnailFileId!.Value);
        await using (var contentSeed = CreateDbContext<ContentDbContext>())
        {
            contentSeed.ShortVideos.AddRange(authored, withThumb);
            await contentSeed.SaveChangesAsync();
        }
        await using (var coreSeed2 = CreateDbContext<CoreDbContext>())
        {
            coreSeed2.Files.AddRange(
                FileFactory.CreateWithId(authored.VideoFileId!.Value),
                FileFactory.CreateWithId(withThumb.VideoFileId!.Value),
                thumbFile
            );
            await coreSeed2.SaveChangesAsync();
        }

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<ShortVideoEntity> loaded = await readContext.ShortVideos.ToListAsync();

        var fileRepository = Resolve<IFileRepository>();
        var userLookup = Resolve<IUserLookupService>();

        IReadOnlyList<ShortVideoDto> dtos = await loaded.ToShortVideoDtosAsync(_mapper, userLookup, fileRepository);

        ShortVideoDto authoredDto = dtos.Single(dto => dto.Id == authored.Id);
        authoredDto.Author.Should().NotBeNull();
        authoredDto.Author!.AvatarUrl.Should().Be(avatarFile.StorageUrl);
        dtos.Single(dto => dto.Id == withThumb.Id).ThumbnailUrl.Should().Be(thumbFile.StorageUrl);
    }

    [Fact]
    public async Task ToShortVideoDtosAsync_WithoutAuthorWithFlags_ShouldStampFlags()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var shortVideo = ShortVideoFactory.Create();
        seedContext.ShortVideos.Add(shortVideo);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<ShortVideoEntity> loaded = await readContext.ShortVideos.ToListAsync();

        var fileRepository = Resolve<IFileRepository>();
        IReadOnlySet<Guid> liked = new HashSet<Guid> { shortVideo.Id };
        IReadOnlySet<Guid> bookmarked = new HashSet<Guid>();

        IReadOnlyList<ShortVideoDto> dtos = await loaded.ToShortVideoDtosAsync(
            _mapper,
            fileRepository,
            liked,
            bookmarked
        );

        dtos.Single(dto => dto.Id == shortVideo.Id).IsLiked.Should().BeTrue();
        dtos.Single(dto => dto.Id == shortVideo.Id).IsBookmarked.Should().BeFalse();
    }
}
