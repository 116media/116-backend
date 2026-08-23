using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer.V1;

/// <summary>
/// Integration tests for the AdminCreateCustomer endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreateCustomerEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

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
        string email = $"admin-create-{Guid.NewGuid():N}@example.com";
        AdminCreateCustomerRequest request = new AdminCreateCustomerRequestBuilder().WithEmail(email).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Customers, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.ReadAsAsync<AdminCreateCustomerResponse>();
        body.Customer.Id.Should().NotBeEmpty();
        body.Customer.FullName.Should().Be(request.FullName);
        body.Customer.Email.Should().Be(email);
        body.Customer.Company.Should().Be(request.Company);

        await using var context = CreateDbContext<ContentDbContext>();
        CustomerEntity? customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == body.Customer.Id);
        customer.Should().NotBeNull();
        customer!.FullName.Should().Be(request.FullName);
        customer.Email.Should().Be(email);
    }

    [Fact]
    public async Task CreateCustomer_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsSuperAdmin();
        string email = $"super-create-{Guid.NewGuid():N}@example.com";
        AdminCreateCustomerRequest request = new AdminCreateCustomerRequestBuilder().WithEmail(email).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Customers, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.ReadAsAsync<AdminCreateCustomerResponse>();
        body.Customer.Id.Should().NotBeEmpty();
        body.Customer.Company.Should().Be(request.Company);

        await using var context = CreateDbContext<ContentDbContext>();
        CustomerEntity? customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == body.Customer.Id);
        customer.Should().NotBeNull();
        customer!.Company.Should().Be(request.Company);
    }

    [Fact]
    public async Task CreateCustomer_WithEmptyFullName_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminCreateCustomerRequest request = new AdminCreateCustomerRequestBuilder().WithFullName(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Customers, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("FullName", Localized<CustomerErrorMessage>(m => m.FullNameRequired()))
        );
    }

    [Fact]
    public async Task CreateCustomer_WithEmptyEmail_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminCreateCustomerRequest request = new AdminCreateCustomerRequestBuilder().WithEmail(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Customers, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Email", Localized<CustomerErrorMessage>(m => m.EmailRequired()))
        );
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidEmail_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminCreateCustomerRequest request = new AdminCreateCustomerRequestBuilder().WithEmail("not-an-email").Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Customers, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Email", Localized<CustomerErrorMessage>(m => m.EmailInvalidFormat()))
        );
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicateEmail_ReturnsConflict()
    {
        await SeedAsync<ContentDbContext>(ctx =>
        {
            CustomerEntity existing = CustomerFactory.Create("duplicate@example.com");
            ctx.Customers.Add(existing);
        });

        Client.AuthenticateAsSuperAdmin();
        AdminCreateCustomerRequest request = new AdminCreateCustomerRequestBuilder()
            .WithEmail("duplicate@example.com")
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Customers, request);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<CustomerErrorMessage>(m => m.AlreadyExists("duplicate@example.com"))
        );
    }
}
