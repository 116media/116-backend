using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.TestData.Factories;

namespace _116.Content.Integration.Tests.Application.Interactions.UseCases.Public.Queries.GetOwnVideoFavorites.V1;

/// <summary>
/// HTTP integration tests for the authenticated user's rated and shared video collections.
/// </summary>
[Collection("Database")]
public class PublicGetOwnVideoFavoritesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<(VideoEntity Published, VideoEntity Unpublished)> SeedVideosAsync()
    {
        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        VideoEntity published = VideoFactory.CreatePublished(category.Id);
        VideoEntity unpublished = VideoFactory.Create(category.Id);
        context.Videos.AddRange(published, unpublished);
        await context.SaveChangesAsync();
        return (published, unpublished);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Collection_AnonymousRequest_ReturnsUnauthorized(bool rated)
    {
        Client.ClearAuthentication();

        HttpResponseMessage response = await Client.GetAsync(
            rated ? Routes.Public.Videos.Rated() : Routes.Public.Videos.Shared()
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Rated_ReturnsOnlyCurrentUsersPublishedVideosAndCurrentStars()
    {
        (VideoEntity published, VideoEntity unpublished) = await SeedVideosAsync();
        await using (ContentDbContext context = CreateDbContext<ContentDbContext>())
        {
            context.VideoRatings.AddRange(
                VideoRatingFactory.Create(published.Id, TestUser.VisitorId, 4),
                VideoRatingFactory.Create(unpublished.Id, TestUser.VisitorId, 5),
                VideoRatingFactory.Create(published.Id, Guid.NewGuid(), 2)
            );
            await context.SaveChangesAsync();
        }
        Client.AuthenticateAsVisitor();

        HttpResponseMessage response = await Client.GetAsync(Routes.Public.Videos.Rated());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<UserVideoActivityDto> body = await response.ReadAsAsync<
            PaginatedResult<UserVideoActivityDto>
        >();
        body.Count.Should().Be(1);
        UserVideoActivityDto item = body.Items.Should().ContainSingle().Subject;
        item.Video.Id.Should().Be(published.Id);
        item.RatedStars.Should().Be(4);
        item.InteractionCount.Should().Be(1);
    }

    [Fact]
    public async Task Rated_AfterRerating_ReturnsUpdatedStarsAndActivityTime()
    {
        (VideoEntity published, _) = await SeedVideosAsync();
        await using (ContentDbContext context = CreateDbContext<ContentDbContext>())
        {
            VideoRatingEntity rating = VideoRatingFactory.Create(published.Id, TestUser.VisitorId, 2);
            context.VideoRatings.Add(rating);
            await context.SaveChangesAsync();
            DateTime createdAt = rating.CreatedAt!.Value;
            rating.UpdateStars(5);
            await context.SaveChangesAsync();
            rating.UpdatedAt.Should().BeOnOrAfter(createdAt);
        }
        Client.AuthenticateAsVisitor();

        HttpResponseMessage response = await Client.GetAsync(Routes.Public.Videos.Rated());

        PaginatedResult<UserVideoActivityDto> body = await response.ReadAsAsync<
            PaginatedResult<UserVideoActivityDto>
        >();
        UserVideoActivityDto item = body.Items.Should().ContainSingle().Subject;
        item.RatedStars.Should().Be(5);
        item.LastInteractedAt.Should().BeAfter(DateTimeOffset.MinValue);
    }

    [Fact]
    public async Task Shared_GroupsOnlyCurrentUsersEventsAndExcludesAnonymousAndUnpublished()
    {
        (VideoEntity published, VideoEntity unpublished) = await SeedVideosAsync();
        await using (ContentDbContext context = CreateDbContext<ContentDbContext>())
        {
            context.VideoShares.Add(
                VideoShareFactory.Create(TestUser.VisitorId, published.Id, EnumShareChannel.Facebook)
            );
            await context.SaveChangesAsync();
            context.VideoShares.AddRange(
                VideoShareFactory.Create(TestUser.VisitorId, published.Id, EnumShareChannel.WhatsApp),
                VideoShareFactory.CreateAnonymous(published.Id, EnumShareChannel.X),
                VideoShareFactory.Create(Guid.NewGuid(), published.Id, EnumShareChannel.Clipboard),
                VideoShareFactory.Create(TestUser.VisitorId, unpublished.Id, EnumShareChannel.WebShare)
            );
            await context.SaveChangesAsync();
        }
        Client.AuthenticateAsVisitor();

        HttpResponseMessage response = await Client.GetAsync(Routes.Public.Videos.Shared());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<UserVideoActivityDto> body = await response.ReadAsAsync<
            PaginatedResult<UserVideoActivityDto>
        >();
        body.Count.Should().Be(1);
        UserVideoActivityDto item = body.Items.Should().ContainSingle().Subject;
        item.Video.Id.Should().Be(published.Id);
        item.InteractionCount.Should().Be(2);
        item.LastShareChannel.Should().Be(EnumShareChannel.WhatsApp);
        item.RatedStars.Should().BeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Collection_PaginationReturnsStablePageMetadata(bool rated)
    {
        (VideoEntity first, _) = await SeedVideosAsync();
        (VideoEntity second, _) = await SeedVideosAsync();
        await using (ContentDbContext context = CreateDbContext<ContentDbContext>())
        {
            if (rated)
            {
                context.VideoRatings.AddRange(
                    VideoRatingFactory.Create(first.Id, TestUser.VisitorId, 3),
                    VideoRatingFactory.Create(second.Id, TestUser.VisitorId, 4)
                );
            }
            else
            {
                context.VideoShares.AddRange(
                    VideoShareFactory.Create(TestUser.VisitorId, first.Id),
                    VideoShareFactory.Create(TestUser.VisitorId, second.Id)
                );
            }
            await context.SaveChangesAsync();
        }
        Client.AuthenticateAsVisitor();
        string url = rated ? Routes.Public.Videos.Rated() : Routes.Public.Videos.Shared();

        HttpResponseMessage response = await Client.GetAsync($"{url}?pageIndex=1&pageSize=1");

        PaginatedResult<UserVideoActivityDto> body = await response.ReadAsAsync<
            PaginatedResult<UserVideoActivityDto>
        >();
        body.PageIndex.Should().Be(1);
        body.PageSize.Should().Be(1);
        body.Count.Should().Be(2);
        body.Items.Should().ContainSingle();
    }
}
