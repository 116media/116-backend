using _116.Identity.Application.Shared.Errors;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Domain.ValueObjects;

/// <summary>
/// Defines the permissions available for Visitor role users using PermissionEntity for type safety.
/// </summary>
/// <remarks>
/// These permissions align with the CoreUserRole.Visitor specification and provide
/// type-safe access to permission definitions using the domain entity.
/// </remarks>
public static class VisitorPermissions
{
    /// <summary>
    /// Returns content-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetContent(UserErrors errors) =>
        [
            PermissionEntity.Create(
                Guid.NewGuid(),
                "articles",
                "read",
                "Allows visitors to view and read published articles",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "videos",
                "read",
                "Grants access to watch published video content streaming",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "contents",
                "read",
                "Provides broad access to view all published content",
                errors
            ),
        ];

    /// <summary>
    /// Returns profile-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetProfile(UserErrors errors) =>
        [
            PermissionEntity.Create(
                Guid.NewGuid(),
                "own_profile",
                "read",
                "Enables visitors to view and read their own profile information",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "own_profile",
                "update",
                "Allows visitors to modify their own profile information",
                errors
            ),
        ];

    /// <summary>
    /// Returns like-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetLikes(UserErrors errors) =>
        [
            PermissionEntity.Create(
                Guid.NewGuid(),
                "likes",
                "create",
                "Grants ability to express appreciation by creating likes",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "own_likes",
                "delete",
                "Allows visitors to remove their previously created likes",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "likes",
                "read",
                "Enables viewing like counts and engagement metrics content",
                errors
            ),
        ];

    /// <summary>
    /// Returns comment-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetComments(UserErrors errors) =>
        [
            PermissionEntity.Create(
                Guid.NewGuid(),
                "comments",
                "read",
                "Provides access to view comments and community discussions",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "comments",
                "create",
                "Enables visitors to participate by posting new comments",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "own_comments",
                "update",
                "Allows visitors to edit their own posted comments",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "own_comments",
                "delete",
                "Grants ability to remove their own posted comments",
                errors
            ),
        ];

    /// <summary>
    /// Returns bookmark-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetBookmarks(UserErrors errors) =>
        [
            PermissionEntity.Create(
                Guid.NewGuid(),
                "bookmarks",
                "create",
                "Enables saving interesting content for later reference access",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "own_bookmarks",
                "delete",
                "Allows removing items from personal bookmark collection management",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "own_bookmarks",
                "read",
                "Grants access to view personal saved bookmark collections",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "bookmarks",
                "read",
                "Provides access to view public community bookmark collections",
                errors
            ),
        ];

    /// <summary>
    /// Returns navigation-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetNavigation(UserErrors errors) =>
        [
            PermissionEntity.Create(
                Guid.NewGuid(),
                "tags",
                "read",
                "Enables browsing content tags for topic based navigation",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "categories",
                "read",
                "Provides access to browse organized content category structures",
                errors
            ),
        ];

    /// <summary>
    /// Returns playlist-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetPlaylists(UserErrors errors) =>
        [
            PermissionEntity.Create(
                Guid.NewGuid(),
                "playlists",
                "create",
                "Grants ability to create custom personalized content playlists",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "own_playlists",
                "update",
                "Allows modifying personal playlists including adding removing content",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "own_playlists",
                "delete",
                "Enables removing personal playlists when no longer needed",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "own_playlists",
                "read",
                "Provides access to view personal created playlist collections",
                errors
            ),
        ];

    /// <summary>
    /// Returns advertisement-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetAds(UserErrors errors) =>
        [
            PermissionEntity.Create(
                Guid.NewGuid(),
                "ads_banners",
                "read",
                "Allows viewing banner advertisements throughout the entire platform",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "ads_stories",
                "read",
                "Enables viewing story format advertisements in content feeds",
                errors
            ),
        ];

    /// <summary>
    /// Returns rating-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetRates(UserErrors errors) =>
        [
            PermissionEntity.Create(
                Guid.NewGuid(),
                "rates",
                "create",
                "Grants ability to rate content using evaluation mechanisms",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "rates",
                "read",
                "Provides access to view ratings and community assessment",
                errors
            ),
        ];

    /// <summary>
    /// Returns share-related permissions for visitors.
    /// </summary>
    public static PermissionEntity[] GetShares(UserErrors errors) =>
        [
            PermissionEntity.Create(
                Guid.NewGuid(),
                "shares",
                "create",
                "Enables sharing content through various social media mechanisms",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "shares",
                "read",
                "Provides access to view sharing statistics and metadata",
                errors
            ),
            PermissionEntity.Create(
                Guid.NewGuid(),
                "own_shares",
                "read",
                "Grants access to view personal sharing history statistics",
                errors
            ),
        ];

    /// <summary>
    /// Gets all visitor permissions as a single flattened array of PermissionEntity.
    /// </summary>
    /// <param name="errors">The user errors factory used for entity validation.</param>
    /// <returns>All permissions for the Visitor role as typed entities.</returns>
    public static PermissionEntity[] GetAllPermissions(UserErrors errors)
    {
        return GetContent(errors)
            .Concat(GetProfile(errors))
            .Concat(GetLikes(errors))
            .Concat(GetComments(errors))
            .Concat(GetBookmarks(errors))
            .Concat(GetNavigation(errors))
            .Concat(GetPlaylists(errors))
            .Concat(GetAds(errors))
            .Concat(GetRates(errors))
            .Concat(GetShares(errors))
            .ToArray();
    }
}
