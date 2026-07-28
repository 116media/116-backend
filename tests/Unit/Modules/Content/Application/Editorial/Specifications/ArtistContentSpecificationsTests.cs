using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for <see cref="ArtistHasContentSpecification"/> — the single predicate behind
/// the directory filter, the per-card content count and the profile 404 rule. Evaluated
/// against in-memory queryables, since the rule itself is pure expression logic.
/// </summary>
public class ArtistContentSpecificationsTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    private static bool Evaluate(
        ArtistEntity artist,
        List<LyricsEntity>? lyrics = null,
        List<VideoEntity>? videos = null,
        List<AlbumEntity>? albums = null,
        List<(ArticleArtistEntity Join, ArticleEntity Article)>? taggedArticles = null
    )
    {
        List<ArticleArtistEntity> joins = [];

        if (taggedArticles is not null)
        {
            foreach ((ArticleArtistEntity join, ArticleEntity article) in taggedArticles)
            {
                // The predicate navigates join.Article.Status, so the navigation is wired
                // by hand for the in-memory evaluation.
                typeof(ArticleArtistEntity).GetProperty(nameof(ArticleArtistEntity.Article))!.SetValue(join, article);
                joins.Add(join);
            }
        }

        var specification = new ArtistHasContentSpecification(
            lyrics: (lyrics ?? []).AsQueryable(),
            videos: (videos ?? []).AsQueryable(),
            albums: (albums ?? []).AsQueryable(),
            articleArtists: joins.AsQueryable()
        );

        return specification.ToExpression().Compile()(artist);
    }

    [Fact]
    public void HasContent_WithNothingAnywhere_ShouldBeFalse()
    {
        Evaluate(ArtistFactory.Create()).Should().BeFalse();
    }

    [Fact]
    public void HasContent_WithOnlyAPublishedSong_ShouldBeTrue()
    {
        ArtistEntity artist = ArtistFactory.Create();
        LyricsEntity song = LyricsFactory.CreatePublishedForArtist(CategoryId, artist.Id);

        Evaluate(artist, lyrics: [song]).Should().BeTrue();
    }

    [Fact]
    public void HasContent_WithOnlyADraftSong_ShouldBeFalse()
    {
        // A draft is never content — same rule on every surface.
        ArtistEntity artist = ArtistFactory.Create();
        LyricsEntity draft = LyricsFactory.CreateForArtist(CategoryId, artist.Id);

        Evaluate(artist, lyrics: [draft]).Should().BeFalse();
    }

    [Fact]
    public void HasContent_WithOnlyAPublishedVideo_ShouldBeTrue()
    {
        ArtistEntity artist = ArtistFactory.Create();
        VideoEntity video = VideoFactory.CreatePublishedForArtist(CategoryId, artist.Id);

        Evaluate(artist, videos: [video]).Should().BeTrue();
    }

    [Theory]
    [InlineData(EnumReleaseType.Album)]
    [InlineData(EnumReleaseType.Mixtape)]
    public void HasContent_WithOnlyARenderedReleaseType_ShouldBeTrue(EnumReleaseType releaseType)
    {
        ArtistEntity artist = ArtistFactory.Create();
        AlbumEntity release = AlbumFactory.CreateForArtist(artist.Id, releaseType);

        Evaluate(artist, albums: [release]).Should().BeTrue();
    }

    [Theory]
    [InlineData(EnumReleaseType.EP)]
    [InlineData(EnumReleaseType.Single)]
    public void HasContent_WithOnlyAnUnrenderedReleaseType_ShouldBeFalse(EnumReleaseType releaseType)
    {
        // EP and Single render in no profile section; counting them would list artists
        // whose profiles 404.
        ArtistEntity artist = ArtistFactory.Create();
        AlbumEntity release = AlbumFactory.CreateForArtist(artist.Id, releaseType);

        Evaluate(artist, albums: [release]).Should().BeFalse();
    }

    [Fact]
    public void HasContent_WithOnlyAPublishedTaggedArticle_ShouldBeTrue()
    {
        ArtistEntity artist = ArtistFactory.Create();
        ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
        ArticleArtistEntity join = ArticleArtistEntity.Create(Guid.NewGuid(), article.Id, artist.Id);

        Evaluate(artist, taggedArticles: [(join, article)]).Should().BeTrue();
    }

    [Fact]
    public void HasContent_WithOnlyAJoinToADraftArticle_ShouldBeFalse()
    {
        // A join row pointing at a draft is not content — the predicate looks through to
        // the article's status.
        ArtistEntity artist = ArtistFactory.Create();
        ArticleEntity draft = ArticleFactory.Create(CategoryId);
        ArticleArtistEntity join = ArticleArtistEntity.Create(Guid.NewGuid(), draft.Id, artist.Id);

        Evaluate(artist, taggedArticles: [(join, draft)]).Should().BeFalse();
    }

    [Fact]
    public void HasContent_WithAnotherArtistsContentOnly_ShouldBeFalse()
    {
        // The predicate scopes every surface to the artist under test.
        ArtistEntity artist = ArtistFactory.Create();
        ArtistEntity other = ArtistFactory.Create();
        LyricsEntity othersSong = LyricsFactory.CreatePublishedForArtist(CategoryId, other.Id);
        AlbumEntity othersAlbum = AlbumFactory.CreateForArtist(other.Id, EnumReleaseType.Album);

        Evaluate(artist, lyrics: [othersSong], albums: [othersAlbum]).Should().BeFalse();
    }
}
