using System.Text.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer.V1;

/// <summary>
/// Integration tests for the AdminCreateCustomer endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateCustomerEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateCustomer_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { FullName = "Jane Doe", Email = "jane@example.com" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Customers, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCustomer_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var request = new { FullName = "Jane Doe", Email = "visitor-create@example.com" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Customers, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCustomer_AsAdmin_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsAdmin();
        var email = $"admin-create-{Guid.NewGuid():N}@example.com";
        var request = new
        {
            FullName = "Admin Created",
            Email = email,
            Phone = "+243999000111",
            Company = "Acme Corp",
            Notes = "VIP customer",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Customers, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var context = CreateDbContext<ContentDbContext>();
        var customer = await context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        customer.Should().NotBeNull();
        customer!.FullName.Should().Be("Admin Created");
    }

    [Fact]
    public async Task CreateCustomer_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsSuperAdmin();
        var email = $"super-create-{Guid.NewGuid():N}@example.com";
        var request = new
        {
            FullName = "Super Created",
            Email = email,
            Phone = "+243999000222",
            Company = "Super Corp",
            Notes = "Created by super admin",
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Customers, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var context = CreateDbContext<ContentDbContext>();
        var customer = await context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        customer.Should().NotBeNull();
        customer!.Company.Should().Be("Super Corp");
    }

    [Fact]
    public async Task CreateCustomer_WithEmptyFullName_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { FullName = "", Email = "valid@example.com" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Customers, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateCustomer_WithEmptyEmail_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { FullName = "Valid Name", Email = "" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Customers, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidEmail_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new { FullName = "Valid Name", Email = "not-an-email" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Customers, request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicateEmail_ReturnsConflict()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var existing = CustomerFactory.Create("duplicate@example.com");
        seedContext.Customers.Add(existing);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();
        var request = new { FullName = "Duplicate", Email = "duplicate@example.com" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Customers, request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
