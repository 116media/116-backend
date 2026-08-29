using _116.BuildingBlocks.Constants;
using _116.Core.Application.Shared.Errors.Facade;
using _116.Core.Infrastructure.Services;
using _116.Shared.Application.Configurations;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using AwesomeAssertions.Specialized;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="CloudinaryService"/>. The three upload entry points each run their
/// own copy of the size, extension, and content-type checks, so every rule is exercised against
/// every entry point rather than against the image path alone. Accepted inputs are asserted by
/// the upload reaching the Cloudinary SDK and failing there — no credentials are configured — so
/// a <see cref="BadGatewayException"/> means validation passed.
/// </summary>
public class CloudinaryServiceTests
{
    private const string PublicId = "test-id";
    private const long SampleLength = 1024;
    private const long BytesPerMegabyte = 1024 * 1024;

    private readonly Mock<ILogger<CloudinaryService>> _loggerMock = new();
    private readonly CoreI18n _i18n = TestErrorsFactory.CreateCoreI18n();
    private readonly CloudinarySettings _settings = new()
    {
        CloudName = "test-cloud",
        ApiKey = "test-key",
        ApiSecret = "test-secret",
    };

    /// <summary>
    /// The upload entry point a row exercises. Carrying the entry point as a row keeps a new
    /// upload method one row away from full coverage instead of a copy of the surrounding tests.
    /// </summary>
    public enum UploadTarget
    {
        /// <summary>The avatar image upload path.</summary>
        Image,

        /// <summary>The raw document upload path.</summary>
        Raw,

        /// <summary>The video upload path.</summary>
        Video,
    }

    /// <summary>
    /// Creates the service under test against settings that point at no real Cloudinary account.
    /// </summary>
    /// <returns>A service whose validation runs but whose uploads cannot succeed.</returns>
    private CloudinaryService CreateService() => new(_settings, _loggerMock.Object, _i18n);

    /// <summary>
    /// Builds an uploaded-file stand-in with the given descriptor, replacing the repeated mock
    /// arrangement the per-method tests each carried.
    /// </summary>
    /// <param name="fileName">The client-supplied file name, including its extension.</param>
    /// <param name="contentType">The client-supplied media type, which may be absent.</param>
    /// <param name="length">The reported file size in bytes.</param>
    /// <returns>The configured file.</returns>
    private static IFormFile CreateFile(string fileName, string? contentType, long length = SampleLength)
    {
        var fileMock = new Mock<IFormFile>();

        fileMock.Setup(f => f.Length).Returns(length);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType!);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        return fileMock.Object;
    }

    /// <summary>
    /// Invokes the upload entry point named by the row.
    /// </summary>
    /// <param name="service">The service under test.</param>
    /// <param name="target">The entry point to exercise.</param>
    /// <param name="file">The file to upload.</param>
    /// <returns>The running upload.</returns>
    private static Task Upload(CloudinaryService service, UploadTarget target, IFormFile file) =>
        target switch
        {
            UploadTarget.Image => service.UploadImageAsync(file, PublicId),
            UploadTarget.Raw => service.UploadRawAsync(file, PublicId),
            UploadTarget.Video => service.UploadVideoAsync(file, PublicId),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unmapped upload target"),
        };

    /// <summary>
    /// The allow-list the given entry point validates file extensions against.
    /// </summary>
    /// <param name="target">The upload entry point.</param>
    /// <returns>The allowed extensions for the entry point.</returns>
    private static string[] AllowedExtensions(UploadTarget target) =>
        target switch
        {
            UploadTarget.Image => FileConstants.AllowedAvatarExtensions,
            UploadTarget.Raw => FileConstants.AllowedRawFileExtensions,
            UploadTarget.Video => FileConstants.AllowedVideoExtensions,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unmapped upload target"),
        };

    /// <summary>
    /// The allow-list the given entry point validates media types against.
    /// </summary>
    /// <param name="target">The upload entry point.</param>
    /// <returns>The allowed media types for the entry point.</returns>
    private static string[] AllowedMimeTypes(UploadTarget target) =>
        target switch
        {
            UploadTarget.Image => FileConstants.AllowedAvatarMimeTypes,
            UploadTarget.Raw => FileConstants.AllowedRawFileMimeTypes,
            UploadTarget.Video => FileConstants.AllowedVideoMimeTypes,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unmapped upload target"),
        };

    /// <summary>
    /// The size ceiling the given entry point enforces, in bytes.
    /// </summary>
    /// <param name="target">The upload entry point.</param>
    /// <returns>The maximum accepted file size for the entry point.</returns>
    private static long MaxSizeBytes(UploadTarget target) =>
        target switch
        {
            UploadTarget.Image => FileConstants.MaxAvatarFileSizeBytes,
            UploadTarget.Raw => FileConstants.MaxRawFileSizeBytes,
            UploadTarget.Video => FileConstants.MaxVideoFileSizeBytes,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unmapped upload target"),
        };

    #region Constructor

    [Fact]
    public void Constructor_WithValidSettings_ShouldNotThrow()
    {
        // Act
        CloudinaryService service = CreateService();

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region Rejected Uploads

    [Theory]
    [InlineData(UploadTarget.Image)]
    [InlineData(UploadTarget.Raw)]
    [InlineData(UploadTarget.Video)]
    public async Task Upload_WithNullFile_ShouldThrowBadRequestException(UploadTarget target)
    {
        // Arrange
        CloudinaryService service = CreateService();

        // Act
        Func<Task> act = () => Upload(service, target, null!);

        // Assert
        ExceptionAssertions<BadRequestException> assertion = await act.Should()
            .ThrowExactlyAsync<BadRequestException>();
        assertion.Which.Message.Should().Be(_i18n.File.FileRequired().Message);
    }

    [Theory]
    [InlineData(UploadTarget.Image, "avatar.jpg", "image/jpeg")]
    [InlineData(UploadTarget.Raw, "proof.pdf", "application/pdf")]
    [InlineData(UploadTarget.Video, "clip.mp4", "video/mp4")]
    public async Task Upload_WithEmptyFile_ShouldThrowBadRequestException(
        UploadTarget target,
        string fileName,
        string contentType
    )
    {
        // Arrange
        CloudinaryService service = CreateService();
        IFormFile file = CreateFile(fileName, contentType, length: 0);

        // Act
        Func<Task> act = () => Upload(service, target, file);

        // Assert
        ExceptionAssertions<BadRequestException> assertion = await act.Should()
            .ThrowExactlyAsync<BadRequestException>();
        assertion.Which.Message.Should().Be(_i18n.File.FileRequired().Message);
    }

    [Theory]
    [InlineData(UploadTarget.Image, "avatar.jpg", "image/jpeg", FileConstants.MaxAvatarFileSizeBytes + 1)]
    [InlineData(UploadTarget.Raw, "proof.pdf", "application/pdf", FileConstants.MaxRawFileSizeBytes + 1)]
    [InlineData(UploadTarget.Video, "clip.mp4", "video/mp4", FileConstants.MaxVideoFileSizeBytes + 1)]
    public async Task Upload_JustOverTheSizeCeiling_ShouldThrowBadRequestException(
        UploadTarget target,
        string fileName,
        string contentType,
        long length
    )
    {
        // Arrange
        CloudinaryService service = CreateService();
        IFormFile file = CreateFile(fileName, contentType, length);

        // Act
        Func<Task> act = () => Upload(service, target, file);

        // Assert
        ExceptionAssertions<BadRequestException> assertion = await act.Should()
            .ThrowExactlyAsync<BadRequestException>();
        assertion.Which.Message.Should().Be(_i18n.File.FileTooLarge(MaxSizeBytes(target) / BytesPerMegabyte).Message);
    }

    [Theory]
    [InlineData(UploadTarget.Image, "malware.exe", "application/x-msdownload", ".exe")]
    [InlineData(UploadTarget.Image, "doc.pdf", "application/pdf", ".pdf")]
    [InlineData(UploadTarget.Image, "clip.mp4", "video/mp4", ".mp4")]
    [InlineData(UploadTarget.Image, "README", "text/plain", "")]
    [InlineData(UploadTarget.Raw, "malware.exe", "application/x-msdownload", ".exe")]
    [InlineData(UploadTarget.Raw, "notes.txt", "text/plain", ".txt")]
    [InlineData(UploadTarget.Raw, "clip.mp4", "video/mp4", ".mp4")]
    [InlineData(UploadTarget.Video, "photo.jpg", "image/jpeg", ".jpg")]
    [InlineData(UploadTarget.Video, "doc.pdf", "application/pdf", ".pdf")]
    public async Task Upload_WithDisallowedExtension_ShouldThrowBadRequestException(
        UploadTarget target,
        string fileName,
        string contentType,
        string extension
    )
    {
        // Arrange
        CloudinaryService service = CreateService();
        IFormFile file = CreateFile(fileName, contentType);

        // Act
        Func<Task> act = () => Upload(service, target, file);

        // Assert
        ExceptionAssertions<BadRequestException> assertion = await act.Should()
            .ThrowExactlyAsync<BadRequestException>();
        assertion
            .Which.Message.Should()
            .Be(_i18n.File.InvalidFileExtension(extension, string.Join(", ", AllowedExtensions(target))).Message);
    }

    [Theory]
    [InlineData(UploadTarget.Image, "avatar.jpg", "video/mp4")]
    [InlineData(UploadTarget.Image, "avatar.png", "text/plain")]
    [InlineData(UploadTarget.Raw, "proof.jpg", "video/mp4")]
    [InlineData(UploadTarget.Raw, "proof.pdf", "text/plain")]
    [InlineData(UploadTarget.Video, "clip.mp4", "image/jpeg")]
    [InlineData(UploadTarget.Video, "clip.mov", "application/pdf")]
    public async Task Upload_WithAllowedExtensionButDisallowedContentType_ShouldThrowBadRequestException(
        UploadTarget target,
        string fileName,
        string contentType
    )
    {
        // Arrange
        CloudinaryService service = CreateService();
        IFormFile file = CreateFile(fileName, contentType);

        // Act
        Func<Task> act = () => Upload(service, target, file);

        // Assert
        ExceptionAssertions<BadRequestException> assertion = await act.Should()
            .ThrowExactlyAsync<BadRequestException>();
        assertion
            .Which.Message.Should()
            .Be(_i18n.File.InvalidFileType(contentType, string.Join(", ", AllowedMimeTypes(target))).Message);
    }

    #endregion

    #region Accepted Uploads

    [Theory]
    [InlineData(UploadTarget.Image, "avatar.jpg", "image/jpeg")]
    [InlineData(UploadTarget.Image, "avatar.jpg", "image/jpg")]
    [InlineData(UploadTarget.Image, "photo.jpeg", "image/jpeg")]
    [InlineData(UploadTarget.Image, "image.png", "image/png")]
    [InlineData(UploadTarget.Image, "animation.gif", "image/gif")]
    [InlineData(UploadTarget.Image, "modern.webp", "image/webp")]
    [InlineData(UploadTarget.Raw, "proof.pdf", "application/pdf")]
    [InlineData(UploadTarget.Raw, "proof.jpg", "image/jpeg")]
    [InlineData(UploadTarget.Raw, "proof.jpeg", "image/jpeg")]
    [InlineData(UploadTarget.Raw, "proof.png", "image/png")]
    [InlineData(UploadTarget.Raw, "proof.gif", "image/gif")]
    [InlineData(UploadTarget.Raw, "proof.webp", "image/webp")]
    [InlineData(UploadTarget.Video, "clip.mp4", "video/mp4")]
    [InlineData(UploadTarget.Video, "clip.mov", "video/quicktime")]
    [InlineData(UploadTarget.Video, "clip.webm", "video/webm")]
    [InlineData(UploadTarget.Video, "clip.avi", "video/x-msvideo")]
    [InlineData(UploadTarget.Video, "clip.mkv", "video/x-matroska")]
    [InlineData(UploadTarget.Video, "clip.3gp", "video/3gpp")]
    public async Task Upload_WithAllowedExtensionAndContentType_ShouldPassValidation(
        UploadTarget target,
        string fileName,
        string contentType
    )
    {
        // Arrange
        CloudinaryService service = CreateService();
        IFormFile file = CreateFile(fileName, contentType);

        // Act
        Func<Task> act = () => Upload(service, target, file);

        // Assert
        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Theory]
    [InlineData(UploadTarget.Image, "avatar.jpg", null)]
    [InlineData(UploadTarget.Image, "avatar.jpg", "")]
    [InlineData(UploadTarget.Image, "avatar.jpg", "application/octet-stream")]
    [InlineData(UploadTarget.Image, "avatar.jpg", "multipart/form-data")]
    [InlineData(UploadTarget.Raw, "proof.pdf", null)]
    [InlineData(UploadTarget.Raw, "proof.pdf", "")]
    [InlineData(UploadTarget.Raw, "proof.pdf", "application/octet-stream")]
    [InlineData(UploadTarget.Raw, "proof.pdf", "multipart/form-data")]
    [InlineData(UploadTarget.Video, "clip.mp4", null)]
    [InlineData(UploadTarget.Video, "clip.mp4", "")]
    [InlineData(UploadTarget.Video, "clip.mp4", "application/octet-stream")]
    [InlineData(UploadTarget.Video, "clip.mp4", "multipart/form-data")]
    public async Task Upload_WithToleratedContentType_ShouldPassValidation(
        UploadTarget target,
        string fileName,
        string? contentType
    )
    {
        // Arrange
        CloudinaryService service = CreateService();
        IFormFile file = CreateFile(fileName, contentType);

        // Act
        Func<Task> act = () => Upload(service, target, file);

        // Assert
        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Theory]
    [InlineData(UploadTarget.Image, "IMAGE.JPG", "image/jpeg")]
    [InlineData(UploadTarget.Image, "photo.png", "Image/PNG")]
    [InlineData(UploadTarget.Image, "avatar.jpg", "image/jpeg; boundary=----WebKitFormBoundary")]
    [InlineData(UploadTarget.Raw, "PROOF.PDF", "application/pdf")]
    [InlineData(UploadTarget.Raw, "proof.png", "Image/PNG")]
    [InlineData(UploadTarget.Raw, "proof.pdf", "application/pdf; charset=utf-8")]
    [InlineData(UploadTarget.Video, "CLIP.MP4", "video/mp4")]
    [InlineData(UploadTarget.Video, "clip.mov", "Video/QuickTime")]
    [InlineData(UploadTarget.Video, "clip.mp4", "video/mp4; boundary=----WebKitFormBoundary")]
    public async Task Upload_WithCasingOrParametersInTheDescriptor_ShouldNormalizeAndPassValidation(
        UploadTarget target,
        string fileName,
        string contentType
    )
    {
        // Arrange
        CloudinaryService service = CreateService();
        IFormFile file = CreateFile(fileName, contentType);

        // Act
        Func<Task> act = () => Upload(service, target, file);

        // Assert
        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Theory]
    [InlineData(UploadTarget.Image, "avatar.jpg", "image/jpeg", FileConstants.MaxAvatarFileSizeBytes)]
    [InlineData(UploadTarget.Raw, "proof.pdf", "application/pdf", FileConstants.MaxRawFileSizeBytes)]
    [InlineData(UploadTarget.Video, "clip.mp4", "video/mp4", FileConstants.MaxVideoFileSizeBytes)]
    public async Task Upload_AtExactlyTheSizeCeiling_ShouldPassValidation(
        UploadTarget target,
        string fileName,
        string contentType,
        long length
    )
    {
        // Arrange
        CloudinaryService service = CreateService();
        IFormFile file = CreateFile(fileName, contentType, length);

        // Act
        Func<Task> act = () => Upload(service, target, file);

        // Assert
        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    #endregion
}
