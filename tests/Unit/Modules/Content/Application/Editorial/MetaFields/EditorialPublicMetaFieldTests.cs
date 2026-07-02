using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticlePromotionFeed;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;
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
    #region Query MetaFields

    [Fact]
    public void PublicGetArticleBySlugMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetArticleBySlugMetaField.GetArticleBySlug;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetArticlePromotionFeedMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetArticlePromotionFeedMetaField.GetArticlePromotionFeed;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetLyricsBySlugMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetLyricsBySlugMetaField.GetLyricsBySlug;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetLyricsByVideoIdMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetLyricsByVideoIdMetaField.GetLyricsByVideoId;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetPopularArticlesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPopularArticlesMetaField.GetPopularArticles;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetPopularVideosMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPopularVideosMetaField.GetPopularVideos;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetPromotedArticlesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPromotedArticlesMetaField.GetPromotedArticles;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetPromotedVideosMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPromotedVideosMetaField.GetPromotedVideos;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetPublicShortBySlugMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPublicShortBySlugMetaField.GetPublicShortBySlug;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetPublicShortsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPublicShortsMetaField.GetPublicShorts;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetPublishedArticlesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPublishedArticlesMetaField.GetPublishedArticles;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetPublishedVideosMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPublishedVideosMetaField.GetPublishedVideos;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetVideoBySlugMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetVideoBySlugMetaField.GetVideoBySlug;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetVideoFeedMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetVideoFeedMetaField.GetVideoFeed;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetVideoPromotionFeedMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetVideoPromotionFeedMetaField.GetVideoPromotionFeed;
        metadata.Should().NotBeNull();
    }

    #endregion
}
