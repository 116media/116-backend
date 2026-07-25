using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using MapsterMapper;
using ContentMappingRegistration = _116.Content.Application.Shared.Mappers.MappingRegistration;

namespace _116.Integration.Tests.Modules.Content.Mappers;

/// <summary>
/// Integration tests for <see cref="ContentOrderMapper" />.
/// Verifies entity-to-DTO mapping with customer, items, and payment navigation from PostgreSQL.
/// </summary>
[Collection("Database")]
public class ContentOrderMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = new Mapper(ContentMappingRegistration.CreateConfiguration());

    [Fact]
    public async Task ToContentOrderSummaryDto_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create("order-test@example.com");
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        ContentOrderEntity loaded = await readContext
            .ContentOrders.Include(o => o.Customer)
            .Include(o => o.Items)
            .FirstAsync(o => o.Id == order.Id);

        ContentOrderSummaryDto dto = loaded.ToContentOrderSummaryDto(_mapper);

        dto.Id.Should().Be(loaded.Id);
        dto.Status.Should().Be(loaded.Status);
        dto.CustomerName.Should().Be(customer.FullName);
        dto.ItemCount.Should().Be(0);
    }

    [Fact]
    public async Task ToContentOrderSummaryDtos_ShouldMapCollection()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create("bulk-test@example.com");
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var o1 = ContentOrderFactory.CreateForCustomer(customer.Id);
        var o2 = ContentOrderFactory.CreateForCustomer(customer.Id);
        seedContext.ContentOrders.AddRange(o1, o2);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        List<ContentOrderEntity> loaded = await readContext
            .ContentOrders.Include(o => o.Customer)
            .Include(o => o.Items)
            .ToListAsync();

        IReadOnlyList<ContentOrderSummaryDto> dtos = loaded.ToContentOrderSummaryDtos(_mapper);

        dtos.Should().HaveCount(2);
        dtos.Should().AllSatisfy(d => d.CustomerName.Should().Be(customer.FullName));
    }

    [Fact]
    public async Task ToContentOrderDetailDto_ShouldMapWithItems()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("Video");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id, "Music", "music");
        seedContext.Categories.Add(category);

        var customer = CustomerFactory.Create("detail-test@example.com");
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        var orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        seedContext.ContentOrderItems.Add(orderItem);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<ContentDbContext>();
        ContentOrderEntity loaded = await readContext
            .ContentOrders.Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Category)
            .Include(o => o.Items)
                .ThenInclude(i => i.Tiers)
                    .ThenInclude(t => t.PricingTier)
            .Include(o => o.Items)
                .ThenInclude(i => i.PromotionLevel)
            .Include(o => o.Payment)
            .FirstAsync(o => o.Id == order.Id);

        ContentOrderDetailDto dto = loaded.ToContentOrderDetailDto(_mapper);

        dto.Id.Should().Be(loaded.Id);
        dto.CustomerName.Should().Be(customer.FullName);
        dto.Items.Should().ContainSingle();
        dto.Items[0].CategoryName.Should().Be("Music");
    }
}
