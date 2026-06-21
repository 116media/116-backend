using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using MapsterMapper;
using ContentMappingRegistration = _116.Content.Application.Shared.Mappers.MappingRegistration;

namespace _116.Integration.Tests.Modules.Content.Mappers;

/// <summary>
/// Integration tests for <see cref="PromotionLevelMapper" />.
/// Verifies entity-to-DTO mapping with real PostgreSQL entities.
/// </summary>
[Collection("Database")]
public class PromotionLevelMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = new Mapper(ContentMappingRegistration.CreateConfiguration());

    [Fact]
    public async Task ToPromotionLevelDto_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var entity = PromotionLevelFactory.Create("Gold", 30, 99.99m);
        seedContext.PromotionLevels.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        PromotionLevelEntity loaded = await readContext.PromotionLevels.FirstAsync(pl => pl.Id == entity.Id);

        PromotionLevelDto dto = loaded.ToPromotionLevelDto(_mapper);

        dto.Id.Should().Be(loaded.Id);
        dto.Name.Should().Be("Gold");
        dto.DurationDays.Should().Be(30);
        dto.PriceUsd.Should().Be(99.99m);
        dto.IsActive.Should().Be(loaded.IsActive);
    }

    [Fact]
    public async Task ToPromotionLevelDtos_ShouldMapCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var pl1 = PromotionLevelFactory.Create("Silver", 7, 19.99m);
        var pl2 = PromotionLevelFactory.Create("Gold", 30, 99.99m);
        seedContext.PromotionLevels.AddRange(pl1, pl2);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<PromotionLevelEntity> loaded = await readContext.PromotionLevels.ToListAsync();

        IReadOnlyList<PromotionLevelDto> dtos = loaded.ToPromotionLevelDtos(_mapper);

        dtos.Should().HaveCount(2);
        dtos.Select(d => d.Name).Should().BeEquivalentTo(["Silver", "Gold"]);
    }

    [Fact]
    public async Task ToPromotionLevelDto_InactiveEntity_ShouldMapIsActiveFalse()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var entity = PromotionLevelFactory.CreateInactive();
        seedContext.PromotionLevels.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        PromotionLevelEntity loaded = await readContext.PromotionLevels.FirstAsync(pl => pl.Id == entity.Id);

        PromotionLevelDto dto = loaded.ToPromotionLevelDto(_mapper);

        dto.IsActive.Should().BeFalse();
    }
}
