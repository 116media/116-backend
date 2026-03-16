using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedArticles;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedVideos;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShortBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug;
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
    public void PublicGetFeaturedArticlesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetFeaturedArticlesMetaField.PublicGetFeaturedArticles;
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
    public void PublicGetFeaturedVideosMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetFeaturedVideosMetaField.PublicGetFeaturedVideos;
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

    #endregion
}
