using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCustomer.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCustomer.V1;

/// <summary>
/// Integration tests for the AdminUpdateCustomer endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateCustomerEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

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
        CustomerEntity customer = await SeedCustomerAsync();

        Client.AuthenticateAsVisitor();
        var request = new { FullName = "Visitor Update", Email = "visitor-update@example.com" };

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Customers}/{customer.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCustomer_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        CustomerEntity customer = await SeedCustomerAsync();

        Client.AuthenticateAsSuperAdmin();
        string updatedEmail = $"updated-{Guid.NewGuid():N}@example.com";
        AdminUpdateCustomerRequest request = new AdminUpdateCustomerRequestBuilder().WithEmail(updatedEmail).Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Customers}/{customer.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminUpdateCustomerResponse>();
        body.Customer.Id.Should().Be(customer.Id);
        body.Customer.FullName.Should().Be(request.FullName);
        body.Customer.Email.Should().Be(updatedEmail);
        body.Customer.Company.Should().Be(request.Company);

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        CustomerEntity? updated = await verifyContext.Customers.FindAsync(customer.Id);
        updated.Should().NotBeNull();
        updated!.FullName.Should().Be(request.FullName);
        updated.Email.Should().Be(updatedEmail);
    }

    [Fact]
    public async Task UpdateCustomer_AsAdmin_WithValidData_ReturnsOk()
    {
        CustomerEntity customer = await SeedCustomerAsync();

        Client.AuthenticateAsAdmin();
        string updatedEmail = $"admin-upd-{Guid.NewGuid():N}@example.com";
        AdminUpdateCustomerRequest request = new AdminUpdateCustomerRequestBuilder().WithEmail(updatedEmail).Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Customers}/{customer.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminUpdateCustomerResponse>();
        body.Customer.Id.Should().Be(customer.Id);
        body.Customer.FullName.Should().Be(request.FullName);
        body.Customer.Email.Should().Be(updatedEmail);

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        CustomerEntity? updated = await verifyContext.Customers.FindAsync(customer.Id);
        updated!.Email.Should().Be(updatedEmail);
    }

    [Fact]
    public async Task UpdateCustomer_NonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminUpdateCustomerRequest request = new AdminUpdateCustomerRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Customers}/{Guid.NewGuid()}", request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Customer"))
        );
    }

    [Fact]
    public async Task UpdateCustomer_WithEmptyFullName_ReturnsValidationError()
    {
        CustomerEntity customer = await SeedCustomerAsync();

        Client.AuthenticateAsSuperAdmin();
        AdminUpdateCustomerRequest request = new AdminUpdateCustomerRequestBuilder().WithFullName(string.Empty).Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Customers}/{customer.Id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("FullName", Localized<CustomerErrorMessage>(m => m.FullNameRequired()))
        );
    }

    [Fact]
    public async Task UpdateCustomer_WithInvalidGuidId_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminUpdateCustomerRequest request = new AdminUpdateCustomerRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Customers}/not-a-guid", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Id", Localized<CustomerErrorMessage>(m => m.Localizer["IdInvalid"].Value))
        );
    }
}
