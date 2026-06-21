using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using MapsterMapper;
using ContentMappingRegistration = _116.Content.Application.Shared.Mappers.MappingRegistration;

namespace _116.Integration.Tests.Modules.Content.Mappers;

/// <summary>
/// Integration tests for <see cref="CustomerMapper" />.
/// Verifies entity-to-DTO mapping with real PostgreSQL entities.
/// </summary>
[Collection("Database")]
public class CustomerMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = new Mapper(ContentMappingRegistration.CreateConfiguration());

    [Fact]
    public async Task ToCustomerDto_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var entity = CustomerFactory.Create("client@example.com");
        seedContext.Customers.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        CustomerEntity loaded = await readContext.Customers.FirstAsync(c => c.Id == entity.Id);

        CustomerDto dto = loaded.ToCustomerDto(_mapper);

        dto.Id.Should().Be(loaded.Id);
        dto.FullName.Should().Be(loaded.FullName);
        dto.Email.Should().Be("client@example.com");
        dto.Phone.Should().Be(loaded.Phone);
        dto.Company.Should().Be(loaded.Company);
        dto.Notes.Should().Be(loaded.Notes);
    }

    [Fact]
    public async Task ToCustomerDtos_ShouldMapCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var c1 = CustomerFactory.Create("alice@example.com");
        var c2 = CustomerFactory.Create("bob@example.com");
        seedContext.Customers.AddRange(c1, c2);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<CustomerEntity> loaded = await readContext.Customers.ToListAsync();

        IReadOnlyList<CustomerDto> dtos = loaded.ToCustomerDtos(_mapper);

        dtos.Should().HaveCount(2);
        dtos.Select(d => d.Email).Should().BeEquivalentTo(["alice@example.com", "bob@example.com"]);
    }
}
