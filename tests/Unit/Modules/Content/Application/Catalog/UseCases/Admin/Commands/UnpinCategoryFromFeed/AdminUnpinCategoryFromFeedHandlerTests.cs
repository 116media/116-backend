using _116.Content.Application.Catalog.UseCases.Admin.Commands.UnpinCategoryFromFeed;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UnpinCategoryFromFeed;

/// <summary>
/// Unit tests for <see cref="AdminUnpinCategoryFromFeedHandler"/>.
/// </summary>
public class AdminUnpinCategoryFromFeedHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminUnpinCategoryFromFeedHandler _handler;

    public AdminUnpinCategoryFromFeedHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminUnpinCategoryFromFeedHandler(
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileRepositoryMock.Object,
            Mapper
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
