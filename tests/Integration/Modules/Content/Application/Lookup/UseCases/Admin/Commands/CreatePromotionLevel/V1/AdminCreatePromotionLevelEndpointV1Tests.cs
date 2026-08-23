using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel.V1;

/// <summary>
/// Integration tests for the AdminCreatePromotionLevel endpoint.
/// </summary>
[Collection("Database")]
public class AdminCreatePromotionLevelEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task CreatePromotionLevel_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new AdminCreatePromotionLevelRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PromotionLevels, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePromotionLevel_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var request = new AdminCreatePromotionLevelRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PromotionLevels, request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePromotionLevel_AsSuperAdmin_WithValidData_ReturnsCreated()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminCreatePromotionLevelRequestBuilder().WithSpotPriority(1).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PromotionLevels, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.ReadAsAsync<AdminCreatePromotionLevelResponse>();
        body.PromotionLevel.Id.Should().NotBeEmpty();
        body.PromotionLevel.Name.Should().Be(request.Name);
        body.PromotionLevel.DurationDays.Should().Be(request.DurationDays);
        body.PromotionLevel.PriceUsd.Should().Be(request.PriceUsd);
        body.PromotionLevel.SpotPriority.Should().Be(request.SpotPriority);
        body.PromotionLevel.IsActive.Should().BeTrue();

        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        PromotionLevelEntity? persisted = await context.PromotionLevels.FindAsync(body.PromotionLevel.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task CreatePromotionLevel_WithDuplicateName_ReturnsConflict()
    {
        await SeedAsync<ContentDbContext, PromotionLevelEntity>(ctx =>
        {
            PromotionLevelEntity existing = PromotionLevelFactory.Create("Featured — 7 days", 7, 50m);
            ctx.PromotionLevels.Add(existing);
            return existing;
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new AdminCreatePromotionLevelRequestBuilder()
            .WithName("Featured — 7 days")
            .WithDurationDays(14)
            .WithPriceUsd(99m)
            .Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PromotionLevels, request);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<PromotionLevelErrorMessage>(m => m.AlreadyExists(request.Name))
        );
    }

    [Fact]
    public async Task CreatePromotionLevel_WithZeroDuration_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminCreatePromotionLevelRequestBuilder().WithDurationDays(0).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PromotionLevels, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("DurationDays", Localized<PromotionLevelErrorMessage>(m => m.DurationMustBePositive()))
        );
    }

    [Fact]
    public async Task CreatePromotionLevel_WithNegativePrice_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminCreatePromotionLevelRequestBuilder().WithPriceUsd(-10m).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PromotionLevels, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("PriceUsd", Localized<PromotionLevelErrorMessage>(m => m.PriceMustBeNonNegative()))
        );
    }

    [Fact]
    public async Task CreatePromotionLevel_WithInvalidSpotPriority_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminCreatePromotionLevelRequestBuilder().WithSpotPriority(4).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PromotionLevels, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("SpotPriority", Localized<PromotionLevelErrorMessage>(m => m.InvalidSpotPriority()))
        );
    }

    [Fact]
    public async Task CreatePromotionLevel_WithEmptyName_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminCreatePromotionLevelRequestBuilder().WithName(string.Empty).Build();

        var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PromotionLevels, request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Name", Localized<PromotionLevelErrorMessage>(m => m.NameRequired()))
        );
    }
}
