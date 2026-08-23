using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistArticles.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArtistArticles.V1;

/// <summary>
/// Integration tests for the PublicGetArtistArticles endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetArtistArticlesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetArtistArticles_WithUnknownSlug_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Articles("no-such-artist"));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Artist"))
        );
    }

    [Fact]
    public async Task GetArtistArticles_ReturnsOnlyPublishedArticlesTaggedToTheArtist()
    {
        string slug = $"tagged-{Guid.NewGuid():N}";

        (ArticleEntity published, ArticleEntity draft, ArticleEntity untagged) = await SeedAsync<
            ContentDbContext,
            (ArticleEntity, ArticleEntity, ArticleEntity)
        >(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArtistEntity artist = ArtistFactory.CreateWithSlug(slug);

            ArticleEntity published = ArticleFactory.CreatePublished(category.Id);
            ArticleEntity draft = ArticleFactory.Create(category.Id);
            ArticleEntity untagged = ArticleFactory.CreatePublished(category.Id);

            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Artists.Add(artist);
            ctx.Articles.AddRange(published, draft, untagged);
            ctx.ArticleArtists.AddRange(
                ArticleArtistEntity.Create(Guid.NewGuid(), published.Id, artist.Id),
                ArticleArtistEntity.Create(Guid.NewGuid(), draft.Id, artist.Id)
            );

            return (published, draft, untagged);
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Articles(slug));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetArtistArticlesResponse body = await response.ReadAsAsync<PublicGetArtistArticlesResponse>();
        body.Articles.Items.Should().ContainSingle(a => a.Id == published.Id);
        body.Articles.Items.Should().NotContain(a => a.Id == draft.Id);
        body.Articles.Items.Should().NotContain(a => a.Id == untagged.Id);
    }

    [Fact]
    public async Task GetArtistArticles_ArticleTaggedToTwoArtists_AppearsOnBothProfiles()
    {
        string slugA = $"artist-a-{Guid.NewGuid():N}";
        string slugB = $"artist-b-{Guid.NewGuid():N}";

        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArtistEntity artistA = ArtistFactory.CreateWithSlug(slugA);
            ArtistEntity artistB = ArtistFactory.CreateWithSlug(slugB);
            ArticleEntity article = ArticleFactory.CreatePublished(category.Id);

            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Artists.AddRange(artistA, artistB);
            ctx.Articles.Add(article);
            ctx.ArticleArtists.AddRange(
                ArticleArtistEntity.Create(Guid.NewGuid(), article.Id, artistA.Id),
                ArticleArtistEntity.Create(Guid.NewGuid(), article.Id, artistB.Id)
            );

            return article;
        });

        Client.ClearAuthentication();

        foreach (string slug in new[] { slugA, slugB })
        {
            var response = await Client.GetAsync(Routes.Public.Artists.Articles(slug));
            PublicGetArtistArticlesResponse body = await response.ReadAsAsync<PublicGetArtistArticlesResponse>();
            body.Articles.Items.Should().ContainSingle(a => a.Id == article.Id);
        }
    }

    [Fact]
    public async Task GetArtistArticles_ForArtistWithNoArticles_ReturnsEmptyPageNot404()
    {
        string slug = $"no-news-{Guid.NewGuid():N}";

        await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity artist = ArtistFactory.CreateWithSlug(slug);
            ctx.Artists.Add(artist);
            return artist;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Artists.Articles(slug));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetArtistArticlesResponse body = await response.ReadAsAsync<PublicGetArtistArticlesResponse>();
        body.Articles.Items.Should().BeEmpty();
    }
}
