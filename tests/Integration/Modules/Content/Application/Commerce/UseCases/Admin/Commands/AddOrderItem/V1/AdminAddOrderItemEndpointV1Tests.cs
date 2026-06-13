using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
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
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var order = ContentOrderFactory.CreateForCustomer(customer.Id);
        seedContext.Customers.Add(customer);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.ContentOrders.Add(order);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            ContentKind = 0,
            CategoryId = category.Id.ToString(),
            SocialBoost = false,
            IsBonus = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Orders}/{order.Id}/items", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddOrderItem_NonExistentOrder_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new
        {
            ContentKind = 0,
            CategoryId = Guid.NewGuid().ToString(),
            SocialBoost = false,
            IsBonus = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/items", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddOrderItem_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new
        {
            ContentKind = 0,
            CategoryId = Guid.NewGuid().ToString(),
            SocialBoost = false,
            IsBonus = false,
        };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Orders}/{Guid.NewGuid()}/items", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
