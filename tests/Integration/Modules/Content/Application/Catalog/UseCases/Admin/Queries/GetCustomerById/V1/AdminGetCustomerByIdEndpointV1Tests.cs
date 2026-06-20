using System.Text.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetCustomerById.V1;

/// <summary>
/// Integration tests for the AdminGetCustomerById endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetCustomerByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetCustomerById_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomerById_AsVisitor_ReturnsForbidden()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}/{customer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetCustomerById_AsSuperAdmin_WithExistingCustomer_ReturnsOk()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}/{customer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var customerProp = doc.RootElement.GetProperty("customer");

        customerProp.GetProperty("id").GetString().Should().Be(customer.Id.ToString());
    }

    [Fact]
    public async Task GetCustomerById_WithNonExistentGuid_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
