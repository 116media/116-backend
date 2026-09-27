using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Messages;
using _116.Content.Application.Interactions.UseCases.Public.Commands.ShareShortVideo.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
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

namespace _116.Content.Integration.Tests.Application.Interactions.UseCases.Public.Commands.ShareShortVideo.V1;

/// <summary>
/// Integration tests for the PublicShareShortVideo endpoint.
/// </summary>
[Collection("Database")]
public class PublicShareShortVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<ShortVideoEntity> SeedShortVideoAsync()
    {
        return await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity shortVideo = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(shortVideo);
            return shortVideo;
        });
    }

    [Fact]
    public async Task ShareShortVideo_AsAnonymous_ReturnsOk()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.ClearAuthentication();

        var response = await Client.PostAsync(Routes.Public.Shorts.Shares(shortVideo.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicShareShortVideoResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.ShortVideoShares.AnyAsync(s => s.ShortVideoId == shortVideo.Id && s.UserId == null))
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task ShareShortVideo_AsVisitor_ReturnsOk()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Shorts.Shares(shortVideo.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicShareShortVideoResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (
            await verifyDb.ShortVideoShares.AnyAsync(s =>
                s.ShortVideoId == shortVideo.Id && s.UserId == TestUser.VisitorId
            )
        )
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task ShareShortVideo_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync(Routes.Public.Shorts.Shares(Guid.NewGuid()), null);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ShortVideo"))
        );
    }

    [Fact]
    public async Task ShareShortVideo_WithShareChannel_PersistsChannel()
    {
        ShortVideoEntity shortVideo = await SeedShortVideoAsync();
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Shorts.Shares(shortVideo.Id),
            new { shareChannel = "whatsapp" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        ShortVideoShareEntity share = await verifyDb.ShortVideoShares.SingleAsync(s => s.ShortVideoId == shortVideo.Id);
        share.ShareChannel.Should().Be(EnumShareChannel.WhatsApp);
    }
}
