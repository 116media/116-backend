using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;

/// <summary>
/// Unit tests for <see cref="AdminUpdateVideoHandler"/>.
/// </summary>
public class AdminUpdateVideoHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateVideoHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUpdateVideoHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpdateVideoHandler(
            _categoryRepositoryMock.Object,
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    private static AdminUpdateVideoCommand BuildCommand(VideoEntity video, Guid categoryId) =>
        new(
            Id: video.Id.ToString(),
            CategoryId: categoryId,
            Title: TestConstants.Content.Editorial.Video.ValidTitle,
            Slug: TestConstants.Content.Editorial.Video.ValidSlug,
            Description: null,
            CustomerId: null,
            OrderItemId: null,
            SocialBoost: false,
            IsFeatured: false,
            FeaturedUntil: null,
            MetaTitle: null,
            MetaDescription: null
        );

    #region Success Cases

    [Fact]
    public async Task Handle_WhenDraftVideo_ShouldUpdateAndReturnVideo()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        VideoEntity video = VideoFactory.CreateWithCategory(CategoryId, category);
        AdminUpdateVideoCommand command = BuildCommand(video, category.Id);
        // video.Slug == command.Slug (both ValidSlug), so handler skips slug conflict check

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        // Act
        AdminUpdateVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Video.Should().NotBeNull();
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        VideoEntity dummy = VideoFactory.Create(CategoryId);
        AdminUpdateVideoCommand command = BuildCommand(dummy, CategoryId) with { Id = nonExistentId.ToString() };
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenVideoIsApproved_ShouldThrowBadRequestException()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateApproved(CategoryId);
        AdminUpdateVideoCommand command = BuildCommand(video, CategoryId);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenSlugConflictsWithAnotherVideo_ShouldThrowConflictException()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        VideoEntity video = VideoFactory.CreateWithSlug(CategoryId, "original-video-slug");
        AdminUpdateVideoCommand command = BuildCommand(video, category.Id);
        VideoEntity conflicting = VideoFactory.CreateWithSlug(CategoryId, command.Slug);

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock.SetupGetBySlug(command.Slug, conflicting);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
