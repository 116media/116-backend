namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Cloudinary test upload result values. All of them are test-owned: they stand in
    /// for what the external service returns, which production never declares.
    /// </summary>
    public static class Cloudinary
    {
        /// <summary>
        /// Test-owned public identifier in a stubbed upload result.
        /// </summary>
        public const string ValidPublicId = "content/articles/uploaded-image";

        /// <summary>
        /// Test-owned secure URL in a stubbed upload result.
        /// </summary>
        public const string ValidSecureUrl = "https://res.cloudinary.com/test/image/upload/v1/uploaded-image.jpg";

        /// <summary>
        /// Test-owned format in a stubbed upload result.
        /// </summary>
        public const string ValidFormat = "jpg";

        /// <summary>
        /// Test-owned pixel width in a stubbed upload result.
        /// </summary>
        public const int ValidWidth = 1200;

        /// <summary>
        /// Test-owned pixel height in a stubbed upload result.
        /// </summary>
        public const int ValidHeight = 630;

        /// <summary>
        /// Test-owned byte size in a stubbed upload result.
        /// </summary>
        public const long ValidBytes = 102400L;

        /// <summary>
        /// Test-owned resource type in a stubbed upload result.
        /// </summary>
        public const string ValidResourceType = "image";
    }
}
