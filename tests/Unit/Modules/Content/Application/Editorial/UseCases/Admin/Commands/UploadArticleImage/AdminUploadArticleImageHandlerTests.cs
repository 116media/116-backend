using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage;

/// <summary>
/// Unit tests for <see cref="AdminUploadArticleImageHandler"/>.
/// </summary>
public class AdminUploadArticleImageHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<ICloudinaryService> _cloudinaryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadArticleImageHandler _handler;
    private readonly IFormFile _mockFile;
    private readonly FileEntity _coverFile;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUploadArticleImageHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _cloudinaryMock = MockCloudinaryService.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        _coverFile = FileFactory.CreateImage();
        _fileRepositoryMock.SetupReplaceImageFile(_coverFile);

        _handler = new AdminUploadArticleImageHandler(
            _articleRepositoryMock.Object,
            _cloudinaryMock.Object,
            _fileRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
        _mockFile = MockYoutubeThumbnailService.CreateMockFormFile();
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenBodyImageUpload_ShouldUploadAndReturnImage()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var command = new AdminUploadArticleImageCommand(
            ArticleId: article.Id.ToString(),
            File: _mockFile,
            ImageType: EnumArticleImageType.Body
        );

        _articleRepositoryMock.SetupGetByIdOrThrow(article);

        // Act
        AdminUploadArticleImageResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Image.Url.Should().Be(TestConstants.Cloudinary.ValidSecureUrl);
        result.Image.StorageKey.Should().Be(TestConstants.Cloudinary.ValidPublicId);
        result.Image.ImageType.Should().Be(EnumArticleImageType.Body);
        article.CoverImageFileId.Should().BeNull();
        _cloudinaryMock.VerifyUploadCalled();
        _articleRepositoryMock.VerifyAddImageCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenCoverImageUploadWithNoExistingCover_ShouldUploadAndUpdateArticle()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        var command = new AdminUploadArticleImageCommand(
            ArticleId: article.Id.ToString(),
            File: _mockFile,
            ImageType: EnumArticleImageType.Cover
        );

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetImagesByArticleId(article.Id, new List<ArticleImageEntity>());

        // Act
        AdminUploadArticleImageResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        article.CoverImageFileId.Should().Be(_coverFile.Id);
        result.Image.Url.Should().Be(_coverFile.StorageUrl);
        result.Image.ImageType.Should().Be(EnumArticleImageType.Cover);
        _fileRepositoryMock.VerifyReplaceImageFileCalled();
        _articleRepositoryMock.VerifyUpdateCalled(article);
        _articleRepositoryMock.VerifyAddImageCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenCoverImageUploadWithExistingCover_ShouldRemoveOldCoverAndUpload()
    {
        // Arrange
        ArticleEntity article = ArticleFactory.Create(CategoryId);
        ArticleImageEntity oldCover = ArticleImageFactory.CreateCover(article.Id);
        var command = new AdminUploadArticleImageCommand(
            ArticleId: article.Id.ToString(),
            File: _mockFile,
            ImageType: EnumArticleImageType.Cover
        );

        _articleRepositoryMock.SetupGetByIdOrThrow(article);
        _articleRepositoryMock.SetupGetImagesByArticleId(article.Id, new List<ArticleImageEntity> { oldCover });

        // Act
        AdminUploadArticleImageResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        article.CoverImageFileId.Should().Be(_coverFile.Id);
        result.Image.ImageType.Should().Be(EnumArticleImageType.Cover);
        _fileRepositoryMock.VerifyReplaceImageFileCalled();
        _articleRepositoryMock.Verify(
            x => x.RemoveImages(It.Is<IEnumerable<ArticleImageEntity>>(images => images.Single() == oldCover)),
            Times.Once
        );
        _articleRepositoryMock.VerifyUpdateCalled(article);
        _articleRepositoryMock.VerifyAddImageCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenArticleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminUploadArticleImageCommand(
            ArticleId: nonExistentId.ToString(),
            File: _mockFile,
            ImageType: EnumArticleImageType.Body
        );
        _articleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
