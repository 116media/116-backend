using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.UnpinCategoryFromFeed;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Mocks.Infrastructure;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Catalog.UseCases.Admin.Commands.UnpinCategoryFromFeed;

/// <summary>
/// Unit tests for <see cref="AdminUnpinCategoryFromFeedHandler"/>.
/// </summary>
public class AdminUnpinCategoryFromFeedHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly AdminUnpinCategoryFromFeedHandler _handler;

    public AdminUnpinCategoryFromFeedHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new AdminUnpinCategoryFromFeedHandler(
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            CreateCategoryDtoService(_fileStorageMock.Object)
        );
    }

    [Fact]
    public async Task Handle_WhenPinned_ShouldUnpinAndCommit()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity category = CategoryFactory.CreatePinned(videoType);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        AdminUnpinCategoryFromFeedResult result = await _handler.Handle(
            new AdminUnpinCategoryFromFeedCommand(category.Id.ToString()),
            CancellationToken.None
        );

        category.IsPinnedToFeed.Should().BeFalse();
        result.Category.IsPinnedToFeed.Should().BeFalse();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNotPinned_ShouldBeIdempotent()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity category = CategoryFactory.Create(videoType);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        AdminUnpinCategoryFromFeedResult result = await _handler.Handle(
            new AdminUnpinCategoryFromFeedCommand(category.Id.ToString()),
            CancellationToken.None
        );

        result.Category.IsPinnedToFeed.Should().BeFalse();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        _categoryRepositoryMock.SetupGetByIdOrThrowNotFound(id);

        Func<Task> act = () =>
            _handler.Handle(new AdminUnpinCategoryFromFeedCommand(id.ToString()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
