using _116.Content.Domain.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for ContentType entity testing.
    /// </summary>
    public static class ContentType
    {
        /// <summary>
        /// The production maximum content type name length.
        /// </summary>
        public const int NameMaxLength = ContentConstants.MaxContentTypeNameLength;

        /// <summary>
        /// Test-owned fixture content type name.
        /// </summary>
        public const string ValidName = "Article";

        /// <summary>
        /// Test-owned second fixture content type name, for tests that need two distinct
        /// content types.
        /// </summary>
        public const string AnotherValidName = "Video";
    }
}
