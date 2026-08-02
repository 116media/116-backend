using _116.Content.Application.Catalog.Constants;
using _116.Content.Application.Commerce.Constants;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Lookup.Constants;
using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Roles.Constants;
using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.User.Constants;
using static _116.Tests.Fixtures.Constants.TestConstants;

namespace _116.Tests.Fixtures.Routes;

/// <summary>
/// Builds sub-resource and action URLs for integration tests from the production
/// route constants, so no test ever concatenates a literal route segment.
/// Base collection URLs come from <see cref="Constants.TestConstants.ApiRoutes" />.
/// </summary>
public static class Routes
{
    /// <summary>
    /// Admin-scoped route builders.
    /// </summary>
    public static class Admin
    {
        /// <summary>
        /// Category sub-resources and actions.
        /// </summary>
        public static class Categories
        {
            public static string Activate(Guid id) =>
                $"{ApiRoutes.Admin.Categories}/{id}/{CatalogRouteConstants.Activate}";

            public static string Deactivate(Guid id) =>
                $"{ApiRoutes.Admin.Categories}/{id}/{CatalogRouteConstants.Deactivate}";

            public static string Pricing(Guid id) =>
                $"{ApiRoutes.Admin.Categories}/{id}/{CatalogRouteConstants.Pricing}";

            public static string PricingItem(Guid id, Guid pricingId) => $"{Pricing(id)}/{pricingId}";
        }

        /// <summary>
        /// Package sub-resources and actions.
        /// </summary>
        public static class Packages
        {
            public static string Activate(Guid id) =>
                $"{ApiRoutes.Admin.Packages}/{id}/{CatalogRouteConstants.Activate}";

            public static string Deactivate(Guid id) =>
                $"{ApiRoutes.Admin.Packages}/{id}/{CatalogRouteConstants.Deactivate}";

            public static string Slots(Guid id) => $"{ApiRoutes.Admin.Packages}/{id}/{CatalogRouteConstants.Slots}";

            public static string Slot(Guid id, Guid slotId) => $"{Slots(id)}/{slotId}";
        }

        /// <summary>
        /// Order sub-resources and actions.
        /// </summary>
        public static class Orders
        {
            public static string Items(Guid orderId) =>
                $"{ApiRoutes.Admin.Orders}/{orderId}/{CommerceRouteConstants.Items}";

            public static string Item(Guid orderId, Guid itemId) => $"{Items(orderId)}/{itemId}";

            public static string ItemTiers(Guid orderId, Guid itemId) =>
                $"{Item(orderId, itemId)}/{CommerceRouteConstants.Tiers}";

            public static string ItemTier(Guid orderId, Guid itemId, Guid tierId) =>
                $"{ItemTiers(orderId, itemId)}/{tierId}";

            public static string Submit(Guid orderId) =>
                $"{ApiRoutes.Admin.Orders}/{orderId}/{CommerceRouteConstants.Submit}";

            public static string Cancel(Guid orderId) =>
                $"{ApiRoutes.Admin.Orders}/{orderId}/{CommerceRouteConstants.Cancel}";

            public static string Payment(Guid orderId) =>
                $"{ApiRoutes.Admin.Orders}/{orderId}/{CommerceRouteConstants.Payment}";

            public static string PaymentProof(Guid orderId) => $"{Payment(orderId)}/{CommerceRouteConstants.Proof}";

            public static string VerifyPayment(Guid orderId) => $"{Payment(orderId)}/{CommerceRouteConstants.Verify}";

            public static string RejectPayment(Guid orderId) => $"{Payment(orderId)}/{CommerceRouteConstants.Reject}";
        }

        /// <summary>
        /// Role sub-resources and actions.
        /// </summary>
        public static class Roles
        {
            public static string Activate(Guid id) => $"{ApiRoutes.Admin.Roles}/{id}/{RoleRouteConstants.Activate}";

            public static string Deactivate(Guid id) => $"{ApiRoutes.Admin.Roles}/{id}/{RoleRouteConstants.Deactivate}";

            public static string Restore(Guid id) => $"{ApiRoutes.Admin.Roles}/{id}/{RoleRouteConstants.Restore}";

            public static string Hard(Guid id) => $"{ApiRoutes.Admin.Roles}/{id}/{RoleRouteConstants.Hard}";

            public static string Permissions(Guid id) =>
                $"{ApiRoutes.Admin.Roles}/{id}/{RoleRouteConstants.Permissions}";

            public static string Permission(Guid id, Guid permissionId) => $"{Permissions(id)}/{permissionId}";
        }

        /// <summary>
        /// Permission actions.
        /// </summary>
        public static class Permissions
        {
            public static string Activate(Guid id) =>
                $"{ApiRoutes.Admin.Permissions}/{id}/{PermissionRouteConstants.Activate}";

            public static string Deactivate(Guid id) =>
                $"{ApiRoutes.Admin.Permissions}/{id}/{PermissionRouteConstants.Deactivate}";

            public static string Restore(Guid id) =>
                $"{ApiRoutes.Admin.Permissions}/{id}/{PermissionRouteConstants.Restore}";

            public static string Hard(Guid id) => $"{ApiRoutes.Admin.Permissions}/{id}/{PermissionRouteConstants.Hard}";
        }

        /// <summary>
        /// Session actions.
        /// </summary>
        public static class Sessions
        {
            public static string Revoke(Guid sessionId) =>
                $"{ApiRoutes.Admin.Sessions}/{SessionRouteConstants.Revoke}/{sessionId}";

            public static string ForceLogout(Guid userId) =>
                $"{ApiRoutes.Admin.Sessions}/{SessionRouteConstants.ForceLogout}/{userId}";

            public static string RefreshToken() => $"{ApiRoutes.Admin.Sessions}/{SessionRouteConstants.RefreshToken}";

            public static string Metrics() => $"{ApiRoutes.Admin.Sessions}/{SessionRouteConstants.Metrics}";

            public static string Export() => $"{ApiRoutes.Admin.Sessions}/{SessionRouteConstants.Export}";

            public static string Cleanup() => $"{ApiRoutes.Admin.Sessions}/{SessionRouteConstants.Cleanup}";
        }

        /// <summary>
        /// Editorial (articles/videos/shorts/lyrics) actions.
        /// </summary>
        public static class Editorial
        {
            public static string Action(string collection, Guid id, string action) =>
                $"{ApiRoutes.Admin.Base}/{collection}/{id}/{action}";

            public static string Submit(string collection, Guid id) =>
                Action(collection, id, EditorialRouteConstants.Submit);

            public static string Approve(string collection, Guid id) =>
                Action(collection, id, EditorialRouteConstants.Approve);

            public static string Publish(string collection, Guid id) =>
                Action(collection, id, EditorialRouteConstants.Publish);

            public static string Reject(string collection, Guid id) =>
                Action(collection, id, EditorialRouteConstants.Reject);

            public static string Archive(string collection, Guid id) =>
                Action(collection, id, EditorialRouteConstants.Archive);

            public static string Seo(string collection, Guid id) => Action(collection, id, EditorialRouteConstants.Seo);

            public static string Tags(string collection, Guid id) =>
                Action(collection, id, EditorialRouteConstants.Tags);

            public static string Thumbnail(string collection, Guid id) =>
                Action(collection, id, EditorialRouteConstants.Thumbnail);

            public static string Metadata(string collection, Guid id) =>
                Action(collection, id, EditorialRouteConstants.Metadata);

            public static string Cover(string collection, Guid id) =>
                Action(collection, id, EditorialRouteConstants.Cover);

            public static string Video(string collection, Guid id) =>
                Action(collection, id, EditorialRouteConstants.Video);

            public static string Images(string collection, Guid id) =>
                Action(collection, id, EditorialRouteConstants.Images);

            public static string Youtube(string collection, Guid id) =>
                Action(collection, id, EditorialRouteConstants.Youtube);

            public static string Shoot(string collection, Guid id) =>
                Action(collection, id, EditorialRouteConstants.Shoot);

            public static string Unpromote(string collection, Guid id) =>
                Action(collection, id, EditorialRouteConstants.Unpromote);

            public static string StreamingLink(string collection, Guid id, string platform) =>
                $"{Action(collection, id, EditorialRouteConstants.StreamingLinks)}/{platform}";
        }

        /// <summary>
        /// Artist profile sub-resources and actions.
        /// </summary>
        public static class Artists
        {
            public static string Avatar(Guid id) => $"{ApiRoutes.Admin.Artists}/{id}/{EditorialRouteConstants.Avatar}";

            public static string VerifyOwner(Guid id) =>
                $"{ApiRoutes.Admin.Artists}/{id}/{EditorialRouteConstants.VerifyOwner}";
        }

        /// <summary>
        /// Album sub-resources and actions.
        /// </summary>
        public static class Albums
        {
            public static string Cover(Guid id) => $"{ApiRoutes.Admin.Albums}/{id}/{EditorialRouteConstants.Cover}";
        }

        /// <summary>
        /// Lookup (content-types/pricing-tiers/promotion-levels/tags) actions.
        /// </summary>
        public static class Lookup
        {
            public static string Activate(string collection, Guid id) =>
                $"{ApiRoutes.Admin.Base}/{collection}/{id}/{LookupRouteConstants.Activate}";

            public static string Deactivate(string collection, Guid id) =>
                $"{ApiRoutes.Admin.Base}/{collection}/{id}/{LookupRouteConstants.Deactivate}";
        }

        /// <summary>
        /// Community lyrics submission moderation and lyrics-revision moderation endpoints.
        /// </summary>
        public static class Lyrics
        {
            public static string Submissions() => $"{ApiRoutes.Admin.Lyrics}/{EditorialRouteConstants.Submissions}";

            public static string Submission(Guid id) => $"{Submissions()}/{id}";

            public static string RejectSubmission(Guid id) => $"{Submission(id)}/{EditorialRouteConstants.Reject}";

            public static string RequestSubmissionRevision(Guid id) =>
                $"{Submission(id)}/{EditorialRouteConstants.RequestRevision}";

            public static string Revision(Guid id) =>
                $"{ApiRoutes.Admin.Lyrics}/{EditorialRouteConstants.Revisions}/{id}";
        }

        /// <summary>
        /// Translation-revision moderation endpoints.
        /// </summary>
        public static class Translations
        {
            public static string Revision(Guid id) =>
                $"{ApiRoutes.Admin.Translations}/{EditorialRouteConstants.Revisions}/{id}";
        }
    }

    /// <summary>
    /// Public-scoped route builders.
    /// </summary>
    public static class Public
    {
        /// <summary>
        /// Public auth action endpoints.
        /// </summary>
        public static class Auth
        {
            public static string Login() => $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.Login}";

            public static string SignUp() => $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.SignUp}";

            public static string ChangePassword() => $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.ChangePassword}";

            public static string SetPassword() => $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.SetPassword}";

            public static string ForgotPassword() => $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.ForgotPassword}";

            public static string ResetPassword() => $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.ResetPassword}";

            public static string ResendOtp() => $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.ResendOtp}";

            public static string VerifyOtp() => $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.VerifyOtp}";

            public static string SignOut() => $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.SignOut}";

            public static string SignOutAll() => $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.SignOutAll}";

            public static string SocialLogin() => $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.SocialLogin}";
        }

        /// <summary>
        /// Authenticated-user ("me") endpoints.
        /// </summary>
        public static class Me
        {
            public static string Roles() => $"{ApiRoutes.Public.Me}/{RoleRouteConstants.Endpoint}";

            public static string Avatar() => $"{ApiRoutes.Public.Me}/{UserRouteConstants.Avatar}";

            public static string Profile() => $"{ApiRoutes.Public.Me}/{UserRouteConstants.Profile}";

            public static string Bookmarks() => $"{ApiRoutes.Public.Me}/{InteractionsRouteConstants.Bookmarks}";

            public static string Playlists() => $"{ApiRoutes.Public.Me}/{InteractionsRouteConstants.Playlists}";
        }

        /// <summary>
        /// Article interaction endpoints.
        /// </summary>
        public static class Articles
        {
            public static string Likes(Guid id) =>
                $"{ApiRoutes.Public.Articles}/{id}/{InteractionsRouteConstants.Likes}";

            public static string Bookmarks(Guid id) =>
                $"{ApiRoutes.Public.Articles}/{id}/{InteractionsRouteConstants.Bookmarks}";

            public static string Comments(Guid id) =>
                $"{ApiRoutes.Public.Articles}/{id}/{InteractionsRouteConstants.Comments}";

            public static string Comment(Guid id, Guid commentId) => $"{Comments(id)}/{commentId}";

            public static string CommentReplies(Guid id, Guid commentId) =>
                $"{Comment(id, commentId)}/{InteractionsRouteConstants.Replies}";

            public static string CommentRepliesList(Guid commentId) =>
                $"{ApiRoutes.Public.Articles}/{InteractionsRouteConstants.Comments}/{commentId}/{InteractionsRouteConstants.Replies}";

            public static string CommentLike(Guid commentId) =>
                $"{ApiRoutes.Public.Articles}/{InteractionsRouteConstants.Comments}/{commentId}/{InteractionsRouteConstants.Likes}";

            public static string Shares(Guid id) =>
                $"{ApiRoutes.Public.Articles}/{id}/{InteractionsRouteConstants.Shares}";

            public static string Popular() => $"{ApiRoutes.Public.Articles}/{EditorialRouteConstants.Popular}";
        }

        /// <summary>
        /// Video interaction endpoints.
        /// </summary>
        public static class Videos
        {
            public static string Rated() => $"{ApiRoutes.Public.Videos}/{InteractionsRouteConstants.Rated}";

            public static string Shared() => $"{ApiRoutes.Public.Videos}/{InteractionsRouteConstants.Shared}";

            public static string Ratings(Guid id) =>
                $"{ApiRoutes.Public.Videos}/{id}/{InteractionsRouteConstants.Ratings}";

            public static string Shares(Guid id) =>
                $"{ApiRoutes.Public.Videos}/{id}/{InteractionsRouteConstants.Shares}";

            public static string Popular() => $"{ApiRoutes.Public.Videos}/{EditorialRouteConstants.Popular}";
        }

        /// <summary>
        /// Short-video interaction endpoints.
        /// </summary>
        public static class Shorts
        {
            public static string Likes(Guid id) => $"{ApiRoutes.Public.Shorts}/{id}/{InteractionsRouteConstants.Likes}";

            public static string Bookmarks(Guid id) =>
                $"{ApiRoutes.Public.Shorts}/{id}/{InteractionsRouteConstants.Bookmarks}";

            public static string Shares(Guid id) =>
                $"{ApiRoutes.Public.Shorts}/{id}/{InteractionsRouteConstants.Shares}";

            public static string Views(Guid id) => $"{ApiRoutes.Public.Shorts}/{id}/{InteractionsRouteConstants.Views}";
        }

        /// <summary>
        /// Lyrics interaction and similar-lyrics endpoints.
        /// </summary>
        public static class Lyrics
        {
            public static string Likes(Guid id) => $"{ApiRoutes.Public.Lyrics}/{id}/{InteractionsRouteConstants.Likes}";

            public static string Shares(Guid id) =>
                $"{ApiRoutes.Public.Lyrics}/{id}/{InteractionsRouteConstants.Shares}";

            public static string Views(Guid id) => $"{ApiRoutes.Public.Lyrics}/{id}/{InteractionsRouteConstants.Views}";

            public static string Similar(Guid id) =>
                $"{ApiRoutes.Public.Lyrics}/{id}/{EditorialRouteConstants.Similar}";

            public static string Translations(Guid id) =>
                $"{ApiRoutes.Public.Lyrics}/{id}/{EditorialRouteConstants.Translations}";
        }

        /// <summary>
        /// Unscoped community lyrics submission and lyrics-text correction-revision endpoints —
        /// deliberately outside both the <c>admin</c> and <c>public</c> route groups, mirroring
        /// the unscoped artist claim route.
        /// </summary>
        public static class LyricsSubmissionsAndRevisions
        {
            public static string Submissions() => $"{ApiRoutes.Lyrics}/{EditorialRouteConstants.Submissions}";

            public static string Revisions(Guid lyricsId) =>
                $"{ApiRoutes.Lyrics}/{lyricsId}/{EditorialRouteConstants.Revisions}";

            public static string RevisionVotes(Guid revisionId) =>
                $"{ApiRoutes.Lyrics}/{EditorialRouteConstants.Revisions}/{revisionId}/{EditorialRouteConstants.Votes}";
        }

        /// <summary>
        /// Unscoped translation-revision review endpoints — deliberately outside both the
        /// <c>admin</c> and <c>public</c> route groups.
        /// </summary>
        public static class Translations
        {
            public static string Revisions(Guid translationId) =>
                $"{ApiRoutes.Translations}/{translationId}/{EditorialRouteConstants.Revisions}";

            public static string RevisionVotes(Guid revisionId) =>
                $"{ApiRoutes.Translations}/{EditorialRouteConstants.Revisions}/{revisionId}/{EditorialRouteConstants.Votes}";
        }

        /// <summary>
        /// Playlist endpoints.
        /// </summary>
        public static class Playlists
        {
            public static string ById(Guid id) => $"{ApiRoutes.Public.Playlists}/{id}";

            public static string Videos(Guid id) =>
                $"{ApiRoutes.Public.Playlists}/{id}/{InteractionsRouteConstants.PlaylistVideos}";

            public static string Video(Guid id, Guid videoId) => $"{Videos(id)}/{videoId}";
        }

        /// <summary>
        /// Artist public profile and claim-request endpoints.
        /// </summary>
        public static class Artists
        {
            public static string BySlug(string slug) => $"{ApiRoutes.Public.Artists}/{slug}";

            /// <summary>
            /// The unscoped claim-request route — <c>POST /api/v1/artists/{id}/claim</c>.
            /// </summary>
            public static string Claim(Guid id) => $"{ApiRoutes.Artists}/{id}/{EditorialRouteConstants.Claim}";
        }
    }
}
