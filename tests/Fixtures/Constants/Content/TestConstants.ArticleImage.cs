namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for ArticleImage entity testing. Storage keys and URLs are per-row
    /// data with no production counterpart.
    /// </summary>
    public static class ArticleImage
    {
        /// <summary>
        /// Test-owned fixture Cloudinary public identifier.
        /// </summary>
        public const string ValidStorageKey = "content/articles/test-image-key";

        /// <summary>
        /// Test-owned fixture Cloudinary URL for the key above.
        /// </summary>
        public const string ValidUrl = "https://res.cloudinary.com/test/image/upload/v1/test-image.jpg";

        /// <summary>
        /// Test-owned second fixture Cloudinary public identifier, for ordering and
        /// uniqueness assertions.
        /// </summary>
        public const string AnotherStorageKey = "content/articles/another-image-key";

        /// <summary>
        /// Test-owned second fixture Cloudinary URL for the key above.
        /// </summary>
        public const string AnotherUrl = "https://res.cloudinary.com/test/image/upload/v1/another-image.jpg";
    }
}
