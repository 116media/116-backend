using _116.Content.Domain.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Lyrics entity testing.
    /// </summary>
    public static class Lyrics
    {
        /// <summary>
        /// The production maximum song title length.
        /// </summary>
        public const int SongTitleMaxLength = ContentConstants.MaxSongTitleLength;

        /// <summary>
        /// The production maximum artist name length, which the lyrics page shares
        /// with the artist profile.
        /// </summary>
        public const int ArtistNameMaxLength = ContentConstants.MaxArtistNameLength;

        /// <summary>
        /// The production maximum lyrics language code length.
        /// </summary>
        public const int LanguageMaxLength = ContentConstants.MaxLyricsLanguageLength;

        /// <summary>
        /// The production maximum editorial slug length, which lyrics share with the
        /// other editorial types.
        /// </summary>
        public const int SlugMaxLength = ContentConstants.MaxSlugLength;

        /// <summary>
        /// The production maximum editorial rejection reason length.
        /// </summary>
        public const int RejectionReasonMaxLength = ContentConstants.MaxRejectionReasonLength;

        /// <summary>
        /// Test-owned fixture song title.
        /// </summary>
        public const string ValidSongTitle = "Eloko Oyo";

        /// <summary>
        /// Test-owned fixture artist name.
        /// </summary>
        public const string ValidArtistName = "Fally Ipupa";

        /// <summary>
        /// Test-owned fixture lyrics body, multi-line so line handling is exercised.
        /// Production sets no length bound on it.
        /// </summary>
        public const string ValidLyricsText =
            "Eloko oyo na lingi\nMpo na yo nde nazali\nSolola na ngai pamba te\nNa lingi yo koloba.";

        /// <summary>
        /// Test-owned fixture language code.
        /// </summary>
        public const string ValidLanguage = "fr";

        /// <summary>
        /// Test-owned fixture lyrics slug.
        /// </summary>
        public const string ValidSlug = "fally-ipupa-eloko-oyo-lyrics";

        /// <summary>
        /// Test-owned second fixture lyrics slug, for uniqueness assertions.
        /// </summary>
        public const string AnotherValidSlug = "fally-ipupa-mabele-lyrics";

        /// <summary>
        /// Test-owned fixture rejection reason.
        /// </summary>
        public const string ValidRejectionReason = "Les paroles contiennent des erreurs de transcription.";
    }
}
