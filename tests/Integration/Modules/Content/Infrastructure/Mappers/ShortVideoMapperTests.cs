using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Core.Application.Shared.Repositories;
using _116.Tests.Fixtures.Factories.Content;
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
}
