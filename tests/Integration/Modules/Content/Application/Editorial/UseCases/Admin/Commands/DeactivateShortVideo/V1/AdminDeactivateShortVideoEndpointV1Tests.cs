using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeactivateShortVideo.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeactivateShortVideo.V1;

/// <summary>
/// Integration tests for the AdminDeactivateShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeactivateShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string DeactivateUrl(Guid id) =>
        Routes.Admin.Editorial.Action(EditorialRouteConstants.Shorts, id, EditorialRouteConstants.Deactivate);

    private async Task<bool> GetIsActiveAsync(Guid id)
    {
        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ShortVideoEntity? shortVideo = await ctx.ShortVideos.FindAsync(id);
        return shortVideo!.IsActive;
    }

    [Fact]
    public async Task DeactivateShortVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(DeactivateUrl(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeactivateShortVideo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync(DeactivateUrl(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeactivateShortVideo_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync(DeactivateUrl(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeactivateShortVideo_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(DeactivateUrl(Guid.NewGuid()), null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ShortVideo"))
        );
    }

    [Fact]
    public async Task DeactivateShortVideo_AsSuperAdmin_AlreadyInactive_ReturnsConflict()
    {
        ShortVideoEntity shortVideo = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.CreateInactive();
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(DeactivateUrl(shortVideo.Id), null);

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ShortVideoErrorMessage>(m => m.AlreadyInactive())
        );
        (await GetIsActiveAsync(shortVideo.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateShortVideo_AsSuperAdmin_ActiveShortVideo_ReturnsOk()
    {
        ShortVideoEntity shortVideo = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(DeactivateUrl(shortVideo.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminDeactivateShortVideoResponse body = await response.ReadAsAsync<AdminDeactivateShortVideoResponse>();
        body.IsSuccess.Should().BeTrue();

        (await GetIsActiveAsync(shortVideo.Id)).Should().BeFalse();
    }
}
