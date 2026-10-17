using _116.BuildingBlocks.Application.Metadata;
using _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeLyricsRevision;
using _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeTranslationRevision;
using _116.Content.Application.Editorial.UseCases.Public.Commands.RequestArtistClaim;
using _116.Content.Application.Editorial.UseCases.Public.Commands.RequestLyricsTranslation;
using _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticlePromotionFeed;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistArticles;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistReleases;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtists;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsTranslations;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedLyrics;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetShortsFeed;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetSimilarLyrics;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetTranslationRevisions;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoPromotionFeed;
using _116.Content.Application.Editorial.UseCases.Public.Queries.Search;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.MetaFields;

/// <summary>
/// Tests that all Editorial public MetaField static fields are correctly initialized.
/// Accessing each static readonly field triggers its initializer, ensuring full coverage.
/// </summary>
public class EditorialPublicMetaFieldTests
{
    #region Command MetaFields

    [Fact]
    public void PublicProposeLyricsRevisionMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicProposeLyricsRevisionMetaField.ProposeLyricsRevision;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("ProposeLyricsRevision");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicProposeTranslationRevisionMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicProposeTranslationRevisionMetaField.ProposeTranslationRevision;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("ProposeTranslationRevision");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicRequestArtistClaimMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicRequestArtistClaimMetaField.RequestArtistClaim;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("RequestArtistClaim");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicRequestLyricsTranslationMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicRequestLyricsTranslationMetaField.RequestLyricsTranslation;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("RequestLyricsTranslation");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicSubmitLyricsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicSubmitLyricsMetaField.SubmitLyrics;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("CreateLyricsSubmission");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicVoteOnLyricsRevisionMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicVoteOnLyricsRevisionMetaField.VoteOnLyricsRevision;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("VoteOnLyricsRevision");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicVoteOnTranslationRevisionMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicVoteOnTranslationRevisionMetaField.VoteOnTranslationRevision;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("VoteOnTranslationRevision");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Querie MetaFields

    [Fact]
    public void PublicGetArticleBySlugMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetArticleBySlugMetaField.GetArticleBySlug;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetArticleBySlug");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetArticlePromotionFeedMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetArticlePromotionFeedMetaField.GetArticlePromotionFeed;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetArticlePromotionFeed");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetArtistArticlesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetArtistArticlesMetaField.GetArtistArticles;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetArtistArticles");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetArtistBySlugMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetArtistBySlugMetaField.GetArtistBySlug;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetArtistBySlug");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetArtistReleasesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetArtistReleasesMetaField.GetArtistReleases;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetArtistReleases");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetArtistsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetArtistsMetaField.GetArtists;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetArtists");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetLyricsBySlugMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetLyricsBySlugMetaField.GetLyricsBySlug;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetLyricsBySlug");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetLyricsByVideoIdMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetLyricsByVideoIdMetaField.GetLyricsByVideoId;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetLyricsByVideoId");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetLyricsTranslationsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetLyricsTranslationsMetaField.GetLyricsTranslations;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetLyricsTranslations");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetPopularArticlesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPopularArticlesMetaField.GetPopularArticles;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("PublicGetPopularArticles");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetPopularVideosMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPopularVideosMetaField.GetPopularVideos;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetPopularVideos");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetPromotedArticlesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPromotedArticlesMetaField.GetPromotedArticles;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetPromotedArticles");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetPromotedVideosMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPromotedVideosMetaField.GetPromotedVideos;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetPromotedVideos");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetPublicShortBySlugMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPublicShortBySlugMetaField.GetPublicShortBySlug;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetPublicShortBySlug");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetPublicShortsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPublicShortsMetaField.GetPublicShorts;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetPublicShorts");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetPublishedArticlesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPublishedArticlesMetaField.GetPublishedArticles;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetPublishedArticles");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetPublishedLyricsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPublishedLyricsMetaField.GetPublishedLyrics;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetPublishedLyrics");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetPublishedVideosMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPublishedVideosMetaField.GetPublishedVideos;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetPublishedVideos");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetShortsFeedMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetShortsFeedMetaField.GetShortsFeed;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetShortsFeed");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetSimilarLyricsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetSimilarLyricsMetaField.GetSimilarLyrics;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetSimilarLyrics");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetTranslationRevisionsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetTranslationRevisionsMetaField.GetTranslationRevisions;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetTranslationRevisions");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetVideoBySlugMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetVideoBySlugMetaField.GetVideoBySlug;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetVideoBySlug");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetVideoFeedMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetVideoFeedMetaField.GetVideoFeed;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("PublicGetVideoFeed");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicGetVideoPromotionFeedMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetVideoPromotionFeedMetaField.GetVideoPromotionFeed;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("GetVideoPromotionFeed");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void PublicSearchMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicSearchMetaField.Search;

        metadata.Should().NotBeNull();
        metadata.Name.Should().Be("PublicSearch");
        metadata.Summary.Should().NotBeNullOrWhiteSpace();
        metadata.Description.Should().NotBeNullOrWhiteSpace();
    }

    #endregion
}
