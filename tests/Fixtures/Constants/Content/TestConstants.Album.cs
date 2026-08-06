using _116.Content.Domain.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Album entity testing.
    /// </summary>
    public static class Album
    {
        /// <summary>
        /// The production maximum album name length.
        /// </summary>
        public const int NameMaxLength = ContentConstants.MaxAlbumNameLength;

        /// <summary>
        /// The production maximum record label name length.
        /// </summary>
        public const int LabelMaxLength = ContentConstants.MaxLabelNameLength;

        /// <summary>
        /// Test-owned fixture album name.
        /// </summary>
        public const string ValidName = "Le Grand Kalle Et L'African Jazz";

        /// <summary>
        /// Test-owned fixture record label name.
        /// </summary>
        public const string ValidLabel = "Fiesta";

        /// <summary>
        /// Test-owned fixture release year. Production bounds the year at the
        /// application layer rather than declaring a constant.
        /// </summary>
        public const short ValidReleaseYear = 1960;
    }
}
