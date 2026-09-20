using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Mappers;

/// <summary>
/// Integration tests for <see cref="ContentOrderMapper" /> through <see cref="IContentOrderDtoFactory" />.
/// Verifies that the customer, category and tier names the projections carry are resolved from
/// PostgreSQL by the factory's batched lookups rather than by a navigation.
/// </summary>
[Collection("Database")]
public class ContentOrderMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task CreateSummaryAsync_ShouldResolveTheCustomerName()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create("order-test@example.com");
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        var factory = Resolve<IContentOrderDtoFactory>();

        ContentOrderSummaryDto dto = await factory.CreateSummaryAsync(order);

        dto.Id.Should().Be(order.Id);
        dto.Status.Should().Be(order.Status);
        dto.CustomerName.Should().Be(customer.FullName);
        dto.ItemCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateManySummariesAsync_ShouldResolveEveryCustomerNameInOneBatch()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create("bulk-test@example.com");
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        var firstOrder = ContentOrderFactory.CreateForCustomer(customer.Id);
        var secondOrder = ContentOrderFactory.CreateForCustomer(customer.Id);
        seedContext.ContentOrders.AddRange(firstOrder, secondOrder);
        await seedContext.SaveChangesAsync();

        var factory = Resolve<IContentOrderDtoFactory>();

        IReadOnlyList<ContentOrderSummaryDto> dtos = await factory.CreateManySummariesAsync([firstOrder, secondOrder]);

        dtos.Should().HaveCount(2);
        dtos.Should().AllSatisfy(dto => dto.CustomerName.Should().Be(customer.FullName));
    }

    [Fact]
    public async Task CreateDetailAsync_ShouldResolveTheCustomerAndItemCategoryNames()
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
            .ContentOrders.Include(o => o.Items)
                .ThenInclude(i => i.Tiers)
            .Include(o => o.Payment)
            .FirstAsync(o => o.Id == order.Id);

        var factory = Resolve<IContentOrderDtoFactory>();

        ContentOrderDetailDto dto = await factory.CreateDetailAsync(loaded);

        dto.Id.Should().Be(loaded.Id);
        dto.CustomerName.Should().Be(customer.FullName);
        dto.Items.Should().ContainSingle();
        dto.Items[0].CategoryName.Should().Be("Music");
    }
}
