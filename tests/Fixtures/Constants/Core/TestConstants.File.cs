using _116.BuildingBlocks.Constants;

namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for File entity testing. The column length limits alias
    /// <see cref="FileConstants" /> rather than copying it; the rest is test fixture data.
    /// </summary>
    public static class File
    {
        /// <summary>
        /// The production maximum stored file name length, distinct from
        /// <see cref="FileConstants.MaxOriginalFileNameLength" />.
        /// </summary>
        public const int FileNameMaxLength = FileConstants.MaxFileNameLength;

        /// <summary>
        /// The production maximum MIME type length.
        /// </summary>
        public const int MimeTypeMaxLength = FileConstants.MaxMimeTypeLength;

        /// <summary>
        /// The production maximum storage URL length.
        /// </summary>
        public const int StorageUrlMaxLength = FileConstants.MaxStorageUrlLength;

        /// <summary>
        /// Test-owned fixture stored file name.
        /// </summary>
        public const string ValidFileName = "test-file.jpg";

        /// <summary>
        /// Test-owned fixture uploaded file name.
        /// </summary>
        public const string ValidOriginalFileName = "original-test-file.jpg";

        /// <summary>
        /// Test-owned fixture MIME type, one of the production allow-listed image types.
        /// </summary>
        public const string ValidMimeType = "image/jpeg";

        /// <summary>
        /// Test-owned fixture storage URL in the shape Cloudinary returns.
        /// </summary>
        public const string ValidStorageUrl = "https://res.cloudinary.com/test/image/upload/v1234567890/test-file.jpg";

        /// <summary>
        /// Test-owned fixture file size of 100 KB, under every production upload ceiling.
        /// </summary>
        public const long ValidSizeInBytes = 1024 * 100;

        /// <summary>
        /// Test-owned fixture Cloudinary public identifier for an image.
        /// </summary>
        public const string ValidStorageKey = "content/test-images/test-image-id";

        /// <summary>
        /// Test-owned fixture Cloudinary public identifier for a short video.
        /// </summary>
        public const string ValidVideoStorageKey = "content/short-videos/test-video-id";
    }
}
