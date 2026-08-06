using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage.V1;

/// <summary>
/// Integration tests for the AdminCreatePackage endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreatePackageEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task CreatePackage_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Name = "Unauthorized Pkg", Description = "Should not be created" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Packages, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePackage_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var request = new { Name = "Forbidden Pkg", Description = "Admin cannot create" };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Packages, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePackage_AsSuperAdmin_WithEmptyName_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminCreatePackageRequest request = new AdminCreatePackageRequestBuilder().WithName(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Packages, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Name", Localized<PackageErrorMessage>(m => m.NameRequired()))
        );
    }

    [Fact]
    public async Task CreatePackage_AsSuperAdmin_WithEmptyDescription_ReturnsValidationError()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminCreatePackageRequest request = new AdminCreatePackageRequestBuilder()
            .WithDescription(string.Empty)
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Packages, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Description", Localized<PackageErrorMessage>(m => m.DescriptionRequired()))
        );
    }

    [Fact]
    public async Task CreatePackage_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminCreatePackageRequest request = new AdminCreatePackageRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Packages, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.ReadAsAsync<AdminCreatePackageResponse>();
        body.Package.Id.Should().NotBeEmpty();
        body.Package.Name.Should().Be(request.Name);
        body.Package.Description.Should().Be(request.Description);

        await using var context = CreateDbContext<ContentDbContext>();
        PackageEntity? package = await context.Packages.FirstOrDefaultAsync(p => p.Id == body.Package.Id);
        package.Should().NotBeNull();
        package!.Name.Should().Be(request.Name);
        package.Description.Should().Be(request.Description);
    }
}
