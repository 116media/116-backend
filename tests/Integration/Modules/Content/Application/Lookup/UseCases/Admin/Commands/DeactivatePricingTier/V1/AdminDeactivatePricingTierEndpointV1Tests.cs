using _116.Content.Application.Lookup.Constants;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier.V1;

/// <summary>
/// Integration tests for the AdminDeactivatePricingTier endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeactivatePricingTierEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<bool> IsPricingTierActiveAsync(Guid id)
    {
        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        PricingTierEntity? pricingTier = await context.PricingTiers.FindAsync(id);
        return pricingTier!.IsActive;
    }

    [Fact]
    public async Task DeactivatePricingTier_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(
            Routes.Admin.Lookup.Deactivate(LookupRouteConstants.PricingTiers, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeactivatePricingTier_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Lookup.Deactivate(LookupRouteConstants.PricingTiers, Guid.NewGuid()),
            null
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("PricingTier"))
        );
    }

    [Fact]
    public async Task DeactivatePricingTier_AsSuperAdmin_WithValidId_ReturnsOk()
    {
        PricingTierEntity pricingTier = await SeedAsync<ContentDbContext, PricingTierEntity>(ctx =>
        {
            PricingTierEntity entity = PricingTierFactory.Create();
            ctx.PricingTiers.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Lookup.Deactivate(LookupRouteConstants.PricingTiers, pricingTier.Id),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await IsPricingTierActiveAsync(pricingTier.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task DeactivatePricingTier_WhenAlreadyInactive_ReturnsConflict()
    {
        PricingTierEntity pricingTier = await SeedAsync<ContentDbContext, PricingTierEntity>(ctx =>
        {
            PricingTierEntity entity = PricingTierFactory.CreateInactive();
            ctx.PricingTiers.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Lookup.Deactivate(LookupRouteConstants.PricingTiers, pricingTier.Id),
            null
        );

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<PricingTierErrorMessage>(m => m.AlreadyInactive())
        );
        (await IsPricingTierActiveAsync(pricingTier.Id)).Should().BeFalse();
    }
}
