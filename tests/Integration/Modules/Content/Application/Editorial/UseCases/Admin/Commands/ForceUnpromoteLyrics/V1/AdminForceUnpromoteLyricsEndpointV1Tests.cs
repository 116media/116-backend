using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteLyrics.V1;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteLyrics.V1;

/// <summary>
/// Integration tests for the AdminForceUnpromoteLyrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminForceUnpromoteLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string Reason = "Government takedown request";

    /// <summary>
    /// Seeds the content type, category and promotion level rows a lyrics page depends on.
    /// </summary>
    /// <returns>The seeded category and promotion level identifiers.</returns>
    private async Task<(Guid CategoryId, Guid PromotionLevelId)> SeedLookupsAsync()
    {
        return await SeedAsync<ContentDbContext, (Guid, Guid)>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            PromotionLevelEntity promotionLevel = PromotionLevelFactory.Create();
            ctx.PromotionLevels.Add(promotionLevel);

            return (category.Id, promotionLevel.Id);
        });
    }

    /// <summary>
    /// Seeds a published lyrics page carrying an active paid promotion.
    /// </summary>
    /// <returns>The seeded promoted lyrics page.</returns>
    private async Task<LyricsEntity> SeedPromotedLyricsAsync()
    {
        (Guid categoryId, Guid promotionLevelId) = await SeedLookupsAsync();

        return await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity lyrics = LyricsFactory.CreatePromoted(categoryId, promotionLevelId);
            ctx.Lyrics.Add(lyrics);
            return lyrics;
        });
    }

    [Fact]
    public async Task ForceUnpromoteLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            Routes.Admin.Editorial.Unpromote(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new AdminForceUnpromoteLyricsRequest(Reason)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// The endpoint is restricted to SuperAdmins, so a plain Admin token is rejected.
    /// </summary>
    [Fact]
    public async Task ForceUnpromoteLyrics_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Routes.Admin.Editorial.Unpromote(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new AdminForceUnpromoteLyricsRequest(Reason)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ForceUnpromoteLyrics_AsSuperAdmin_WithNonExistentId_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            Routes.Admin.Editorial.Unpromote(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new AdminForceUnpromoteLyricsRequest(Reason)
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// An empty reason is rejected by the request validation pipeline before the handler runs,
    /// leaving the promotion untouched.
    /// </summary>
    [Fact]
    public async Task ForceUnpromoteLyrics_WithEmptyReason_ReturnsBadRequestAndKeepsPromotion()
    {
        LyricsEntity lyrics = await SeedPromotedLyricsAsync();
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            Routes.Admin.Editorial.Unpromote(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminForceUnpromoteLyricsRequest(string.Empty)
        );

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FirstOrDefaultAsync(item => item.Id == lyrics.Id);

        persisted.Should().NotBeNull();
        persisted!.IsPromoted.Should().BeTrue();
        persisted.UnpromotedAt.Should().BeNull();
    }

    /// <summary>
    /// A reason longer than <see cref="ContentConstants.MaxRejectionReasonLength" /> is rejected
    /// by the maximum-length rule.
    /// </summary>
    [Fact]
    public async Task ForceUnpromoteLyrics_WithTooLongReason_ReturnsBadRequest()
    {
        LyricsEntity lyrics = await SeedPromotedLyricsAsync();
        Client.AuthenticateAsSuperAdmin();

        string tooLongReason = new('x', ContentConstants.MaxRejectionReasonLength + 1);

        var response = await Client.PostAsJsonAsync(
            Routes.Admin.Editorial.Unpromote(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminForceUnpromoteLyricsRequest(tooLongReason)
        );

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FirstOrDefaultAsync(item => item.Id == lyrics.Id);

        persisted.Should().NotBeNull();
        persisted!.IsPromoted.Should().BeTrue();
    }

    /// <summary>
    /// The success path clears the promotion and stamps the audit trail required for a future
    /// pro-rata refund calculation.
    /// </summary>
    [Fact]
    public async Task ForceUnpromoteLyrics_AsSuperAdmin_WithPromotedPage_ReturnsOkAndStampsAuditTrail()
    {
        LyricsEntity lyrics = await SeedPromotedLyricsAsync();
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            Routes.Admin.Editorial.Unpromote(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminForceUnpromoteLyricsRequest(Reason)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AdminForceUnpromoteLyricsResponse body = await response.ReadAsAsync<AdminForceUnpromoteLyricsResponse>();
        body.LyricsId.Should().Be(lyrics.Id);
        body.UnpromotedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(5));

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FirstOrDefaultAsync(item => item.Id == lyrics.Id);

        persisted.Should().NotBeNull();
        persisted!.IsPromoted.Should().BeFalse();
        persisted.PromotedUntil.Should().BeNull();
        persisted.UnpromotedAt.Should().NotBeNull();
        persisted.UnpromotedBy.Should().Be(TestUser.SuperAdminId.ToString());
        persisted.UnpromotedReason.Should().Be(Reason);
    }

    /// <summary>
    /// A lyrics page that carries no active promotion cannot be unpromoted — the domain guard
    /// surfaces as a 400 and nothing is persisted.
    /// </summary>
    [Fact]
    public async Task ForceUnpromoteLyrics_WhenNotPromoted_ReturnsBadRequest()
    {
        (Guid categoryId, Guid _) = await SeedLookupsAsync();
        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            LyricsEntity entity = LyricsFactory.Create(categoryId);
            ctx.Lyrics.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            Routes.Admin.Editorial.Unpromote(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminForceUnpromoteLyricsRequest(Reason)
        );

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FirstOrDefaultAsync(item => item.Id == lyrics.Id);

        persisted.Should().NotBeNull();
        persisted!.UnpromotedAt.Should().BeNull();
        persisted.UnpromotedBy.Should().BeNull();
        persisted.UnpromotedReason.Should().BeNull();
    }
}
