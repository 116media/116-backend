using System.Net.Http.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Queries.GetPendingPaymentOrders.V1;

/// <summary>
/// Integration tests for the AdminGetPendingPaymentOrders endpoint.
/// </summary>
[Collection("Database")]
public class AdminGetPendingPaymentOrdersEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetPendingPaymentOrders_AsSuperAdmin_ReturnsOk()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Orders}/pending-payment");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPendingPaymentOrders_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Admin.Orders}/pending-payment");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
