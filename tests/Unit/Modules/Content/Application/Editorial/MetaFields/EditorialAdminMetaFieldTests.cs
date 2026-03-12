using _116.Content.Application.Editorial.UseCases.Admin.Commands.ActivateShortVideo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveArticle;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveVideo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveVideo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeId;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeactivateShortVideo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteArticle;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteShortVideo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteVideo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishArticle;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishVideo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitArticle;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleSeo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;
using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllArticles;
using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllLyrics;
using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllShorts;
using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllVideos;
using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById;
using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetShortById;
using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById;
using _116.Shared.Application.Metadata;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.MetaFields;

/// <summary>
/// Tests that all Editorial admin MetaField static fields are correctly initialized.
/// Accessing each static readonly field triggers its initializer, ensuring full coverage.
/// </summary>
public class EditorialAdminMetaFieldTests
{
    #region Article Command MetaFields

    [Fact]
    public void AdminCreateArticleMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminCreateArticleMetaField.AdminCreateArticle;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminSubmitArticleMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminSubmitArticleMetaField.AdminSubmitArticle;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminApproveArticleMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminApproveArticleMetaField.AdminApproveArticle;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminPublishArticleMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminPublishArticleMetaField.AdminPublishArticle;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminRejectArticleMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminRejectArticleMetaField.AdminRejectArticle;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminArchiveArticleMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminArchiveArticleMetaField.AdminArchiveArticle;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminDeleteArticleMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminDeleteArticleMetaField.AdminDeleteArticle;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminUpdateArticleMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUpdateArticleMetaField.AdminUpdateArticle;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminUpdateArticleSeoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUpdateArticleSeoMetaField.AdminUpdateArticleSeo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminUpdateArticleTagsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUpdateArticleTagsMetaField.AdminUpdateArticleTags;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminUploadArticleImageMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUploadArticleImageMetaField.AdminUploadArticleImage;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Article Query MetaFields

    [Fact]
    public void AdminGetAllArticlesMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetAllArticlesMetaField.AdminGetAllArticles;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminGetArticleByIdMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetArticleByIdMetaField.AdminGetArticleById;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Video Command MetaFields

    [Fact]
    public void AdminCreateVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminCreateVideoMetaField.AdminCreateVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminSubmitVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminSubmitVideoMetaField.AdminSubmitVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminApproveVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminApproveVideoMetaField.AdminApproveVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminPublishVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminPublishVideoMetaField.AdminPublishVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminRejectVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminRejectVideoMetaField.AdminRejectVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminArchiveVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminArchiveVideoMetaField.AdminArchiveVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminDeleteVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminDeleteVideoMetaField.AdminDeleteVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminUpdateVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUpdateVideoMetaField.AdminUpdateVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminUpdateVideoSeoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUpdateVideoSeoMetaField.AdminUpdateVideoSeo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminUpdateVideoTagsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUpdateVideoTagsMetaField.AdminUpdateVideoTags;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminAttachYoutubeIdMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminAttachYoutubeIdMetaField.AdminAttachYoutubeId;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminUploadVideoThumbnailMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUploadVideoThumbnailMetaField.AdminUploadVideoThumbnail;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminScheduleShootMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminScheduleShootMetaField.AdminScheduleShoot;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Video Query MetaFields

    [Fact]
    public void AdminGetAllVideosMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetAllVideosMetaField.AdminGetAllVideos;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminGetVideoByIdMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetVideoByIdMetaField.AdminGetVideoById;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region ShortVideo Command MetaFields

    [Fact]
    public void AdminCreateShortVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminCreateShortVideoMetaField.AdminCreateShortVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminActivateShortVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminActivateShortVideoMetaField.AdminActivateShortVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminDeactivateShortVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminDeactivateShortVideoMetaField.AdminDeactivateShortVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminDeleteShortVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminDeleteShortVideoMetaField.AdminDeleteShortVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminUploadShortVideoThumbnailMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUploadShortVideoThumbnailMetaField.AdminUploadShortVideoThumbnail;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region ShortVideo Query MetaFields

    [Fact]
    public void AdminGetAllShortsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetAllShortsMetaField.AdminGetAllShorts;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminGetShortByIdMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetShortByIdMetaField.AdminGetShortById;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Lyrics Command MetaFields

    [Fact]
    public void AdminCreateLyricsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminCreateLyricsMetaField.AdminCreateLyrics;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminUpdateLyricsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUpdateLyricsMetaField.AdminUpdateLyrics;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void AdminUpdateLyricsSeoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminUpdateLyricsSeoMetaField.AdminUpdateLyricsSeo;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Lyrics Query MetaFields

    [Fact]
    public void AdminGetAllLyricsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminGetAllLyricsMetaField.AdminGetAllLyrics;
        metadata.Should().NotBeNull();
    }

    #endregion
}
