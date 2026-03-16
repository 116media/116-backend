using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;

/// <summary>
/// Unit tests for <see cref="AdminCreateVideoHandler"/>.
/// </summary>
public class AdminCreateVideoHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreateVideoHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();

    public AdminCreateVideoHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminCreateVideoHandler(
            _categoryRepositoryMock.Object,
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenValidFreeVideo_ShouldCreateAndReturnVideo()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        string slug = TestConstants.Content.Editorial.Video.ValidSlug;

        var command = new AdminCreateVideoCommand(
            CategoryId: category.Id,
            Title: TestConstants.Content.Editorial.Video.ValidTitle,
            Slug: slug,
            AuthorId: AuthorId,
            CustomerId: null,
            OrderItemId: null,
            Description: null,
            ShootingScheduledAt: null
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock.SetupGetBySlug(slug, null);

        VideoEntity created = VideoFactory.CreateWithCategory(category.Id, category);
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        AdminCreateVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Video.Should().NotBeNull();
        _videoRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenValidPaidVideo_ShouldCreateAndReturnVideo()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        string slug = TestConstants.Content.Editorial.Video.ValidSlug;
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();

        var command = new AdminCreateVideoCommand(
            CategoryId: category.Id,
            Title: TestConstants.Content.Editorial.Video.ValidTitle,
            Slug: slug,
            AuthorId: AuthorId,
            CustomerId: customerId,
            OrderItemId: orderItemId,
            Description: null,
            ShootingScheduledAt: null
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock.SetupGetBySlug(slug, null);

        VideoEntity created = VideoFactory.CreateWithCategory(category.Id, category);
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        AdminCreateVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Video.Should().NotBeNull();
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
            Title: TestConstants.Content.Editorial.Video.ValidTitle,
            Slug: TestConstants.Content.Editorial.Video.ValidSlug,
            AuthorId: AuthorId,
            CustomerId: null,
            OrderItemId: null,
            Description: null,
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
        const string slug = TestConstants.Content.Editorial.Video.ValidSlug;

        var command = new AdminCreateVideoCommand(
            CategoryId: category.Id,
            Title: TestConstants.Content.Editorial.Video.ValidTitle,
            Slug: slug,
            AuthorId: AuthorId,
            CustomerId: null,
            OrderItemId: null,
            Description: null,
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
