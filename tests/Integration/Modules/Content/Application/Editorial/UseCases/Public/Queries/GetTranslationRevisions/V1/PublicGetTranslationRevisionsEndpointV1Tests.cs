using _116.Content.Application.Editorial.UseCases.Public.Queries.GetTranslationRevisions;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetTranslationRevisions.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetTranslationRevisions.V1;

/// <summary>
/// Integration tests for the PublicGetTranslationRevisions endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetTranslationRevisionsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetTranslationRevisions_AnonymousWithNonExistentTranslation_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Translations.Revisions(Guid.NewGuid()));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("LyricsTranslation"))
        );
    }

    [Fact]
    public async Task GetTranslationRevisions_HistoryHasEveryStatus_ReturnsAllRevisions()
    {
        (
            LyricsTranslationEntity translation,
            LyricsTranslationRevisionEntity pending,
            LyricsTranslationRevisionEntity autoAccepted,
            LyricsTranslationRevisionEntity acceptedByModerator,
            LyricsTranslationRevisionEntity rejected
        ) = await SeedAsync<
            ContentDbContext,
            (
                LyricsTranslationEntity,
                LyricsTranslationRevisionEntity,
                LyricsTranslationRevisionEntity,
                LyricsTranslationRevisionEntity,
                LyricsTranslationRevisionEntity
            )
        >(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
            LyricsTranslationEntity translation = LyricsTranslationFactory.Create(lyrics.Id, "es");

            LyricsTranslationRevisionEntity pending = LyricsTranslationRevisionFactory.Create(translation.Id);
            LyricsTranslationRevisionEntity autoAccepted = LyricsTranslationRevisionFactory.CreateAutoAccepted(
                translation.Id
            );
            LyricsTranslationRevisionEntity acceptedByModerator =
                LyricsTranslationRevisionFactory.CreateAcceptedByModerator(translation.Id, Guid.NewGuid());
            LyricsTranslationRevisionEntity rejected = LyricsTranslationRevisionFactory.CreateRejected(
                translation.Id,
                Guid.NewGuid()
            );

            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            ctx.LyricsTranslations.Add(translation);
            ctx.LyricsTranslationRevisions.AddRange(pending, autoAccepted, acceptedByModerator, rejected);

            return (translation, pending, autoAccepted, acceptedByModerator, rejected);
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Translations.Revisions(translation.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicGetTranslationRevisionsResponse>();

        body.Revisions.Should().HaveCount(4);
        body.Revisions.Should().Contain(r => r.Id == pending.Id && r.Status == "Pending" && r.DecidedByUserId == null);
        body.Revisions.Should()
            .Contain(r => r.Id == autoAccepted.Id && r.Status == "Accepted" && r.DecidedByUserId == null);
        body.Revisions.Should()
            .Contain(r => r.Id == acceptedByModerator.Id && r.Status == "Accepted" && r.DecidedByUserId != null);
        body.Revisions.Should()
            .Contain(r => r.Id == rejected.Id && r.Status == "Rejected" && r.DecidedByUserId != null);
    }
}
