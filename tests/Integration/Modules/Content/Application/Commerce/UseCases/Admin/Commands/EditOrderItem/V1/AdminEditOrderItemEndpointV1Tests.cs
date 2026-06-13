using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
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
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        var orderItem = ContentOrderItemFactory.Create(order.Id, category.Id);
        seedContext.Customers.Add(customer);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.ContentOrders.Add(order);
        seedContext.ContentOrderItems.Add(orderItem);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsAdmin();
        var request = new { ContentKind = 1, SocialBoost = true };

        var url = $"{ApiRoutes.Admin.Orders}/{order.Id}/items/{orderItem.Id}";
        var msg = new HttpRequestMessage(HttpMethod.Patch, url) { Content = JsonContent.Create(request) };
        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EditOrderItem_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();
        var request = new { ContentKind = 1 };

        var url = $"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/items/{Guid.NewGuid()}";
        var msg = new HttpRequestMessage(HttpMethod.Patch, url) { Content = JsonContent.Create(request) };
        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EditOrderItem_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { ContentKind = 1 };

        var url = $"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/items/{Guid.NewGuid()}";
        var msg = new HttpRequestMessage(HttpMethod.Patch, url) { Content = JsonContent.Create(request) };
        var response = await Client.SendAsync(msg);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
