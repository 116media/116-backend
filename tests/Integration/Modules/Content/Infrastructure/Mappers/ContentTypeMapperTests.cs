using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using MapsterMapper;
using ContentMappingRegistration = _116.Content.Application.Shared.Mappers.MappingRegistration;

namespace _116.Integration.Tests.Modules.Content.Mappers;

/// <summary>
/// Integration tests for <see cref="ContentTypeMapper" />.
/// Verifies entity-to-DTO mapping with real PostgreSQL entities.
/// </summary>
[Collection("Database")]
public class ContentTypeMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = new Mapper(ContentMappingRegistration.CreateConfiguration());

    [Fact]
    public async Task ToContentTypeDto_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var entity = ContentTypeFactory.Create("IntegrationVideo");
        seedContext.ContentTypes.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        ContentTypeEntity loaded = await readContext.ContentTypes.FirstAsync(ct => ct.Id == entity.Id);

        ContentTypeDto dto = loaded.ToContentTypeDto(_mapper);

        dto.Id.Should().Be(loaded.Id);
        dto.Name.Should().Be("IntegrationVideo");
        dto.IsActive.Should().Be(loaded.IsActive);
    }

    [Fact]
    public async Task ToContentTypeDtos_ShouldMapCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var ct1 = ContentTypeFactory.Create("Article");
        var ct2 = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.AddRange(ct1, ct2);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<ContentTypeEntity> loaded = await readContext.ContentTypes.ToListAsync();

        IReadOnlyList<ContentTypeDto> dtos = loaded.ToContentTypeDtos(_mapper);

        dtos.Should().HaveCount(2);
        dtos.Select(d => d.Name).Should().BeEquivalentTo(["Article", "Video"]);
    }

    [Fact]
    public async Task ToContentTypeDto_InactiveEntity_ShouldMapIsActiveFalse()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var entity = ContentTypeFactory.CreateInactive();
        seedContext.ContentTypes.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        ContentTypeEntity loaded = await readContext.ContentTypes.FirstAsync(ct => ct.Id == entity.Id);

        ContentTypeDto dto = loaded.ToContentTypeDto(_mapper);

        dto.IsActive.Should().BeFalse();
    }
}
