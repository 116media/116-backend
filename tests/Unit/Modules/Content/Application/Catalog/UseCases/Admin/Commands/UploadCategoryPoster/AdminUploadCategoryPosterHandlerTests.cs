using _116.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
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

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster;

/// <summary>
/// Unit tests for <see cref="AdminUploadCategoryPosterHandler"/>.
/// </summary>
public class AdminUploadCategoryPosterHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUploadCategoryPosterHandler _handler;

    public AdminUploadCategoryPosterHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUploadCategoryPosterHandler(
            _categoryRepositoryMock.Object,
            _fileStorageMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldUploadPosterAndReturnCategory()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        FileReferenceDto fileEntity = FileReferenceDtoFactory.CreateImage();
        IFormFile file = CreateMockFormFile();

        var command = new AdminUploadCategoryPosterCommand(Id: category.Id.ToString(), File: file);

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _fileStorageMock.SetupUpload(StoredFileFactory.From(fileEntity));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        category.PosterFileId.Should().Be(fileEntity.Id);

        _unitOfWorkMock.VerifyExecutedInTransaction();
    }

    [Fact]
    public async Task Handle_WhenCategoryHasExistingPoster_ShouldReplacePoster()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        Guid existingPosterId = Guid.NewGuid();
        category.SetPosterFileId(existingPosterId);

        FileReferenceDto newFileEntity = FileReferenceDtoFactory.CreateImage();
        IFormFile file = CreateMockFormFile();

        var command = new AdminUploadCategoryPosterCommand(Id: category.Id.ToString(), File: file);

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _fileStorageMock.SetupUpload(StoredFileFactory.From(newFileEntity));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        category.PosterFileId.Should().Be(newFileEntity.Id);
        _unitOfWorkMock.VerifyExecutedInTransaction();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        IFormFile file = CreateMockFormFile();

        var command = new AdminUploadCategoryPosterCommand(Id: nonExistentId.ToString(), File: file);

        _categoryRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    private static IFormFile CreateMockFormFile()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("poster.jpg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        return fileMock.Object;
    }
}
