using _116.Content.Application.Interactions.UseCases.Admin.Commands.DeleteArticleComment;
using _116.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment;
using _116.Content.Application.Interactions.UseCases.Public.Commands.AddVideoToPlaylist;
using _116.Content.Application.Interactions.UseCases.Public.Commands.BookmarkArticle;
using _116.Content.Application.Interactions.UseCases.Public.Commands.BookmarkShortVideo;
using _116.Content.Application.Interactions.UseCases.Public.Commands.CreatePlaylist;
using _116.Content.Application.Interactions.UseCases.Public.Commands.DeleteArticleComment;
using _116.Content.Application.Interactions.UseCases.Public.Commands.DeletePlaylist;
using _116.Content.Application.Interactions.UseCases.Public.Commands.EditArticleComment;
using _116.Content.Application.Interactions.UseCases.Public.Commands.LikeArticle;
using _116.Content.Application.Interactions.UseCases.Public.Commands.LikeShortVideo;
using _116.Content.Application.Interactions.UseCases.Public.Commands.RateVideo;
using _116.Content.Application.Interactions.UseCases.Public.Commands.RecordShortVideoView;
using _116.Content.Application.Interactions.UseCases.Public.Commands.RemoveVideoFromPlaylist;
using _116.Content.Application.Interactions.UseCases.Public.Commands.RenamePlaylist;
using _116.Content.Application.Interactions.UseCases.Public.Commands.ShareArticle;
using _116.Content.Application.Interactions.UseCases.Public.Commands.ShareShortVideo;
using _116.Content.Application.Interactions.UseCases.Public.Commands.ShareVideo;
using _116.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkArticle;
using _116.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkShortVideo;
using _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeArticle;
using _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeShortVideo;
using _116.Content.Application.Interactions.UseCases.Public.Queries.GetArticleComments;
using _116.Content.Application.Interactions.UseCases.Public.Queries.GetMyArticleBookmarks;
using _116.Content.Application.Interactions.UseCases.Public.Queries.GetMyPlaylists;
using _116.Content.Application.Interactions.UseCases.Public.Queries.GetPlaylistById;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Metadata;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.MetaFields;

/// <summary>
/// Tests that all Interactions MetaField static fields are correctly initialized and validates shared coverage for error messages and DTOs.
/// </summary>
public class InteractionsMetaFieldTests
{
    #region Admin Command MetaFields

    [Fact]
    public void AdminDeleteArticleCommentMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = AdminDeleteArticleCommentMetaField.AdminDeleteArticleComment;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Public Article Command MetaFields

    [Fact]
    public void PublicAddArticleCommentMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicAddArticleCommentMetaField.PublicAddArticleComment;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicBookmarkArticleMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicBookmarkArticleMetaField.PublicBookmarkArticle;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicDeleteArticleCommentMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicDeleteArticleCommentMetaField.PublicDeleteArticleComment;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicEditArticleCommentMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicEditArticleCommentMetaField.PublicEditArticleComment;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicLikeArticleMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicLikeArticleMetaField.PublicLikeArticle;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicShareArticleMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicShareArticleMetaField.PublicShareArticle;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicUnbookmarkArticleMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicUnbookmarkArticleMetaField.PublicUnbookmarkArticle;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicUnlikeArticleMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicUnlikeArticleMetaField.PublicUnlikeArticle;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Public Short Video Command MetaFields

    [Fact]
    public void PublicBookmarkShortVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicBookmarkShortVideoMetaField.PublicBookmarkShortVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicLikeShortVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicLikeShortVideoMetaField.PublicLikeShortVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicRecordShortVideoViewMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicRecordShortVideoViewMetaField.PublicRecordShortVideoView;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicShareShortVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicShareShortVideoMetaField.PublicShareShortVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicUnbookmarkShortVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicUnbookmarkShortVideoMetaField.PublicUnbookmarkShortVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicUnlikeShortVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicUnlikeShortVideoMetaField.PublicUnlikeShortVideo;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Public Video Command MetaFields

    [Fact]
    public void PublicRateVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicRateVideoMetaField.PublicRateVideo;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicShareVideoMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicShareVideoMetaField.PublicShareVideo;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Public Playlist Command MetaFields

    [Fact]
    public void PublicAddVideoToPlaylistMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicAddVideoToPlaylistMetaField.PublicAddVideoToPlaylist;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicCreatePlaylistMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicCreatePlaylistMetaField.PublicCreatePlaylist;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicDeletePlaylistMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicDeletePlaylistMetaField.PublicDeletePlaylist;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicRemoveVideoFromPlaylistMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicRemoveVideoFromPlaylistMetaField.PublicRemoveVideoFromPlaylist;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicRenamePlaylistMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicRenamePlaylistMetaField.PublicRenamePlaylist;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Public Query MetaFields

    [Fact]
    public void PublicGetArticleCommentsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetArticleCommentsMetaField.PublicGetArticleComments;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetMyArticleBookmarksMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetMyArticleBookmarksMetaField.PublicGetMyArticleBookmarks;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetMyPlaylistsMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetMyPlaylistsMetaField.PublicGetMyPlaylists;
        metadata.Should().NotBeNull();
    }

    [Fact]
    public void PublicGetPlaylistByIdMetaField_ShouldBeInitialized()
    {
        RouteMetadata metadata = PublicGetPlaylistByIdMetaField.PublicGetPlaylistById;
        metadata.Should().NotBeNull();
    }

    #endregion

    #region Shared Coverage

    [Fact]
    public void ArticleInteractionErrorMessage_CommentNotFound_ShouldReturnFormattedMessage()
    {
        Guid commentId = Guid.NewGuid();
        string message = ArticleInteractionErrorMessage.CommentNotFound(commentId);
        message.Should().Contain(commentId.ToString());
    }

    [Fact]
    public void PlaylistErrorMessage_NotFound_ShouldReturnFormattedMessage()
    {
        Guid id = Guid.NewGuid();
        string message = PlaylistErrorMessage.NotFound(id);
        message.Should().Contain(id.ToString());
    }

    [Fact]
    public void VideoInPlaylistDto_EqualityAndProperties_ShouldWork()
    {
        Guid videoId = Guid.NewGuid();
        var dto1 = new VideoInPlaylistDto(
            VideoId: videoId,
            Title: "Test Video",
            ThumbnailUrl: null,
            RatingAverage: 4.5m,
            RatingCount: 10,
            SortOrder: 1
        );
        var dto2 = new VideoInPlaylistDto(
            VideoId: videoId,
            Title: "Test Video",
            ThumbnailUrl: null,
            RatingAverage: 4.5m,
            RatingCount: 10,
            SortOrder: 1
        );

        dto1.VideoId.Should().Be(videoId);
        dto1.Title.Should().Be("Test Video");
        dto1.ThumbnailUrl.Should().BeNull();
        dto1.RatingAverage.Should().Be(4.5m);
        dto1.RatingCount.Should().Be(10);
        dto1.SortOrder.Should().Be(1);

        dto1.Should().Be(dto2);
        (dto1 == dto2).Should().BeTrue();
        (dto1 != dto2).Should().BeFalse();
        dto1.GetHashCode().Should().Be(dto2.GetHashCode());

        var (vid, title, thumb, avg, cnt, order) = dto1;
        vid.Should().Be(videoId);
        title.Should().Be("Test Video");
        thumb.Should().BeNull();
        avg.Should().Be(4.5m);
        cnt.Should().Be(10);
        order.Should().Be(1);
    }

    #endregion
}
