using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
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
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminUpdateVideoHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUpdateVideoHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminUpdateVideoHandler(
            _categoryRepositoryMock.Object,
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileRepositoryMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    private static AdminUpdateVideoCommand BuildCommand(VideoEntity video, Guid categoryId) =>
        new(
            Id: video.Id.ToString(),
            CategoryId: categoryId,
            Title: TestConstants.Video.ValidTitle,
            Slug: TestConstants.Video.ValidSlug,
            Description: TestConstants.Video.ValidDescription,
            CustomerId: null,
            OrderItemId: null,
            SocialBoost: false,
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

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        // Act
        AdminUpdateVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.CategoryId.Should().Be(command.CategoryId);
        video.Title.Should().Be(command.Title);
        video.Slug.Should().Be(command.Slug);
        video.Description.Should().Be(command.Description);
        video.CustomerId.Should().BeNull();
        video.OrderItemId.Should().BeNull();
        video.SocialBoost.Should().BeFalse();
        video.MetaTitle.Should().BeNull();
        video.MetaDescription.Should().BeNull();
        result.Video.Id.Should().Be(video.Id);
        result.Video.Title.Should().Be(command.Title);
        result.Video.Slug.Should().Be(command.Slug);
        _videoRepositoryMock.VerifyUpdateCalled(video);
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
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoIsApproved_ShouldThrowBadRequestException()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateApproved(CategoryId);
        string originalTitle = video.Title;
        string originalSlug = video.Slug;
        AdminUpdateVideoCommand command = BuildCommand(video, CategoryId);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        video.Status.Should().Be(EnumContentStatus.Approved);
        video.Title.Should().Be(originalTitle);
        video.Slug.Should().Be(originalSlug);
        _unitOfWorkMock.VerifyCommitNotCalled();
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
        video.Slug.Should().Be("original-video-slug");
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
