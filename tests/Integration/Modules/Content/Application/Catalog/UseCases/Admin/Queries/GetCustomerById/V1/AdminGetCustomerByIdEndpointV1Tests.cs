using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCustomerById.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetCustomerById.V1;

/// <summary>
/// Integration tests for the AdminGetCustomerById endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetCustomerByIdEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<CustomerEntity> SeedCustomerAsync()
    {
        return await SeedAsync<ContentDbContext, CustomerEntity>(ctx =>
        {
            CustomerEntity customer = CustomerFactory.Create();
            ctx.Customers.Add(customer);
            return customer;
        });
    }

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
        CustomerEntity customer = await SeedCustomerAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}/{customer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetCustomerById_AsSuperAdmin_WithExistingCustomer_ReturnsOk()
    {
        CustomerEntity customer = await SeedCustomerAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}/{customer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminGetCustomerByIdResponse>();
        body.Customer.Id.Should().Be(customer.Id);
        body.Customer.FullName.Should().Be(customer.FullName);
        body.Customer.Email.Should().Be(customer.Email);
    }

    [Fact]
    public async Task GetCustomerById_WithNonExistentGuid_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Customers}/{Guid.NewGuid()}");

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Customer"))
        );
    }
}
