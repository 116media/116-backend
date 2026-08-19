using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.V1;

/// <summary>
/// Integration tests for the AdminCreateOrder endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateOrderEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateOrder_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        CustomerEntity customer = await SeedAsync<ContentDbContext, CustomerEntity>(ctx =>
        {
            CustomerEntity entity = CustomerFactory.Create();
            ctx.Customers.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminCreateOrderRequest request = new AdminCreateOrderRequestBuilder()
            .WithCustomerId(customer.Id.ToString())
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadAsAsync<AdminCreateOrderResponse>();
        body.Order.Id.Should().NotBeEmpty();
        body.Order.CustomerName.Should().Be(customer.FullName);
        body.Order.Status.Should().Be(EnumOrderStatus.Draft);
        body.Order.ItemCount.Should().Be(0);

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentOrderEntity? persisted = await db.ContentOrders.FindAsync(body.Order.Id);
        persisted.Should().NotBeNull();
        persisted!.CustomerId.Should().Be(customer.Id);
        persisted.Status.Should().Be(EnumOrderStatus.Draft);
    }

    [Fact]
    public async Task CreateOrder_AsAdmin_WithValidData_ReturnsCreated()
    {
        CustomerEntity customer = await SeedAsync<ContentDbContext, CustomerEntity>(ctx =>
        {
            CustomerEntity entity = CustomerFactory.Create();
            ctx.Customers.Add(entity);
            return entity;
        });

        Client.AuthenticateAsAdmin();
        AdminCreateOrderRequest request = new AdminCreateOrderRequestBuilder()
            .WithCustomerId(customer.Id.ToString())
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadAsAsync<AdminCreateOrderResponse>();
        body.Order.Id.Should().NotBeEmpty();
        body.Order.CustomerName.Should().Be(customer.FullName);

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentOrderEntity? persisted = await db.ContentOrders.FindAsync(body.Order.Id);
        persisted.Should().NotBeNull();
        persisted!.CustomerId.Should().Be(customer.Id);
    }

    [Fact]
    public async Task CreateOrder_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        AdminCreateOrderRequest request = new AdminCreateOrderRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrder_WithPackage_FillsItemsAndTiersFromTheSlots()
    {
        CustomerEntity customer = CustomerFactory.Create();

        // A content type named after an enum member resolves to that kind; any other name
        // falls back to Custom.
        ContentTypeEntity articleType = ContentTypeFactory.Create(EnumCoreContentType.Article.ToString());
        ContentTypeEntity unnamedType = ContentTypeFactory.Create();
        CategoryEntity billedCategory = CategoryFactory.Create(articleType.Id);
        CategoryEntity bonusCategory = CategoryFactory.Create(unnamedType.Id);

        PricingTierEntity firstTier = PricingTierFactory.Create();
        PricingTierEntity secondTier = PricingTierFactory.Create();
        CategoryPricingEntity billedFirstPrice = CategoryPricingFactory.Create(
            billedCategory.Id,
            firstTier.Id,
            TestConstants.CategoryPricing.ValidPriceUsd
        );
        CategoryPricingEntity billedSecondPrice = CategoryPricingFactory.Create(
            billedCategory.Id,
            secondTier.Id,
            TestConstants.CategoryPricing.UpdatedPriceUsd
        );
        CategoryPricingEntity bonusPrice = CategoryPricingFactory.Create(
            bonusCategory.Id,
            firstTier.Id,
            TestConstants.CategoryPricing.ValidPriceUsd
        );

        PackageEntity package = PackageFactory.Create();
        PackageSlotEntity billedSlot = PackageSlotFactory.Create(
            package.Id,
            billedCategory.Id,
            isRequired: true,
            quantity: TestConstants.PackageSlot.AnotherValidQuantity
        );
        PackageSlotEntity bonusSlot = PackageSlotFactory.Create(
            package.Id,
            bonusCategory.Id,
            isRequired: false,
            quantity: TestConstants.PackageSlot.ValidQuantity
        );

        // An open slot carries no category, so the factory skips it entirely.
        PackageSlotEntity openSlot = PackageSlotFactory.CreateOpen(package.Id);

        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.AddRange(articleType, unnamedType);
            ctx.Categories.AddRange(billedCategory, bonusCategory);
            ctx.PricingTiers.AddRange(firstTier, secondTier);
            ctx.CategoryPricing.AddRange(billedFirstPrice, billedSecondPrice, bonusPrice);
            ctx.Packages.Add(package);
            ctx.PackageSlots.AddRange(billedSlot, bonusSlot, openSlot);
        });

        Client.AuthenticateAsSuperAdmin();
        AdminCreateOrderRequest request = new AdminCreateOrderRequestBuilder()
            .WithCustomerId(customer.Id.ToString())
            .WithPackageId(package.Id)
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadAsAsync<AdminCreateOrderResponse>();
        body.Order.ItemCount.Should().Be(3);

        decimal billedItemPrice =
            TestConstants.CategoryPricing.ValidPriceUsd + TestConstants.CategoryPricing.UpdatedPriceUsd;
        body.Order.TotalAmountUsd.Should().Be(billedItemPrice * TestConstants.PackageSlot.AnotherValidQuantity);

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        List<ContentOrderItemEntity> persisted = await db
            .ContentOrderItems.Include(item => item.Tiers)
            .Where(item => item.OrderId == body.Order.Id)
            .ToListAsync();

        persisted.Should().HaveCount(3);

        List<ContentOrderItemEntity> billedItems = [.. persisted.Where(item => !item.IsBonus)];
        billedItems.Should().HaveCount(TestConstants.PackageSlot.AnotherValidQuantity);
        billedItems.Should().OnlyContain(item => item.CategoryId == billedCategory.Id);
        billedItems.Should().OnlyContain(item => item.ContentKind == EnumCoreContentType.Article);
        billedItems.Should().OnlyContain(item => item.Tiers.Count == 2);

        ContentOrderItemEntity bonusItem = persisted.Single(item => item.IsBonus);
        bonusItem.CategoryId.Should().Be(bonusCategory.Id);
        bonusItem.ContentKind.Should().Be(EnumCoreContentType.Custom);
        bonusItem.Tiers.Should().ContainSingle();
        bonusItem.Tiers.Single().PriceSnapshotUsd.Should().Be(TestConstants.CategoryPricing.ValidPriceUsd);
    }

    [Fact]
    public async Task CreateOrder_WithInactivePackage_ReturnsNotFound()
    {
        CustomerEntity customer = CustomerFactory.Create();
        PackageEntity package = PackageFactory.CreateInactive();
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.Packages.Add(package);
        });

        Client.AuthenticateAsSuperAdmin();
        AdminCreateOrderRequest request = new AdminCreateOrderRequestBuilder()
            .WithCustomerId(customer.Id.ToString())
            .WithPackageId(package.Id)
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Package"))
        );
    }

    [Fact]
    public async Task CreateOrder_WithNonExistentCustomer_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminCreateOrderRequest request = new AdminCreateOrderRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Customer"))
        );
    }
}
