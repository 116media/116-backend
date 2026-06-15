using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using MapsterMapper;
using ContentMappingRegistration = _116.Content.Application.Shared.Mappers.MappingRegistration;

namespace _116.Integration.Tests.Modules.Content.Mappers;

/// <summary>
/// Integration tests for <see cref="TagMapper" />.
/// Verifies entity-to-DTO mapping with real PostgreSQL entities.
/// </summary>
[Collection("Database")]
public class TagMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = new Mapper(ContentMappingRegistration.CreateConfiguration());

    [Fact]
    public async Task ToTagDto_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var entity = TagFactory.Create("Hip-Hop", "hip-hop");
        seedContext.Tags.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        TagEntity loaded = await readContext.Tags.FirstAsync(t => t.Id == entity.Id);

        TagDto dto = loaded.ToTagDto(_mapper);

        dto.Id.Should().Be(loaded.Id);
        dto.Name.Should().Be("Hip-Hop");
        dto.Slug.Should().Be("hip-hop");
    }

    [Fact]
    public async Task ToTagDtos_ShouldMapCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var tag1 = TagFactory.Create("Rumba", "rumba");
        var tag2 = TagFactory.Create("Ndombolo", "ndombolo");
        seedContext.Tags.AddRange(tag1, tag2);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<TagEntity> loaded = await readContext.Tags.ToListAsync();

        IReadOnlyList<TagDto> dtos = loaded.ToTagDtos(_mapper);

        dtos.Should().HaveCount(2);
        dtos.Select(d => d.Name).Should().BeEquivalentTo(["Rumba", "Ndombolo"]);
        dtos.Select(d => d.Slug).Should().BeEquivalentTo(["rumba", "ndombolo"]);
    }
}
