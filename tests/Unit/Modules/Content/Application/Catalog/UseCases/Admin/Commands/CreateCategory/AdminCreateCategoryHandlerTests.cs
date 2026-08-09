using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;
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

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;

/// <summary>
/// Unit tests for <see cref="AdminCreateCategoryHandler"/>.
/// </summary>
public class AdminCreateCategoryHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminCreateCategoryHandler _handler;

    public AdminCreateCategoryHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminCreateCategoryHandler(
            _lookupRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileRepositoryMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateAndReturnCategory()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        string name = TestConstants.Category.ValidName;
        string slug = TestConstants.Category.ValidSlug;

        var command = new AdminCreateCategoryCommand(
            ContentTypeId: contentType.Id.ToString(),
            Name: name,
            Slug: slug,
            Description: TestConstants.Category.ValidDescription,
            IsFree: false,
            IsGossip: false,
            IsExclusive: false
        );

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);
        _categoryRepositoryMock.SetupGetBySlug(slug, null);

        CategoryEntity created = CategoryFactory.Create(contentType.Id, name, slug);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        AdminCreateCategoryResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().NotBeNull();

        _categoryRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenSlugNotFound_ShouldNotThrowConflict()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        string slug = TestConstants.Category.ValidSlug;

        var command = new AdminCreateCategoryCommand(
            ContentTypeId: contentType.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: slug,
            Description: TestConstants.Category.ValidDescription,
            IsFree: true,
            IsGossip: false,
            IsExclusive: false
        );

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);
        _categoryRepositoryMock.SetupGetBySlug(slug, null);

        CategoryEntity created = CategoryFactory.Create(contentType.Id);
        _categoryRepositoryMock.SetupGetByIdOrThrow(created);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_ShouldReloadCategoryAfterCreation()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        string slug = TestConstants.Category.ValidSlug;

        var command = new AdminCreateCategoryCommand(
            ContentTypeId: contentType.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: slug,
            Description: TestConstants.Category.ValidDescription,
            IsFree: false,
            IsGossip: false,
            IsExclusive: false
        );

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);
        _categoryRepositoryMock.SetupGetBySlug(slug, null);

        CategoryEntity reloaded = CategoryFactory.Create(contentType.Id);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reloaded);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(
            x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region Exclusive Mutex

    [Fact]
    public async Task Handle_WithIsExclusive_ShouldUnsetCurrentExclusive()
    {
        // Arrange
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity currentExclusive = CategoryFactory.Create(videoType);
        currentExclusive.SetExclusive();

        var command = new AdminCreateCategoryCommand(
            ContentTypeId: videoType.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: TestConstants.Category.ValidSlug,
            Description: TestConstants.Category.ValidDescription,
            IsFree: false,
            IsGossip: false,
            IsExclusive: true
        );

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrow(videoType);
        _categoryRepositoryMock.SetupGetBySlug(TestConstants.Category.ValidSlug, null);
        _categoryRepositoryMock.SetupGetExclusiveCategory(currentExclusive);

        CategoryEntity created = CategoryFactory.Create(videoType);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        currentExclusive.IsExclusive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithIsExclusiveFalse_ShouldNotQueryExclusive()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();

        var command = new AdminCreateCategoryCommand(
            ContentTypeId: contentType.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: TestConstants.Category.ValidSlug,
            Description: TestConstants.Category.ValidDescription,
            IsFree: false,
            IsGossip: false,
            IsExclusive: false
        );

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);
        _categoryRepositoryMock.SetupGetBySlug(TestConstants.Category.ValidSlug, null);

        CategoryEntity created = CategoryFactory.Create(contentType.Id);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(x => x.GetExclusiveCategoryAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithIsExclusive_NoCurrentExclusive_ShouldSucceed()
    {
        // Arrange
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));

        var command = new AdminCreateCategoryCommand(
            ContentTypeId: videoType.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: TestConstants.Category.ValidSlug,
            Description: TestConstants.Category.ValidDescription,
            IsFree: false,
            IsGossip: false,
            IsExclusive: true
        );

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrow(videoType);
        _categoryRepositoryMock.SetupGetBySlug(TestConstants.Category.ValidSlug, null);
        _categoryRepositoryMock.SetupGetExclusiveCategory(null);

        CategoryEntity created = CategoryFactory.Create(videoType);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithIsExclusive_NonVideoContentType_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentTypeEntity articleType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Article));

        var command = new AdminCreateCategoryCommand(
            ContentTypeId: articleType.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: TestConstants.Category.ValidSlug,
            Description: TestConstants.Category.ValidDescription,
            IsFree: false,
            IsGossip: false,
            IsExclusive: true
        );

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrow(articleType);
        _categoryRepositoryMock.SetupGetBySlug(TestConstants.Category.ValidSlug, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenContentTypeNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        var command = new AdminCreateCategoryCommand(
            ContentTypeId: nonExistentId.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: TestConstants.Category.ValidSlug,
            Description: TestConstants.Category.ValidDescription,
            IsFree: false,
            IsGossip: false,
            IsExclusive: false
        );

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        const string slug = TestConstants.Category.ValidSlug;

        var command = new AdminCreateCategoryCommand(
            ContentTypeId: contentType.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: slug,
            Description: TestConstants.Category.ValidDescription,
            IsFree: false,
            IsGossip: false,
            IsExclusive: false
        );

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);

        CategoryEntity existing = CategoryFactory.Create(contentType.Id, "Other Name", slug);
        _categoryRepositoryMock.SetupGetBySlug(slug, existing);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldNotAddOrCommit()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        string slug = TestConstants.Category.ValidSlug;

        var command = new AdminCreateCategoryCommand(
            ContentTypeId: contentType.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: slug,
            Description: TestConstants.Category.ValidDescription,
            IsFree: false,
            IsGossip: false,
            IsExclusive: false
        );

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);

        CategoryEntity existing = CategoryFactory.Create(contentType.Id, "Other Name", slug);
        _categoryRepositoryMock.SetupGetBySlug(slug, existing);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (ConflictException)
        {
            // Expected
        }

        // Assert
        _categoryRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<CategoryEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
