namespace _116.Tests.Fixtures.Constants;

public static partial class TestConstants
{
    /// <summary>
    /// Constants for File entity testing.
    /// Mirrors <c>src/BuildingBlocks/Constants/FileConstants.cs</c>.
    /// </summary>
    public static class File
    {
        public const int FileNameMaxLength = 255;
        public const int MimeTypeMaxLength = 100;
        public const int StorageUrlMaxLength = 2048;

        public const string ValidFileName = "test-file.jpg";
        public const string ValidOriginalFileName = "original-test-file.jpg";
        public const string ValidMimeType = "image/jpeg";
        public const string ValidStorageUrl = "https://res.cloudinary.com/test/image/upload/v1234567890/test-file.jpg";
        public const long ValidSizeInBytes = 1024 * 100; // 100 KB
    }
}
