using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Builders.Requests;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Content.Integration.Tests.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier.V1;

/// <summary>
/// Integration tests for the AdminCreatePricingTier endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreatePricingTierEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreatePricingTier_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new AdminCreatePricingTierRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PricingTiers, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePricingTier_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var request = new AdminCreatePricingTierRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PricingTiers, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePricingTier_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminCreatePricingTierRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PricingTiers, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.ReadAsAsync<AdminCreatePricingTierResponse>();
        body.PricingTier.Id.Should().NotBeEmpty();
        body.PricingTier.Name.Should().Be(request.Name);
        body.PricingTier.Description.Should().Be(request.Description);
        body.PricingTier.IsActive.Should().BeTrue();

        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        PricingTierEntity? persisted = await context.PricingTiers.FindAsync(body.PricingTier.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task CreatePricingTier_WithDuplicateName_ReturnsConflict()
    {
        await SeedAsync<ContentDbContext, PricingTierEntity>(ctx =>
        {
            PricingTierEntity existing = PricingTierFactory.Create("base_upload");
            ctx.PricingTiers.Add(existing);
            return existing;
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new AdminCreatePricingTierRequestBuilder().WithName("base_upload").Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PricingTiers, request);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<PricingTierErrorMessage>(m => m.AlreadyExists(request.Name))
        );
    }

    [Fact]
    public async Task CreatePricingTier_WithEmptyName_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminCreatePricingTierRequestBuilder().WithName(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PricingTiers, request);

        await response.ShouldBeValidationProblem("Name", Localized<PricingTierErrorMessage>(m => m.NameRequired()));
    }
}
