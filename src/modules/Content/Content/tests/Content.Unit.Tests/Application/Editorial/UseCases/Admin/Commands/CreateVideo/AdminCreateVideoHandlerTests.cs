using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.CreateVideo;

/// <summary>
/// Unit tests for <see cref="AdminCreateVideoHandler"/>.
/// </summary>
public class AdminCreateVideoHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly AdminCreateVideoHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();

    public AdminCreateVideoHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new AdminCreateVideoHandler(
            new AdminCreateVideoService(
                _categoryRepositoryMock.Object,
                _videoRepositoryMock.Object,
                TestErrorsFactory.CreateContentI18n()
            ),
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            new VideoDtoService(
                Mapper,
                _fileStorageMock.Object,
                _videoRepositoryMock.Object,
                CreateContentLookupService()
            )
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenValidFreeVideo_ShouldCreateAndReturnVideo()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        string slug = TestConstants.Video.ValidSlug;

        var command = new AdminCreateVideoCommand(
            CategoryId: category.Id,
            Title: TestConstants.Video.ValidTitle,
            Slug: slug,
            AuthorId: AuthorId,
            CustomerId: null,
            OrderItemId: null,
            Description: TestConstants.Video.ValidDescription,
            ShootingScheduledAt: null
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock.SetupGetBySlug(slug, null);

        VideoEntity created = VideoFactory.CreateWithCategory(category);
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        AdminCreateVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Video.Id.Should().Be(created.Id);
        _videoRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenValidPaidVideo_ShouldCreateAndReturnVideo()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        string slug = TestConstants.Video.ValidSlug;
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();

        var command = new AdminCreateVideoCommand(
            CategoryId: category.Id,
            Title: TestConstants.Video.ValidTitle,
            Slug: slug,
            AuthorId: AuthorId,
            CustomerId: customerId,
            OrderItemId: orderItemId,
            Description: TestConstants.Video.ValidDescription,
            ShootingScheduledAt: null
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock.SetupGetBySlug(slug, null);

        VideoEntity created = VideoFactory.CreateWithCategory(category);
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        AdminCreateVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Video.Id.Should().Be(created.Id);
        _videoRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminCreateVideoCommand(
            CategoryId: nonExistentId,
            Title: TestConstants.Video.ValidTitle,
            Slug: TestConstants.Video.ValidSlug,
            AuthorId: AuthorId,
            CustomerId: null,
            OrderItemId: null,
            Description: TestConstants.Video.ValidDescription,
            ShootingScheduledAt: null
        );
        _categoryRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        const string slug = TestConstants.Video.ValidSlug;

        var command = new AdminCreateVideoCommand(
            CategoryId: category.Id,
            Title: TestConstants.Video.ValidTitle,
            Slug: slug,
            AuthorId: AuthorId,
            CustomerId: null,
            OrderItemId: null,
            Description: TestConstants.Video.ValidDescription,
            ShootingScheduledAt: null
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        VideoEntity existing = VideoFactory.CreateWithSlug(category.Id, slug);
        _videoRepositoryMock.SetupGetBySlug(slug, existing);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
