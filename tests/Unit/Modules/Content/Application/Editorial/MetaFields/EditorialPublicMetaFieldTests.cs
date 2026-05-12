using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticlePromotionFeed;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoPromotionFeed;
using _116.Shared.Application.Metadata;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.MetaFields;

/// <summary>
/// Tests that all Editorial public MetaField static fields are correctly initialized.
/// Accessing each static readonly field triggers its initializer, ensuring full coverage.
/// </summary>
public class EditorialPublicMetaFieldTests
{
    #region Public Article MetaFields

    [Fact]
    public void PublicGetPublishedArticlesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPublishedArticlesMetaField.PublicGetPublishedArticles;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetArticleBySlugMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetArticleBySlugMetaField.PublicGetArticleBySlug;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetPromotedArticlesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPromotedArticlesMetaField.PublicGetPromotedArticles;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetArticlePromotionFeedMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetArticlePromotionFeedMetaField.PublicGetArticlePromotionFeed;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Public Video MetaFields

    [Fact]
    public void PublicGetPublishedVideosMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPublishedVideosMetaField.PublicGetPublishedVideos;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetVideoBySlugMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetVideoBySlugMetaField.PublicGetVideoBySlug;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetPromotedVideosMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPromotedVideosMetaField.PublicGetPromotedVideos;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetVideoPromotionFeedMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetVideoPromotionFeedMetaField.PublicGetVideoPromotionFeed;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Public ShortVideo MetaFields

    [Fact]
    public void PublicGetPublicShortsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPublicShortsMetaField.PublicGetPublicShorts;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetPublicShortBySlugMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPublicShortBySlugMetaField.PublicGetPublicShortBySlug;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Public Lyrics MetaFields

    [Fact]
    public void PublicGetLyricsBySlugMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetLyricsBySlugMetaField.PublicGetLyricsBySlug;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetLyricsByVideoIdMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetLyricsByVideoIdMetaField.PublicGetLyricsByVideoId;
        metadata.Should().NotBeNull();
    }

    #endregion
}
