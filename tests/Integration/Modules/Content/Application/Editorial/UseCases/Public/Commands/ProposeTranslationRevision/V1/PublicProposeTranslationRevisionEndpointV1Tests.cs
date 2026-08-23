using _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeTranslationRevision.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.ProposeTranslationRevision.V1;

/// <summary>
/// Integration tests for the PublicProposeTranslationRevision endpoint.
/// </summary>
[Collection("Database")]
public class PublicProposeTranslationRevisionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task ProposeTranslationRevision_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Translations.Revisions(Guid.NewGuid()),
            new PublicProposeTranslationRevisionRequest("New text", null)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProposeTranslationRevision_AsVisitor_WithEmptyProposedText_ReturnsBadRequest()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Translations.Revisions(Guid.NewGuid()),
            new PublicProposeTranslationRevisionRequest(string.Empty, null)
        );

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail("ProposedText", Localized<TranslationErrorMessage>(m => m.ProposedTextRequired()))
        );
    }

    [Fact]
    public async Task ProposeTranslationRevision_AsVisitor_WithNonExistentTranslation_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Translations.Revisions(Guid.NewGuid()),
            new PublicProposeTranslationRevisionRequest("New text", null)
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("LyricsTranslation"))
        );
    }

    [Fact]
    public async Task ProposeTranslationRevision_HappyPath_CreatesPendingRevision()
    {
        LyricsTranslationEntity translation = await SeedAsync<ContentDbContext, LyricsTranslationEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
            LyricsTranslationEntity translation = LyricsTranslationFactory.Create(lyrics.Id, "es");
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            ctx.LyricsTranslations.Add(translation);
            return translation;
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Translations.Revisions(translation.Id),
            new PublicProposeTranslationRevisionRequest("A better translation", "Fixed a typo")
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicProposeTranslationRevisionResponse>();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsTranslationRevisionEntity? persisted = await ctx.LyricsTranslationRevisions.FindAsync(body.RevisionId);

        persisted.Should().NotBeNull();
        persisted!.TranslationId.Should().Be(translation.Id);
        persisted.ProposedText.Should().Be("A better translation");
        persisted.EditSummary.Should().Be("Fixed a typo");
        persisted.ProposedByUserId.Should().Be(TestUser.VisitorId);
        persisted.Status.Should().Be(EnumRevisionStatus.Pending);
    }
}
