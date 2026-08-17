using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts.V1;

/// <summary>
/// Integration tests for the PublicGetPublicShorts endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetPublicShortsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetShorts_AsAnonymous_ReturnsOk()
    {
        ShortVideoEntity activeShort = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            return entity;
        });
        ShortVideoEntity inactiveShort = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.CreateInactive();
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Shorts);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPublicShortsResponse body = await response.ReadAsAsync<PublicGetPublicShortsResponse>();
        body.ShortVideos.Items.Should().Contain(item => item.Id == activeShort.Id);
        body.ShortVideos.Items.Should().NotContain(item => item.Id == inactiveShort.Id);
        body.ShortVideos.Items.Should().OnlyContain(item => item.IsActive);
        body.ShortVideos.PageIndex.Should().Be(0);
        body.ShortVideos.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetShorts_WhenVisitorLikedOne_StampsFlagOnlyOnThatItem()
    {
        var userId = await SeedAuthenticatedUserAsync();
        ShortVideoEntity likedShort = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            ctx.ShortVideoLikes.Add(ShortVideoLikeEntity.Create(Guid.NewGuid(), userId, entity.Id));
            return entity;
        });
        ShortVideoEntity otherShort = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            return entity;
        });

        Client.AuthenticateAs(userId, "Visitor");

        var response = await Client.GetAsync(ApiRoutes.Public.Shorts);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetPublicShortsResponse body = await response.ReadAsAsync<PublicGetPublicShortsResponse>();
        body.ShortVideos.Items.Single(item => item.Id == likedShort.Id).IsLiked.Should().BeTrue();
        body.ShortVideos.Items.Single(item => item.Id == otherShort.Id).IsLiked.Should().BeFalse();
    }

    [Fact]
    public async Task GetShorts_WhenAnonymous_ReturnsFalseFlags()
    {
        await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity entity = ShortVideoFactory.Create();
            ctx.ShortVideos.Add(entity);
            ctx.ShortVideoLikes.Add(ShortVideoLikeEntity.Create(Guid.NewGuid(), Guid.NewGuid(), entity.Id));
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(ApiRoutes.Public.Shorts);

        PublicGetPublicShortsResponse body = await response.ReadAsAsync<PublicGetPublicShortsResponse>();
        body.ShortVideos.Items.Should().OnlyContain(item => !item.IsLiked && !item.IsBookmarked);
    }
}
