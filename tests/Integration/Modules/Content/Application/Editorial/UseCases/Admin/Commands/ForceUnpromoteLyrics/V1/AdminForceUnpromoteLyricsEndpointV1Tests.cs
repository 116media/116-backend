using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteLyrics.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteLyrics.V1;

/// <summary>
/// Integration tests for the AdminForceUnpromoteLyrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminForceUnpromoteLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

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

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Lyrics"))
        );
    }

    [Fact]
    public async Task ForceUnpromoteLyrics_WithEmptyReason_ReturnsBadRequestAndKeepsPromotion()
    {
        LyricsEntity lyrics = await SeedPromotedLyricsAsync();
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PostAsJsonAsync(
            Routes.Admin.Editorial.Unpromote(EditorialRouteConstants.Lyrics, lyrics.Id),
            new AdminForceUnpromoteLyricsRequest(string.Empty)
        );

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("Reason", Localized<LyricsErrorMessage>(m => m.RejectionReasonRequired()))
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FirstOrDefaultAsync(item => item.Id == lyrics.Id);

        persisted.Should().NotBeNull();
        persisted!.IsPromoted.Should().BeTrue();
        persisted.UnpromotedAt.Should().BeNull();
    }

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

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "Reason",
                Localized<LyricsErrorMessage>(m => m.RejectionReasonTooLong(ContentConstants.MaxRejectionReasonLength))
            )
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FirstOrDefaultAsync(item => item.Id == lyrics.Id);

        persisted.Should().NotBeNull();
        persisted!.IsPromoted.Should().BeTrue();
    }

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

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<LyricsErrorMessage>(m => m.NotPromoted())
        );

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FirstOrDefaultAsync(item => item.Id == lyrics.Id);

        persisted.Should().NotBeNull();
        persisted!.UnpromotedAt.Should().BeNull();
        persisted.UnpromotedBy.Should().BeNull();
        persisted.UnpromotedReason.Should().BeNull();
    }
}
