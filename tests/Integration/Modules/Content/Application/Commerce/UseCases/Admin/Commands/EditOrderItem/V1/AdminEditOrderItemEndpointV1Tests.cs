using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem.V1;

/// <summary>
/// Integration tests for the AdminEditOrderItem endpoint.
/// </summary>
[Collection("Database")]
public class AdminEditOrderItemEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task EditOrderItem_AsAdmin_WithValidData_ReturnsOk()
    {
        CustomerEntity customer = CustomerFactory.Create();
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        ContentOrderEntity order = ContentOrderFactory.CreateForCustomer(customer.Id);
        ContentOrderItemEntity orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        await SeedAsync<ContentDbContext>(ctx =>
        {
            ctx.Customers.Add(customer);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.ContentOrders.Add(order);
            ctx.ContentOrderItems.Add(orderItem);
        });

        Client.AuthenticateAsAdmin();
        var request = new { ContentKind = 1, SocialBoost = true };

        var url = Routes.Admin.Orders.Item(order.Id, orderItem.Id);
        var msg = new HttpRequestMessage(HttpMethod.Patch, url) { Content = JsonContent.Create(request) };
        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminEditOrderItemResponse>();
        body.Item.Id.Should().Be(orderItem.Id);
        body.Item.ContentKind.Should().Be(EnumCoreContentType.Video);
        body.Item.SocialBoost.Should().BeTrue();

        await using ContentDbContext db = CreateDbContext<ContentDbContext>();
        ContentOrderItemEntity? persisted = await db.ContentOrderItems.FindAsync(orderItem.Id);
        persisted!.ContentKind.Should().Be(EnumCoreContentType.Video);
        persisted.SocialBoost.Should().BeTrue();
    }

    [Fact]
    public async Task EditOrderItem_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();
        var request = new { ContentKind = 1 };

        var url = Routes.Admin.Orders.Item(Guid.NewGuid(), Guid.NewGuid());
        var msg = new HttpRequestMessage(HttpMethod.Patch, url) { Content = JsonContent.Create(request) };
        var response = await Client.SendAsync(msg);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentOrder"))
        );
    }

    [Fact]
    public async Task EditOrderItem_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { ContentKind = 1 };

        var url = Routes.Admin.Orders.Item(Guid.NewGuid(), Guid.NewGuid());
        var msg = new HttpRequestMessage(HttpMethod.Patch, url) { Content = JsonContent.Create(request) };
        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
