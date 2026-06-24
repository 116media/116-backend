using _116.Content.Application.Catalog.Constants;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed;

/// <summary>
/// Unit tests for <see cref="AdminPinCategoryToFeedHandler"/>.
/// </summary>
public class AdminPinCategoryToFeedHandlerTests : BaseContentHandlerTest
{
    private const int Min = EditorialFeedConstants.MinVideosToPinToFeed;
    private const int Cap = CatalogFeedConstants.MaxPinnedCategoriesPerContentType;

    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminPinCategoryToFeedHandler _handler;

    public AdminPinCategoryToFeedHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminPinCategoryToFeedHandler(
            _categoryRepositoryMock.Object,
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileRepositoryMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    private static AdminPinCategoryToFeedCommand Command(CategoryEntity category) => new(Id: category.Id.ToString());

    #region Success Cases

    [Fact]
    public async Task Handle_WhenEligibleAndBelowCap_ShouldPin()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity category = CategoryFactory.Create(videoType);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock.SetupCountPublishedByCategory(category.Id, Min);

        AdminPinCategoryToFeedResult result = await _handler.Handle(Command(category), CancellationToken.None);

        result.Category.IsPinnedToFeed.Should().BeTrue();
        category.IsPinnedToFeed.Should().BeTrue();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenExactlyMinimumPublishedVideos_ShouldPin()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity category = CategoryFactory.Create(videoType);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock.SetupCountPublishedByCategory(category.Id, Min);

        await _handler.Handle(Command(category), CancellationToken.None);

        category.IsPinnedToFeed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenCapReached_ShouldEvictOldestAndPinNew()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        var baseTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        List<CategoryEntity> existing = CategoryFactory.CreateManyPinned(videoType, Cap, baseTime);
        CategoryEntity newCategory = CategoryFactory.Create(videoType);

        _categoryRepositoryMock.SetupGetByIdOrThrow(newCategory);
        _categoryRepositoryMock.SetupGetPinnedToFeedCategories(existing);
        _videoRepositoryMock.SetupCountPublishedByCategory(newCategory.Id, Min);

        await _handler.Handle(Command(newCategory), CancellationToken.None);

        existing[0].IsPinnedToFeed.Should().BeFalse();
        existing.Skip(1).Should().OnlyContain(c => c.IsPinnedToFeed);
        newCategory.IsPinnedToFeed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAlreadyPinnedAtCap_ShouldRefreshAndNotEvict()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        var baseTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        List<CategoryEntity> existing = CategoryFactory.CreateManyPinned(videoType, Cap, baseTime);
        CategoryEntity target = existing[0];

        _categoryRepositoryMock.SetupGetByIdOrThrow(target);
        _categoryRepositoryMock.SetupGetPinnedToFeedCategories(existing);
        _videoRepositoryMock.SetupCountPublishedByCategory(target.Id, Min);

        await _handler.Handle(Command(target), CancellationToken.None);

        existing.Should().OnlyContain(c => c.IsPinnedToFeed);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenInactive_ShouldThrowBadRequest()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity category = CategoryFactory.Create(videoType);
        category.Deactivate();
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        Func<Task> act = () => _handler.Handle(Command(category), CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        category.IsPinnedToFeed.Should().BeFalse();
    }

    [Theory]
    [InlineData(nameof(EnumCoreContentType.Short))]
    [InlineData(nameof(EnumCoreContentType.Custom))]
    [InlineData(nameof(EnumCoreContentType.Article))]
    public async Task Handle_WhenNonVideoContentType_ShouldThrowBadRequest(string typeName)
    {
        ContentTypeEntity type = ContentTypeFactory.Create(typeName);
        CategoryEntity category = CategoryFactory.Create(type);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        Func<Task> act = () => _handler.Handle(Command(category), CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenFewerThanMinimumPublishedVideos_ShouldThrowBadRequest()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity category = CategoryFactory.Create(videoType);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock.SetupCountPublishedByCategory(category.Id, Min - 1);

        Func<Task> act = () => _handler.Handle(Command(category), CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        category.IsPinnedToFeed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        _categoryRepositoryMock.SetupGetByIdOrThrowNotFound(id);

        Func<Task> act = () =>
            _handler.Handle(new AdminPinCategoryToFeedCommand(id.ToString()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
