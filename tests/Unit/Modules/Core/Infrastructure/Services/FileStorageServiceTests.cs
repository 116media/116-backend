using _116.Core.Application.Shared.Mappers;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Services;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using AwesomeAssertions;
using Mapster;
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
    private readonly Mock<IFileRepository> _fileRepositoryMock = new();
    private readonly Mock<IFileUploadService> _fileUploadServiceMock = new();
    private readonly Mock<ICloudinaryService> _cloudinaryServiceMock = new();
    private readonly IMapper _mapper;
    private readonly PassThroughHybridCache _cache = new();
    private readonly FileStorageService _service;

    /// <summary>
    /// Initializes a new instance of <see cref="FileStorageServiceTests"/>.
    /// </summary>
    public FileStorageServiceTests()
    {
        _mapper = new Mapper(MappingRegistration.CreateConfiguration());
        _service = new FileStorageService(
            _fileRepositoryMock.Object,
            _fileUploadServiceMock.Object,
            _cloudinaryServiceMock.Object,
            _mapper,
            _cache
        );
    }

    /// <summary>
    /// Builds an uploaded file for the case under test.
    /// </summary>
    /// <param name="contentType">The submitted content type.</param>
    /// <returns>The mocked form file.</returns>
    private static IFormFile FormFile(string contentType = "image/jpeg")
    {
        Mock<IFormFile> file = new();
        file.Setup(f => f.ContentType).Returns(contentType);
        file.Setup(f => f.FileName).Returns("asset.jpg");
        return file.Object;
    }

    #region Upload

    [Fact]
    public async Task UploadAsync_WithTheImageKind_ShouldUseTheImagePipeline()
    {
        // Arrange
        FileEntity uploaded = FileFactory.CreateJpeg();
        _fileUploadServiceMock
            .Setup(x =>
                x.UploadImageAsync(
                    It.IsAny<IFormFile>(),
                    "poster",
                    "content/posters",
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(uploaded);

        // Act
        StoredFile handle = await _service.UploadAsync(
            file: FormFile(),
            publicId: "poster",
            folder: "content/posters",
            kind: EnumStoredFileKind.Image,
            cancellationToken: CancellationToken.None
        );

        // Assert
        handle.Reference.Id.Should().Be(uploaded.Id);
        _fileUploadServiceMock.Verify(
            x =>
                x.UploadVideoAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task UploadAsync_WithTheVideoKind_ShouldUseTheVideoPipeline()
    {
        // Arrange
        FileEntity uploaded = FileFactory.CreateVideo();
        _fileUploadServiceMock
            .Setup(x =>
                x.UploadVideoAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(uploaded);

        // Act
        StoredFile handle = await _service.UploadAsync(
            file: FormFile("video/mp4"),
            publicId: "clip",
            folder: "content/videos",
            kind: EnumStoredFileKind.Video,
            cancellationToken: CancellationToken.None
        );

        // Assert
        handle.Reference.Id.Should().Be(uploaded.Id);
    }

    [Fact]
    public async Task UploadAsync_WithTheRawKind_ShouldUseTheRawPipeline()
    {
        // Arrange
        FileEntity uploaded = FileFactory.CreatePdf();
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
            .ReturnsAsync(uploaded);

        // Act
        StoredFile handle = await _service.UploadAsync(
            file: FormFile("application/pdf"),
            publicId: "proof",
            folder: "content/proofs",
            kind: EnumStoredFileKind.Raw,
            cancellationToken: CancellationToken.None
        );

        // Assert
        handle.Reference.Id.Should().Be(uploaded.Id);
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
        // The stored mime type decides the storage class a delete targets, so a browser's
        // charset or boundary must never reach the row.
        string captured = string.Empty;
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
            .ReturnsAsync(FileFactory.CreatePdf());

        // Act
        await _service.UploadAsync(
            file: FormFile(submitted),
            publicId: "proof",
            folder: "content/proofs",
            kind: EnumStoredFileKind.Raw,
            cancellationToken: CancellationToken.None
        );

        // Assert
        captured.Should().Be(expected);
    }

    [Fact]
    public async Task UploadFromUrlAsync_WhenTheProviderAssetIsFetched_ShouldReturnAHandle()
    {
        // Arrange
        FileEntity uploaded = FileFactory.CreateJpeg();
        _fileUploadServiceMock
            .Setup(x =>
                x.UploadAvatarFromUrlAsync(It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(uploaded);

        // Act
        StoredFile? handle = await _service.UploadFromUrlAsync(
            currentFileId: null,
            url: "https://cdn.example/avatar.jpg",
            cancellationToken: CancellationToken.None
        );

        // Assert
        handle.Should().NotBeNull();
        handle!.Reference.StorageUrl.Should().Be(uploaded.StorageUrl);
    }

    [Fact]
    public async Task UploadFromUrlAsync_WhenTheAvatarAlreadyComesFromThatUrl_ShouldReturnNull()
    {
        // Arrange
        _fileUploadServiceMock
            .Setup(x =>
                x.UploadAvatarFromUrlAsync(It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((FileEntity?)null);

        // Act
        StoredFile? handle = await _service.UploadFromUrlAsync(
            currentFileId: Guid.NewGuid(),
            url: "https://cdn.example/avatar.jpg",
            cancellationToken: CancellationToken.None
        );

        // Assert
        handle.Should().BeNull();
    }

    #endregion

    #region Record

    [Fact]
    public async Task RecordAsync_ShouldRebuildTheAggregateFromTheHandleWithoutLosingAField()
    {
        // The reference carries every field the factory takes; this pins that the round trip
        // through the contract is lossless.
        FileEntity uploaded = FileFactory.CreateJpeg();
        StoredFile handle = StoredFileFactory.From(uploaded);
        FileEntity? rebuilt = null;

        _fileUploadServiceMock
            .Setup(x => x.RecordAsync(It.IsAny<FileEntity>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .Callback<FileEntity, Guid?, CancellationToken>((file, _, _) => rebuilt = file)
            .ReturnsAsync((FileEntity file, Guid? _, CancellationToken _) => file);

        // Act
        FileReferenceDto recorded = await _service.RecordAsync(file: handle, cancellationToken: CancellationToken.None);

        // Assert
        rebuilt.Should().NotBeNull();
        rebuilt!.Id.Should().Be(uploaded.Id);
        rebuilt.FileName.Should().Be(uploaded.FileName);
        rebuilt.OriginalFileName.Should().Be(uploaded.OriginalFileName);
        rebuilt.MimeType.Should().Be(uploaded.MimeType);
        rebuilt.StorageUrl.Should().Be(uploaded.StorageUrl);
        rebuilt.SizeInBytes.Should().Be(uploaded.SizeInBytes);
        rebuilt.StorageKey.Should().Be(uploaded.StorageKey);
        recorded.Id.Should().Be(uploaded.Id);
    }

    [Fact]
    public async Task RecordAsync_ShouldPassTheSupersededFileThrough()
    {
        // Arrange
        var supersededFileId = Guid.NewGuid();
        StoredFile handle = StoredFileFactory.From(FileFactory.CreateJpeg());
        _fileUploadServiceMock
            .Setup(x => x.RecordAsync(It.IsAny<FileEntity>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileEntity file, Guid? _, CancellationToken _) => file);

        // Act
        await _service.RecordAsync(
            file: handle,
            supersededFileId: supersededFileId,
            cancellationToken: CancellationToken.None
        );

        // Assert
        _fileUploadServiceMock.Verify(
            x => x.RecordAsync(It.IsAny<FileEntity>(), supersededFileId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region Resolve

    [Fact]
    public async Task ResolveAsync_WithoutAnId_ShouldReturnNullWithoutQuerying()
    {
        // Act
        FileReferenceDto? reference = await _service.ResolveAsync(fileId: null);

        // Assert
        reference.Should().BeNull();
        _fileRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResolveAsync_WhenTheFileExists_ShouldProjectIt()
    {
        // Arrange
        FileEntity file = FileFactory.CreateJpeg();
        _fileRepositoryMock.Setup(x => x.GetByIdAsync(file.Id, It.IsAny<CancellationToken>())).ReturnsAsync(file);

        // Act
        FileReferenceDto? reference = await _service.ResolveAsync(file.Id);

        // Assert
        reference.Should().NotBeNull();
        reference!.StorageUrl.Should().Be(file.StorageUrl);
    }

    [Fact]
    public async Task ResolveAsync_WhenTheFileIsAbsent_ShouldReturnNull()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        _fileRepositoryMock
            .Setup(x => x.GetByIdAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileEntity?)null);

        // Act
        FileReferenceDto? reference = await _service.ResolveAsync(fileId);

        // Assert
        reference.Should().BeNull();
    }

    [Fact]
    public async Task ResolveManyAsync_ShouldProjectEveryFoundFileKeyedById()
    {
        // Arrange
        FileEntity first = FileFactory.CreateJpeg();
        FileEntity second = FileFactory.CreatePng();
        _fileRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, FileEntity> { [first.Id] = first, [second.Id] = second });

        // Act
        IReadOnlyDictionary<Guid, FileReferenceDto> references = await _service.ResolveManyAsync([first.Id, second.Id]);

        // Assert
        references.Should().HaveCount(2);
        references[first.Id].StorageUrl.Should().Be(first.StorageUrl);
        references[second.Id].StorageUrl.Should().Be(second.StorageUrl);
    }

    [Fact]
    public async Task ResolveUrlsAsync_ShouldProjectTheUrlOfEveryResolvedFile()
    {
        // Arrange
        FileEntity file = FileFactory.CreateJpeg();
        _fileRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, FileEntity> { [file.Id] = file });

        // Act
        IReadOnlyDictionary<Guid, string> resolved = await _service.ResolveUrlsAsync([file.Id]);

        // Assert
        resolved.Should().BeEquivalentTo(new Dictionary<Guid, string> { [file.Id] = file.StorageUrl });
    }

    [Fact]
    public async Task ResolveUrlsAsync_ShouldShareOneCacheEntryPerFileWithTheReferenceProjection()
    {
        // Both projections read the same row, so they must not occupy two key spaces.
        FileEntity file = FileFactory.CreateJpeg();
        _fileRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, FileEntity> { [file.Id] = file });

        await _service.ResolveUrlsAsync([file.Id]);

        _cache.WrittenKeys.Should().ContainSingle().Which.Should().Contain(file.Id.ToString());
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteTheRow()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        _fileRepositoryMock.Setup(x => x.SoftDeleteByIdAsync(fileId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        bool deleted = await _service.DeleteAsync(fileId);

        // Assert
        deleted.Should().BeTrue();
        _fileRepositoryMock.Verify(x => x.SoftDeleteByIdAsync(fileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAssetsAsync_ShouldRemoveTheKeysUnderTheGivenKind()
    {
        // Deleting under the wrong resource type silently leaves the asset in place, so the
        // kind must reach the gateway unchanged.
        List<string> keys = ["content/articles/image-0"];
        _cloudinaryServiceMock
            .Setup(x =>
                x.DeleteManyAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<EnumStoredFileKind>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        // Act
        bool removed = await _service.DeleteAssetsAsync(keys, EnumStoredFileKind.Video);

        // Assert
        removed.Should().BeTrue();
        _cloudinaryServiceMock.Verify(
            x => x.DeleteManyAsync(keys, EnumStoredFileKind.Video, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion
}
