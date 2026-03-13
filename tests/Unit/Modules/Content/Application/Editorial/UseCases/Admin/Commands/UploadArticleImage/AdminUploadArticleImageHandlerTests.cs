using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Services;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
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
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadArticleImageHandler _handler;
    private readonly IFormFile _mockFile;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUploadArticleImageHandlerTests()
    {
        _articleRepositoryMock = MockArticleRepository.Create();
        _cloudinaryMock = MockCloudinaryService.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUploadArticleImageHandler(
            _articleRepositoryMock.Object,
            _cloudinaryMock.Object,
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
        result.Should().NotBeNull();
        result.Image.Should().NotBeNull();
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
        result.Should().NotBeNull();
        result.Image.Should().NotBeNull();
        _cloudinaryMock.VerifyUploadCalled();
        _articleRepositoryMock.VerifyUpdateCalled();
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
        result.Should().NotBeNull();
        _cloudinaryMock.VerifyUploadCalled();
        _articleRepositoryMock.VerifyRemoveImagesCalled();
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
    }

    #endregion
}
