using System.Text.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCustomer.V1;

/// <summary>
/// Integration tests for the AdminUpdateCustomer endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateCustomerEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdateCustomer_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { FullName = "Updated", Email = "updated@example.com" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Customers}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCustomer_AsVisitor_ReturnsForbidden()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsVisitor();
        var request = new { FullName = "Visitor Update", Email = "visitor-update@example.com" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Customers}/{customer.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCustomer_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var updatedEmail = $"updated-{Guid.NewGuid():N}@example.com";
        var request = new
        {
            FullName = "Updated Name",
            Email = updatedEmail,
            Phone = "+243888000111",
            Company = "Updated Corp",
            Notes = "Updated notes",
        };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Customers}/{customer.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateCustomer_AsAdmin_WithValidData_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsAdmin();
        var updatedEmail = $"admin-upd-{Guid.NewGuid():N}@example.com";
        var request = new { FullName = "Admin Updated", Email = updatedEmail };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Customers}/{customer.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateCustomer_NonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { FullName = "Ghost", Email = "ghost@example.com" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Customers}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCustomer_WithEmptyFullName_ReturnsValidationError()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var customer = CustomerFactory.Create();
        seedContext.Customers.Add(customer);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { FullName = "", Email = "valid@example.com" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Customers}/{customer.Id}", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateCustomer_WithInvalidGuidId_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { FullName = "Valid", Email = "valid@example.com" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Customers}/not-a-guid", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }
}
