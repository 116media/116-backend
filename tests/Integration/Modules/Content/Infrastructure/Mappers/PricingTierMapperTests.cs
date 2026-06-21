using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using MapsterMapper;
using ContentMappingRegistration = _116.Content.Application.Shared.Mappers.MappingRegistration;

namespace _116.Integration.Tests.Modules.Content.Mappers;

/// <summary>
/// Integration tests for <see cref="PricingTierMapper" />.
/// Verifies entity-to-DTO mapping with real PostgreSQL entities.
/// </summary>
[Collection("Database")]
public class PricingTierMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = new Mapper(ContentMappingRegistration.CreateConfiguration());

    [Fact]
    public async Task ToPricingTierDto_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var entity = PricingTierFactory.CreateWithDescription("base_upload", "Base upload fee");
        seedContext.PricingTiers.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        PricingTierEntity loaded = await readContext.PricingTiers.FirstAsync(pt => pt.Id == entity.Id);

        PricingTierDto dto = loaded.ToPricingTierDto(_mapper);

        dto.Id.Should().Be(loaded.Id);
        dto.Name.Should().Be("base_upload");
        dto.Description.Should().Be("Base upload fee");
        dto.IsActive.Should().Be(loaded.IsActive);
    }

    [Fact]
    public async Task ToPricingTierDtos_ShouldMapCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var pt1 = PricingTierFactory.Create("standard");
        var pt2 = PricingTierFactory.Create("premium");
        seedContext.PricingTiers.AddRange(pt1, pt2);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<PricingTierEntity> loaded = await readContext.PricingTiers.ToListAsync();

        IReadOnlyList<PricingTierDto> dtos = loaded.ToPricingTierDtos(_mapper);

        dtos.Should().HaveCount(2);
        dtos.Select(d => d.Name).Should().BeEquivalentTo(["standard", "premium"]);
    }

    [Fact]
    public async Task ToPricingTierDto_InactiveEntity_ShouldMapIsActiveFalse()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var entity = PricingTierFactory.CreateInactive();
        seedContext.PricingTiers.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        PricingTierEntity loaded = await readContext.PricingTiers.FirstAsync(pt => pt.Id == entity.Id);

        PricingTierDto dto = loaded.ToPricingTierDto(_mapper);

        dto.IsActive.Should().BeFalse();
    }
}
