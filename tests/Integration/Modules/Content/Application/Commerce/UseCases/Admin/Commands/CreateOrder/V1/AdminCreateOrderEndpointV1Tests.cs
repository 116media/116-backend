using _116.Content.Infrastructure.Persistence;
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
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { CustomerId = customer.Id.ToString() };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateOrder_AsAdmin_WithValidData_ReturnsCreated()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsAdmin();
        var request = new { CustomerId = customer.Id.ToString() };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateOrder_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { CustomerId = Guid.NewGuid().ToString() };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrder_WithNonExistentCustomer_ReturnsNotFoundOrBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { CustomerId = Guid.NewGuid().ToString() };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Orders, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }
}
