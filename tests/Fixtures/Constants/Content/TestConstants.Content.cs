namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for Content module entity testing.
    /// Mirrors <c>src/Modules/Content/Content/Domain/Constants/ContentConstants.cs</c>.
    /// </summary>
    public static class Content
    {
        /// <summary>
        /// Constants for ContentType entity testing.
        /// </summary>
        public static class ContentType
        {
            public const int NameMaxLength = 30;
            public const string ValidName = "Article";
            public const string AnotherValidName = "Video";
        }

        /// <summary>
        /// Constants for PricingTier entity testing.
        /// </summary>
        public static class PricingTier
        {
            public const int NameMaxLength = 40;
            public const int DescriptionMaxLength = 200;
            public const string ValidName = "base_upload";
            public const string AnotherValidName = "social_boost";
            public const string ValidDescription = "Base upload fee for content.";
        }

        /// <summary>
        /// Constants for PromotionLevel entity testing.
        /// </summary>
        public static class PromotionLevel
        {
            public const int NameMaxLength = 40;
            public const string ValidName = "Featured — 7 days";
            public const string AnotherValidName = "À la Une — 14 days";
            public const int ValidDurationDays = 7;
            public const decimal ValidPriceUsd = 50m;
            public const decimal ZeroPriceUsd = 0m;
        }

        /// <summary>
        /// Constants for Tag entity testing.
        /// </summary>
        public static class Tag
        {
            public const int NameMaxLength = 50;
            public const int SlugMaxLength = 60;
            public const string ValidName = "Fally Ipupa";
            public const string ValidSlug = "fally-ipupa";
            public const string AnotherValidName = "Kinshasa";
            public const string AnotherValidSlug = "kinshasa";
        }

        /// <summary>
        /// Constants for Category entity testing.
        /// </summary>
        public static class Category
        {
            public const int NameMaxLength = 60;
            public const int SlugMaxLength = 80;
            public const int DescriptionMaxLength = 300;
            public const string ValidName = "Artist Profile";
            public const string ValidSlug = "artist-profile";
            public const string AnotherValidName = "116 Le Focus";
            public const string AnotherValidSlug = "116-le-focus";
            public const string ValidDescription = "Artist profile category.";
        }

        /// <summary>
        /// Constants for Customer entity testing.
        /// </summary>
        public static class Customer
        {
            public const int FullNameMaxLength = 100;
            public const int EmailMaxLength = 200;
            public const int PhoneMaxLength = 30;
            public const int CompanyMaxLength = 100;
            public const int NotesMaxLength = 500;
            public const string ValidFullName = "John Doe";
            public const string ValidEmail = "customer@example.com";
            public const string AnotherValidEmail = "other@example.com";
            public const string ValidPhone = "+243812345678";
            public const string ValidCompany = "Acme Music Label";
            public const string ValidNotes = "VIP customer with special pricing.";
        }

        /// <summary>
        /// Constants for Package entity testing.
        /// </summary>
        public static class Package
        {
            public const int NameMaxLength = 100;
            public const int DescriptionMaxLength = 500;
            public const string ValidName = "Artist Starter Pack";
            public const string AnotherValidName = "Premium Bundle";
            public const string ValidDescription = "Includes 1 artist profile and 1 interview.";
        }

        /// <summary>
        /// Constants for PackageSlot entity testing.
        /// </summary>
        public static class PackageSlot
        {
            public const int ValidQuantity = 1;
            public const int AnotherValidQuantity = 2;
        }

        /// <summary>
        /// Constants for CategoryPricing entity testing.
        /// </summary>
        public static class CategoryPricing
        {
            public const decimal ValidPriceUsd = 25m;
            public const decimal ZeroPriceUsd = 0m;
            public const decimal UpdatedPriceUsd = 50m;
        }

        /// <summary>
        /// Constants for Commerce entity testing (orders, payments, items, tiers).
        /// </summary>
        public static class Commerce
        {
            public const decimal ValidTierPriceUsd = 100.00m;
            public const decimal ValidPromoPriceUsd = 50.00m;
            public const decimal ValidTotalAmountUsd = 150.00m;
            public const string ValidReceiptUrl = "https://receipts.example.com/pay-123.pdf";
            public const string ValidRejectionNotes = "Payment proof is not legible, please resubmit.";
        }

        /// <summary>
        /// Constants for Editorial entity testing (Article, Video, ShortVideo, Lyrics).
        /// </summary>
        public static class Editorial
        {
            /// <summary>
            /// Constants for Article entity testing.
            /// </summary>
            public static class Article
            {
                public const int TitleMaxLength = 100;
                public const int SlugMaxLength = 220;
                public const int HeadlineMinLength = 100;
                public const int HeadlineMaxLength = 500;
                public const int RejectionReasonMaxLength = 500;

                public const string ValidTitle = "Fally Ipupa — Portrait d'un Géant";
                public const string ValidSlug = "fally-ipupa-portrait-dun-geant";
                public const string ValidHeadline =
                    "Retour sur la carrière époustouflante de Fally Ipupa, artiste congolais qui a conquis l'Afrique et le monde entier avec son style unique.";
                public const string ValidBody = "<p>Corps de l'article complet ici.</p>";
                public const string ValidRejectionReason = "Le contenu n'est pas conforme aux standards éditoriaux.";
            }

            /// <summary>
            /// Constants for Video entity testing.
            /// </summary>
            public static class Video
            {
                public const int TitleMaxLength = 100;
                public const int SlugMaxLength = 220;
                public const int YoutubeVideoUrlMaxLength = 200;
                public const int RejectionReasonMaxLength = 500;

                public const string ValidTitle = "116 Le Focus — Fally Ipupa";
                public const string ValidSlug = "116-le-focus-fally-ipupa";
                public const string ValidYoutubeVideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
                public const string ValidDescription = "Épisode complet de 116 Le Focus avec Fally Ipupa.";
                public const string ValidRejectionReason = "La qualité vidéo ne répond pas aux critères requis.";
            }

            /// <summary>
            /// Constants for ShortVideo entity testing.
            /// </summary>
            public static class ShortVideo
            {
                public const int TitleMaxLength = 200;
                public const int SlugMaxLength = 220;

                public const string ValidTitle = "Teaser — Fally Ipupa Focus";
                public const string ValidSlug = "teaser-fally-ipupa-focus";
                public const string ValidVideoUrl = "https://res.cloudinary.com/test/video/upload/v1/test-short.mp4";
                public const string ValidVideoStorageKey = "content/shorts/test-short";
            }

            /// <summary>
            /// Constants for Lyrics entity testing.
            /// </summary>
            public static class Lyrics
            {
                public const int SongTitleMaxLength = 200;
                public const int ArtistNameMaxLength = 100;
                public const int LanguageMaxLength = 5;
                public const int SlugMaxLength = 220;
                public const int RejectionReasonMaxLength = 500;

                public const string ValidSongTitle = "Eloko Oyo";
                public const string ValidArtistName = "Fally Ipupa";
                public const string ValidLyricsText =
                    "Eloko oyo na lingi\nMpo na yo nde nazali\nSolola na ngai pamba te\nNa lingi yo koloba.";
                public const string ValidLanguage = "fr";
                public const string ValidSlug = "fally-ipupa-eloko-oyo-lyrics";
                public const string AnotherValidSlug = "fally-ipupa-mabele-lyrics";
                public const string ValidRejectionReason = "Les paroles contiennent des erreurs de transcription.";
            }

            /// <summary>
            /// Constants for Artist entity testing.
            /// </summary>
            public static class Artist
            {
                public const int NameMaxLength = 100;
                public const int SlugMaxLength = 220;

                public const string ValidName = "Fally Ipupa";
                public const string ValidSlug = "fally-ipupa";
                public const string AnotherValidSlug = "koffi-olomide";
                public const string ValidBio = "Congolese singer, songwriter, and dancer.";
            }

            /// <summary>
            /// Constants for Album entity testing.
            /// </summary>
            public static class Album
            {
                public const int NameMaxLength = 200;
                public const int LabelMaxLength = 100;

                public const string ValidName = "Le Grand Kalle Et L'African Jazz";
                public const string ValidLabel = "Fiesta";
                public const short ValidReleaseYear = 1960;
            }

            /// <summary>
            /// Constants for ArticleImage entity testing.
            /// </summary>
            public static class ArticleImage
            {
                public const string ValidStorageKey = "content/articles/test-image-key";
                public const string ValidUrl = "https://res.cloudinary.com/test/image/upload/v1/test-image.jpg";
                public const string AnotherStorageKey = "content/articles/another-image-key";
                public const string AnotherUrl = "https://res.cloudinary.com/test/image/upload/v1/another-image.jpg";
            }

            /// <summary>
            /// Cloudinary test upload result values.
            /// </summary>
            public static class Cloudinary
            {
                public const string ValidPublicId = "content/articles/uploaded-image";
                public const string ValidSecureUrl =
                    "https://res.cloudinary.com/test/image/upload/v1/uploaded-image.jpg";
                public const string ValidFormat = "jpg";
                public const int ValidWidth = 1200;
                public const int ValidHeight = 630;
                public const long ValidBytes = 102400L;
                public const string ValidResourceType = "image";
            }
        }

        /// <summary>
        /// Constants for Interactions entity testing (likes, bookmarks, comments, playlists, ratings, shares).
        /// </summary>
        public static class Interactions
        {
            public const int MaxCommentBodyLength = 1000;
            public const int MaxPlaylistNameLength = 100;
            public const string ValidCommentBody = "This is a valid test comment body.";
            public const string ValidPlaylistName = "My Test Playlist";
            public const short ValidStarRating = 4;
            public const short InvalidStarRatingAboveMax = 6;
            public const short InvalidStarRatingBelowMin = 0;
        }
    }
}
