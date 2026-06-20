using System.Text.Json;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Content.Api.Catalog;

/// <summary>
/// Integration tests for the admin customer endpoints verifying create, update,
/// get-all, and get-by-id operations against a real PostgreSQL database
/// through the full API pipeline.
/// </summary>
[Collection("Database")]
public class AdminCustomerEndpointTests(PostgresFixture db) : BaseApiTest(db)
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
