using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot.V1;

/// <summary>
/// Integration tests for the AdminScheduleShoot endpoint.
/// </summary>
[Collection("Database")]
public class AdminScheduleShootEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<VideoEntity> SeedVideoAsync()
    {
        return await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            VideoEntity video = VideoFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Videos.Add(video);
            return video;
        });
    }

    [Fact]
    public async Task ScheduleShoot_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Shoot(EditorialRouteConstants.Videos, Guid.NewGuid()),
            new { ShootingScheduledAt = DateTimeOffset.UtcNow.AddDays(7) }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ScheduleShoot_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Shoot(EditorialRouteConstants.Videos, Guid.NewGuid()),
            new { ShootingScheduledAt = DateTimeOffset.UtcNow.AddDays(7) }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ScheduleShoot_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsAdmin();
        AdminScheduleShootRequest request = new AdminScheduleShootRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Shoot(EditorialRouteConstants.Videos, Guid.NewGuid()),
            request
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that scheduling a shoot with a future date on an existing video persists
    /// the scheduled date.
    /// </summary>
    [Fact]
    public async Task ScheduleShoot_AsSuperAdmin_WithFutureDate_ReturnsOkAndPersists()
    {
        VideoEntity video = await SeedVideoAsync();
        Client.AuthenticateAsSuperAdmin();
        DateTimeOffset scheduledAt = DateTimeOffset.UtcNow.AddDays(14);
        AdminScheduleShootRequest request = new AdminScheduleShootRequestBuilder()
            .WithShootingScheduledAt(scheduledAt)
            .Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Shoot(EditorialRouteConstants.Videos, video.Id),
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminScheduleShootResponse body = await response.ReadAsAsync<AdminScheduleShootResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext verifyContext = CreateDbContext<ContentDbContext>();
        VideoEntity? persisted = await verifyContext.Videos.FindAsync(video.Id);
        persisted!.ShootingScheduledAt.Should().NotBeNull();
        persisted.ShootingScheduledAt!.Value.Should().BeCloseTo(request.ShootingScheduledAt, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Verifies that scheduling a shoot with a date in the past returns a 400 Bad Request
    /// response from the validator because <c>ValidShootingScheduledAt</c> requires the
    /// date to be greater than <c>DateTimeOffset.UtcNow</c>.
    /// </summary>
    [Fact]
    public async Task ScheduleShoot_WithDateInPast_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminScheduleShootRequest request = new AdminScheduleShootRequestBuilder()
            .WithShootingScheduledAt(DateTimeOffset.UtcNow.AddDays(-7))
            .Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Shoot(EditorialRouteConstants.Videos, Guid.NewGuid()),
            request
        );

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
