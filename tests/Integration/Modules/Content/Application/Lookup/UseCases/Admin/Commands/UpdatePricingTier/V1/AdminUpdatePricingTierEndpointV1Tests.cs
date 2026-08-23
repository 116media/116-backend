using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier.V1;

/// <summary>
/// Integration tests for the AdminUpdatePricingTier endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdatePricingTierEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task UpdatePricingTier_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new AdminUpdatePricingTierRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PricingTiers}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePricingTier_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminUpdatePricingTierRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PricingTiers}/{Guid.NewGuid()}", request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("PricingTier"))
        );
    }

    [Fact]
    public async Task UpdatePricingTier_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        PricingTierEntity pricingTier = await SeedAsync<ContentDbContext, PricingTierEntity>(ctx =>
        {
            PricingTierEntity entity = PricingTierFactory.Create();
            ctx.PricingTiers.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new AdminUpdatePricingTierRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PricingTiers}/{pricingTier.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminUpdatePricingTierResponse>();
        body.PricingTier.Id.Should().Be(pricingTier.Id);
        body.PricingTier.Name.Should().Be(request.Name);
        body.PricingTier.Description.Should().Be(request.Description);

        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        PricingTierEntity? persisted = await context.PricingTiers.FindAsync(pricingTier.Id);
        persisted!.Name.Should().Be(request.Name);
        persisted.Description.Should().Be(request.Description);
    }

    [Fact]
    public async Task UpdatePricingTier_WithNameTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new AdminUpdatePricingTierRequestBuilder()
            .WithName(new string('P', TestConstants.PricingTier.NameMaxLength + 1))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PricingTiers}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "Name",
                Localized<PricingTierErrorMessage>(m => m.NameTooLong(ContentConstants.MaxPricingTierNameLength))
            )
        );
    }

    [Fact]
    public async Task UpdatePricingTier_WithDescriptionTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new AdminUpdatePricingTierRequestBuilder()
            .WithDescription(new string('D', TestConstants.PricingTier.DescriptionMaxLength + 1))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PricingTiers}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "Description",
                Localized<PricingTierErrorMessage>(m =>
                    m.DescriptionTooLong(ContentConstants.MaxPricingTierDescriptionLength)
                )
            )
        );
    }
}
