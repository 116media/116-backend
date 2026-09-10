using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Services;
using _116.Tests.Fixtures.Factories.Core;
using AwesomeAssertions;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="FileStorageService"/>, Core's implementation of the cross-module
/// storage contract.
/// </summary>
public class FileStorageServiceTests
{
    private readonly Mock<IFileUploadService> _fileUploadServiceMock = new();
    private readonly FileStorageService _service;

    /// <summary>
    /// Initializes a new instance of <see cref="FileStorageServiceTests"/>.
    /// </summary>
    public FileStorageServiceTests()
    {
        _service = new FileStorageService(
            new Mock<IFileRepository>().Object,
            _fileUploadServiceMock.Object,
            new Mock<ICloudinaryService>().Object,
            new Mock<IMapper>().Object
        );
    }

    [Theory]
    [InlineData("application/pdf; charset=utf-8", "application/pdf")]
    [InlineData("image/JPEG; boundary=----WebKitFormBoundary", "image/jpeg")]
    [InlineData("  video/mp4  ", "video/mp4")]
    public async Task UploadAsync_WithParametersOrCasingInTheContentType_ShouldStoreTheBareMediaType(
        string submitted,
        string expected
    )
    {
        // Arrange
        string captured = string.Empty;
        FileEntity uploaded = FileFactory.CreateJpeg();

        _fileUploadServiceMock
            .Setup(x =>
                x.UploadRawAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<IFormFile, string, string, string, string, CancellationToken>(
                (_, _, _, _, mimeType, _) => captured = mimeType
            )
            .ReturnsAsync(uploaded);

        Mock<IFormFile> file = new();
        file.Setup(f => f.ContentType).Returns(submitted);
        file.Setup(f => f.FileName).Returns("proof.pdf");

        // Act
        await _service.UploadAsync(
            file: file.Object,
            publicId: "proof",
            folder: "content/proofs",
            kind: EnumStoredFileKind.Raw,
            cancellationToken: CancellationToken.None
        );

        // Assert
        captured.Should().Be(expected);
    }
}
