using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCustomers.V1;
using _116.Content.Domain.Entities;
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
        List<CustomerEntity> customers = null!;
        await SeedAsync<ContentDbContext>(ctx =>
        {
            customers = CustomerFactory.CreateMany(3);
            ctx.Customers.AddRange(customers);
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllCustomersResponse>();
        body.Customers.PageIndex.Should().Be(0);
        body.Customers.PageSize.Should().Be(10);
        body.Customers.Count.Should().Be(3);
        body.Customers.Items.Should().Contain(c => c.Id == customers[0].Id);
    }

    [Fact]
    public async Task GetAllCustomers_AsAdmin_ReturnsOk()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}?pageIndex=0&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetAllCustomersResponse>();
        body.Customers.Should().NotBeNull();
    }
}
