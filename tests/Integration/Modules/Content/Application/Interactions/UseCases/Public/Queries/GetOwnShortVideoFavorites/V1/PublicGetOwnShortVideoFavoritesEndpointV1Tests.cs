using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetOwnShortVideoFavorites.V1;

/// <summary>Integration tests for the authenticated short-video favorite collection endpoints.</summary>
[Collection("Database")]
public class PublicGetOwnShortVideoFavoritesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string LikedUrl => $"{ApiRoutes.Public.Shorts}/liked";
    private static string BookmarkedUrl => $"{ApiRoutes.Public.Shorts}/bookmarked";
    private static string SharedUrl => $"{ApiRoutes.Public.Shorts}/shared";

    [Theory]
    [InlineData("liked")]
    [InlineData("bookmarked")]
    [InlineData("shared")]
    public async Task GetCollection_WithoutAuthentication_ReturnsUnauthorized(string collection)
    {
        Client.ClearAuthentication();

        HttpResponseMessage response = await Client.GetAsync($"{ApiRoutes.Public.Shorts}/{collection}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("liked")]
    [InlineData("bookmarked")]
    [InlineData("shared")]
    public async Task GetCollection_WhenEmpty_ReturnsEmptyPage(string collection)
    {
        Client.AuthenticateAsVisitor();

        HttpResponseMessage response = await Client.GetAsync($"{ApiRoutes.Public.Shorts}/{collection}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<UserShortVideoActivityDto> body = await response.ReadAsAsync<
            PaginatedResult<UserShortVideoActivityDto>
        >();
        body.Count.Should().Be(0);
        body.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLikedShortVideos_FiltersOwnershipAndInactiveShorts_AndPaginatesStableTies()
    {
        Guid otherUserId = Guid.NewGuid();
        DateTime tie = DateTime.UtcNow.AddHours(-1);
        Guid firstLikeId = Guid.NewGuid();
        Guid secondLikeId = Guid.NewGuid();
        (ShortVideoEntity expectedFirst, ShortVideoEntity expectedSecond) = await SeedAsync<
            ContentDbContext,
            (ShortVideoEntity, ShortVideoEntity)
        >(ctx =>
        {
            ShortVideoEntity first = ShortVideoFactory.Create();
            ShortVideoEntity second = ShortVideoFactory.Create();
            ShortVideoEntity inactive = ShortVideoFactory.CreateInactive();
            ShortVideoEntity other = ShortVideoFactory.Create();
            ctx.ShortVideos.AddRange(first, second, inactive, other);

            ShortVideoLikeEntity firstLike = ShortVideoLikeEntity.Create(firstLikeId, TestUser.VisitorId, first.Id);
            ShortVideoLikeEntity secondLike = ShortVideoLikeEntity.Create(secondLikeId, TestUser.VisitorId, second.Id);
            ShortVideoLikeEntity inactiveLike = ShortVideoLikeEntity.Create(
                Guid.NewGuid(),
                TestUser.VisitorId,
                inactive.Id
            );
            ctx.ShortVideoLikes.AddRange(
                firstLike,
                secondLike,
                inactiveLike,
                ShortVideoLikeEntity.Create(Guid.NewGuid(), otherUserId, other.Id)
            );
            return first.Id.CompareTo(second.Id) > 0 ? (first, second) : (second, first);
        });
        await using (ContentDbContext context = CreateDbContext<ContentDbContext>())
        {
            List<ShortVideoLikeEntity> likes = await context
                .ShortVideoLikes.Where(x => x.Id == firstLikeId || x.Id == secondLikeId)
                .ToListAsync();
            likes.ForEach(like => like.CreatedAt = tie);
            await context.SaveChangesAsync();
        }
        Client.AuthenticateAsVisitor();

        HttpResponseMessage firstResponse = await Client.GetAsync($"{LikedUrl}?pageIndex=0&pageSize=1");
        HttpResponseMessage secondResponse = await Client.GetAsync($"{LikedUrl}?pageIndex=1&pageSize=1");
        PaginatedResult<UserShortVideoActivityDto> firstPage = await firstResponse.ReadAsAsync<
            PaginatedResult<UserShortVideoActivityDto>
        >();
        PaginatedResult<UserShortVideoActivityDto> secondPage = await secondResponse.ReadAsAsync<
            PaginatedResult<UserShortVideoActivityDto>
        >();

        firstPage.Count.Should().Be(2);
        firstPage.Items.Should().ContainSingle().Which.ShortVideo.Id.Should().Be(expectedFirst.Id);
        firstPage.Items.Single().ShortVideo.IsLiked.Should().BeTrue();
        secondPage.Items.Should().ContainSingle().Which.ShortVideo.Id.Should().Be(expectedSecond.Id);
    }

    [Fact]
    public async Task GetBookmarkedShortVideos_ReturnsActualBookmarkTimestampAndActiveCurrentUserRows()
    {
        DateTime bookmarkedAt = DateTime.UtcNow.AddDays(-3);
        Guid bookmarkId = Guid.NewGuid();
        ShortVideoEntity expected = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity active = ShortVideoFactory.Create();
            ShortVideoEntity inactive = ShortVideoFactory.CreateInactive();
            ShortVideoEntity other = ShortVideoFactory.Create();
            ctx.ShortVideos.AddRange(active, inactive, other);
            ShortVideoBookmarkEntity bookmark = ShortVideoBookmarkEntity.Create(
                bookmarkId,
                TestUser.VisitorId,
                active.Id
            );
            ctx.ShortVideoBookmarks.AddRange(
                bookmark,
                ShortVideoBookmarkEntity.Create(Guid.NewGuid(), TestUser.VisitorId, inactive.Id),
                ShortVideoBookmarkEntity.Create(Guid.NewGuid(), Guid.NewGuid(), other.Id)
            );
            return active;
        });
        await using (ContentDbContext context = CreateDbContext<ContentDbContext>())
        {
            ShortVideoBookmarkEntity bookmark = await context.ShortVideoBookmarks.SingleAsync(x => x.Id == bookmarkId);
            bookmark.CreatedAt = bookmarkedAt;
            await context.SaveChangesAsync();
        }
        Client.AuthenticateAsVisitor();

        HttpResponseMessage response = await Client.GetAsync(BookmarkedUrl);
        PaginatedResult<UserShortVideoActivityDto> body = await response.ReadAsAsync<
            PaginatedResult<UserShortVideoActivityDto>
        >();

        body.Count.Should().Be(1);
        UserShortVideoActivityDto item = body.Items.Should().ContainSingle().Which;
        item.ShortVideo.Id.Should().Be(expected.Id);
        item.ShortVideo.IsBookmarked.Should().BeTrue();
        item.LastInteractedAt.Should().BeCloseTo(bookmarkedAt, TimeSpan.FromMilliseconds(1));
        item.InteractionCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSharedShortVideos_GroupsOwnShares_ExcludesAnonymousOtherUsersAndInactiveShorts()
    {
        DateTime older = DateTime.UtcNow.AddDays(-2);
        DateTime latest = DateTime.UtcNow.AddDays(-1);
        Guid firstShareId = Guid.NewGuid();
        Guid secondShareId = Guid.NewGuid();
        ShortVideoEntity expected = await SeedAsync<ContentDbContext, ShortVideoEntity>(ctx =>
        {
            ShortVideoEntity active = ShortVideoFactory.Create();
            ShortVideoEntity anonymous = ShortVideoFactory.Create();
            ShortVideoEntity other = ShortVideoFactory.Create();
            ShortVideoEntity inactive = ShortVideoFactory.CreateInactive();
            ctx.ShortVideos.AddRange(active, anonymous, other, inactive);
            ShortVideoShareEntity first = ShortVideoShareEntity.Create(firstShareId, TestUser.VisitorId, active.Id);
            ShortVideoShareEntity second = ShortVideoShareEntity.Create(secondShareId, TestUser.VisitorId, active.Id);
            ctx.ShortVideoShares.AddRange(
                first,
                second,
                ShortVideoShareEntity.Create(Guid.NewGuid(), null, anonymous.Id),
                ShortVideoShareEntity.Create(Guid.NewGuid(), Guid.NewGuid(), other.Id),
                ShortVideoShareEntity.Create(Guid.NewGuid(), TestUser.VisitorId, inactive.Id)
            );
            return active;
        });
        await using (ContentDbContext context = CreateDbContext<ContentDbContext>())
        {
            List<ShortVideoShareEntity> shares = await context
                .ShortVideoShares.Where(x => x.Id == firstShareId || x.Id == secondShareId)
                .ToListAsync();
            shares.Single(x => x.Id == firstShareId).CreatedAt = older;
            shares.Single(x => x.Id == secondShareId).CreatedAt = latest;
            await context.SaveChangesAsync();
        }
        Client.AuthenticateAsVisitor();

        HttpResponseMessage response = await Client.GetAsync(SharedUrl);
        PaginatedResult<UserShortVideoActivityDto> body = await response.ReadAsAsync<
            PaginatedResult<UserShortVideoActivityDto>
        >();

        body.Count.Should().Be(1);
        UserShortVideoActivityDto item = body.Items.Should().ContainSingle().Which;
        item.ShortVideo.Id.Should().Be(expected.Id);
        item.InteractionCount.Should().Be(2);
        item.LastInteractedAt.Should().BeCloseTo(latest, TimeSpan.FromMilliseconds(1));
    }
}
