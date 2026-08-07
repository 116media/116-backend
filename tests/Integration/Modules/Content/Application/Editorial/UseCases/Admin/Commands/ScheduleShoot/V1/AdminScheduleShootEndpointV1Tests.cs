using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot.V1;

/// <summary>
/// Integration tests for the AdminScheduleShoot endpoint.
/// </summary>
[Collection("Database")]
public class AdminScheduleShootEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

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

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Video"))
        );
    }

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

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "ShootingScheduledAt",
                Localized<VideoErrorMessage>(m => m.ShootingScheduledDateMustBeInFuture())
            )
        );
    }
}
