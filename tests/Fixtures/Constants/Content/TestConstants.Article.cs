using _116.Content.Domain.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Article entity testing.
    /// </summary>
    public static class Article
    {
        /// <summary>
        /// The production maximum editorial title length, which articles share with
        /// videos.
        /// </summary>
        public const int TitleMaxLength = ContentConstants.MaxTitleLength;

        /// <summary>
        /// The production maximum editorial slug length, which articles share with the
        /// other editorial types.
        /// </summary>
        public const int SlugMaxLength = ContentConstants.MaxSlugLength;

        /// <summary>
        /// The production minimum article headline length, enforced on update only.
        /// </summary>
        public const int HeadlineMinLength = ContentConstants.MinHeadlineLength;

        /// <summary>
        /// The production maximum article headline length.
        /// </summary>
        public const int HeadlineMaxLength = ContentConstants.MaxHeadlineLength;

        /// <summary>
        /// The production maximum editorial rejection reason length.
        /// </summary>
        public const int RejectionReasonMaxLength = ContentConstants.MaxRejectionReasonLength;

        /// <summary>
        /// Test-owned fixture article title.
        /// </summary>
        public const string ValidTitle = "Fally Ipupa — Portrait d'un Géant";

        /// <summary>
        /// Test-owned fixture article slug matching <see cref="ValidTitle" />.
        /// </summary>
        public const string ValidSlug = "fally-ipupa-portrait-dun-geant";

        /// <summary>
        /// Test-owned fixture headline, written long enough to clear
        /// <see cref="HeadlineMinLength" />.
        /// </summary>
        public const string ValidHeadline =
            "Retour sur la carrière époustouflante de Fally Ipupa, artiste congolais qui a conquis l'Afrique et le monde entier avec son style unique.";

        /// <summary>
        /// Test-owned fixture article body. Production sets no length bound on it.
        /// </summary>
        public const string ValidBody = "<p>Corps de l'article complet ici.</p>";

        /// <summary>
        /// Test-owned fixture rejection reason.
        /// </summary>
        public const string ValidRejectionReason = "Le contenu n'est pas conforme aux standards éditoriaux.";
    }
}
