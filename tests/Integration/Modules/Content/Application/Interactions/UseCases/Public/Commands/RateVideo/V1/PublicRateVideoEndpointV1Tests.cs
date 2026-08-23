using _116.Content.Application.Interactions.UseCases.Public.Commands.RateVideo.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RateVideo.V1;

/// <summary>
/// Integration tests for the PublicRateVideo endpoint.
/// </summary>
[Collection("Database")]
public class PublicRateVideoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<VideoEntity> SeedPublishedVideoAsync()
    {
        return await SeedAsync<ContentDbContext, VideoEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            VideoEntity video = VideoFactory.CreatePublished(category.Id);
            ctx.Videos.Add(video);
            return video;
        });
    }

    [Fact]
    public async Task RateVideo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        PublicRateVideoRequest request = new PublicRateVideoRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Videos.Ratings(Guid.NewGuid()), request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RateVideo_AsVisitor_NonExistentVideo_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();
        PublicRateVideoRequest request = new PublicRateVideoRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Videos.Ratings(Guid.NewGuid()), request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Video"))
        );
    }

    [Fact]
    public async Task RateVideo_AsVisitor_WithValidData_StoresRating()
    {
        VideoEntity video = await SeedPublishedVideoAsync();
        Client.AuthenticateAsVisitor();
        PublicRateVideoRequest request = new PublicRateVideoRequestBuilder().Build();

        var response = await Client.PostAsJsonAsync(Routes.Public.Videos.Ratings(video.Id), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicRateVideoResponse body = await response.ReadAsAsync<PublicRateVideoResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        VideoRatingEntity? rating = await verifyDb.VideoRatings.FirstOrDefaultAsync(r =>
            r.VideoId == video.Id && r.UserId == TestUser.VisitorId
        );
        rating.Should().NotBeNull();
        rating!.Stars.Should().Be(request.Stars);

        VideoEntity? storedVideo = await verifyDb.Videos.FindAsync(video.Id);
        storedVideo!.RatingCount.Should().Be(1);
        storedVideo.RatingAverage.Should().Be(request.Stars);
    }

    [Fact]
    public async Task RateVideo_AsVisitor_WhenRatingTwice_UpdatesExistingRating()
    {
        VideoEntity video = await SeedPublishedVideoAsync();
        Client.AuthenticateAsVisitor();

        PublicRateVideoRequest firstRequest = new PublicRateVideoRequestBuilder().WithStars(2).Build();
        PublicRateVideoRequest secondRequest = new PublicRateVideoRequestBuilder().WithStars(5).Build();

        await Client.PostAsJsonAsync(Routes.Public.Videos.Ratings(video.Id), firstRequest);

        var response = await Client.PostAsJsonAsync(Routes.Public.Videos.Ratings(video.Id), secondRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.VideoRatings.CountAsync(r => r.VideoId == video.Id && r.UserId == TestUser.VisitorId))
            .Should()
            .Be(1);
        VideoRatingEntity rating = await verifyDb.VideoRatings.FirstAsync(r =>
            r.VideoId == video.Id && r.UserId == TestUser.VisitorId
        );
        rating.Stars.Should().Be(secondRequest.Stars);
    }
}
