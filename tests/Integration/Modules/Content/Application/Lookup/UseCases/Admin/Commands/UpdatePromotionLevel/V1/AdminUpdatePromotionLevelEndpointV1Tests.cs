using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePromotionLevel.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePromotionLevel.V1;

/// <summary>
/// Integration tests for the AdminUpdatePromotionLevel endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdatePromotionLevelEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdatePromotionLevel_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new AdminUpdatePromotionLevelRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PromotionLevels}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePromotionLevel_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminUpdatePromotionLevelRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PromotionLevels}/{Guid.NewGuid()}", request);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePromotionLevel_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        PromotionLevelEntity promotionLevel = await SeedAsync<ContentDbContext, PromotionLevelEntity>(ctx =>
        {
            PromotionLevelEntity entity = PromotionLevelFactory.Create();
            ctx.PromotionLevels.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new AdminUpdatePromotionLevelRequestBuilder().WithSpotPriority(2).Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PromotionLevels}/{promotionLevel.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminUpdatePromotionLevelResponse>();
        body.PromotionLevel.Id.Should().Be(promotionLevel.Id);
        body.PromotionLevel.Name.Should().Be(request.Name);
        body.PromotionLevel.DurationDays.Should().Be(request.DurationDays);
        body.PromotionLevel.PriceUsd.Should().Be(request.PriceUsd);
        body.PromotionLevel.SpotPriority.Should().Be(request.SpotPriority);

        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        PromotionLevelEntity? persisted = await context.PromotionLevels.FindAsync(promotionLevel.Id);
        persisted!.Name.Should().Be(request.Name);
        persisted.DurationDays.Should().Be(request.DurationDays);
        persisted.PriceUsd.Should().Be(request.PriceUsd);
    }

    /// <summary>
    /// Verifies that updating a promotion level with a name exceeding the maximum allowed length
    /// (40 characters) returns a 400 Bad Request response, exercising the
    /// <c>isRequired=false</c> branch of <c>ValidPromotionLevelName</c> in PromotionLevelValidation.
    /// </summary>
    [Fact]
    public async Task UpdatePromotionLevel_WithNameTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new AdminUpdatePromotionLevelRequestBuilder()
            .WithName(new string('L', TestConstants.Content.PromotionLevel.NameMaxLength + 1))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PromotionLevels}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating a promotion level with a negative price returns a 400 Bad Request
    /// response, exercising the <c>ValidPriceUsd</c> rule in PromotionLevelValidation.
    /// </summary>
    [Fact]
    public async Task UpdatePromotionLevel_WithNegativePrice_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new AdminUpdatePromotionLevelRequestBuilder().WithPriceUsd(-10m).Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PromotionLevels}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating a promotion level with a zero duration returns a 400 Bad Request
    /// response, exercising the <c>ValidDurationDays</c> rule in PromotionLevelValidation.
    /// </summary>
    [Fact]
    public async Task UpdatePromotionLevel_WithZeroDuration_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new AdminUpdatePromotionLevelRequestBuilder().WithDurationDays(0).Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PromotionLevels}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that updating a promotion level with an invalid spot priority (outside 1-3)
    /// returns a 400 Bad Request response, exercising the <c>ValidSpotPriority</c> rule
    /// in PromotionLevelValidation.
    /// </summary>
    [Fact]
    public async Task UpdatePromotionLevel_WithInvalidSpotPriority_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new AdminUpdatePromotionLevelRequestBuilder().WithSpotPriority(5).Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.PromotionLevels}/{id}", request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
