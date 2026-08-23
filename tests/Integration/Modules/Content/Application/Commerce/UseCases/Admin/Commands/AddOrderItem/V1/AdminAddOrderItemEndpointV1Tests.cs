using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.V1;

/// <summary>
/// Integration tests for the AdminAddOrderItem endpoint.
/// </summary>
[Collection("Database")]
public class AdminAddOrderItemEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AddOrderItem_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.ContentOrders.Add(order);
        });

        Client.AuthenticateAsSuperAdmin();
        AdminAddOrderItemRequest request = new AdminAddOrderItemRequestBuilder()
            .WithContentKind(EnumCoreContentType.Article)
            .WithCategoryId(category.Id.ToString())
            .WithSocialBoost(false)
            .WithIsBonus(false)
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Orders.Items(order.Id), request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadAsAsync<AdminAddOrderItemResponse>();
        body.Item.Id.Should().NotBeEmpty();
        body.Item.CategoryId.Should().Be(category.Id);
        body.Item.ContentKind.Should().Be(request.ContentKind);
        body.Item.SocialBoost.Should().Be(request.SocialBoost);
        body.Item.IsBonus.Should().Be(request.IsBonus);

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentOrderItemEntity? persisted = await db.ContentOrderItems.FindAsync(body.Item.Id);
        persisted.Should().NotBeNull();
        persisted!.OrderId.Should().Be(order.Id);
        persisted.CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task AddOrderItem_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminAddOrderItemRequest request = new AdminAddOrderItemRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Orders.Items(Guid.NewGuid()), request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentOrder"))
        );
    }

    [Fact]
    public async Task AddOrderItem_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        AdminAddOrderItemRequest request = new AdminAddOrderItemRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Orders.Items(Guid.NewGuid()), request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddItem_ToSubmittedOrder_ReturnsBadRequest()
    {
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        ContentOrderEntity order = ContentOrderFactory.CreateSubmitted();
        CustomerEntity orderCustomer = CustomerFactory.CreateWithId(order.CustomerId);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(orderCustomer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.ContentOrders.Add(order);
        });

        Client.AuthenticateAsSuperAdmin();
        AdminAddOrderItemRequest request = new AdminAddOrderItemRequestBuilder()
            .WithCategoryId(category.Id.ToString())
            .Build();

        var response = await Client.PostAsJsonAsync(Routes.Admin.Orders.Items(order.Id), request);

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ContentOrderErrorMessage>(m => m.CannotAddItemToNonDraftOrder())
        );
    }
}
