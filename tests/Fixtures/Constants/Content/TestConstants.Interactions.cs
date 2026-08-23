using _116.Content.Domain.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Interactions entity testing (likes, bookmarks, comments, playlists,
    /// ratings, shares).
    /// </summary>
    public static class Interactions
    {
        /// <summary>
        /// The production maximum article comment body length.
        /// </summary>
        public const int MaxCommentBodyLength = ContentConstants.MaxCommentBodyLength;

        /// <summary>
        /// The production maximum user playlist name length.
        /// </summary>
        public const int MaxPlaylistNameLength = ContentConstants.MaxPlaylistNameLength;

        /// <summary>
        /// Test-owned fixture comment body.
        /// </summary>
        public const string ValidCommentBody = "This is a valid test comment body.";

        /// <summary>
        /// Test-owned fixture playlist name.
        /// </summary>
        public const string ValidPlaylistName = "My Test Playlist";

        /// <summary>
        /// Test-owned in-range star rating. The 1-5 scale is enforced in the rating entity
        /// and validators rather than declared as constants.
        /// </summary>
        public const short ValidStarRating = 4;

        /// <summary>
        /// Test-owned rating one above the scale, for the upper boundary.
        /// </summary>
        public const short InvalidStarRatingAboveMax = 6;

        /// <summary>
        /// Test-owned rating one below the scale, for the lower boundary.
        /// </summary>
        public const short InvalidStarRatingBelowMin = 0;
    }
}
