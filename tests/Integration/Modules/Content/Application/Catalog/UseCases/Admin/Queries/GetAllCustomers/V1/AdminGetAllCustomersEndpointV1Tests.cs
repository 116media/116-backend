using System.Text.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCustomers.V1;

/// <summary>
/// Integration tests for the AdminGetAllCustomers endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetAllCustomersEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetAllCustomers_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllCustomers_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllCustomers_AsSuperAdmin_ReturnsOkWithItems()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var customers = CustomerFactory.CreateMany(3);
        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.TryGetProperty("customers", out var customersProp).Should().BeTrue();
        customersProp.TryGetProperty("items", out var items).Should().BeTrue();
        items.GetArrayLength().Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetAllCustomers_AsAdmin_ReturnsOk()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
