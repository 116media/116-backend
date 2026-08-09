using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;
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

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;

/// <summary>
/// Unit tests for <see cref="AdminUpdateCategoryHandler"/>.
/// </summary>
public class AdminUpdateCategoryHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminUpdateCategoryHandler _handler;

    public AdminUpdateCategoryHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminUpdateCategoryHandler(
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileRepositoryMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldUpdateAndReturnCategory()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        string newName = TestConstants.Category.AnotherValidName;
        string newSlug = TestConstants.Category.AnotherValidSlug;

        var command = new AdminUpdateCategoryCommand(
            Id: category.Id.ToString(),
            Name: newName,
            Slug: newSlug,
            Description: "Updated description",
            IsGossip: false,
            IsExclusive: false,
            IsDefaultForLyrics: false
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetBySlug(newSlug, null);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category)
            .Verifiable();

        // Act
        AdminUpdateCategoryResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().NotBeNull();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenSameSlugSameEntity_ShouldNotThrowConflict()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        string slug = TestConstants.Category.ValidSlug;
        CategoryEntity category = CategoryFactory.Create(contentType.Id, TestConstants.Category.ValidName, slug);

        var command = new AdminUpdateCategoryCommand(
            Id: category.Id.ToString(),
            Name: "Updated Name",
            Slug: slug,
            Description: TestConstants.Category.ValidDescription,
            IsGossip: false,
            IsExclusive: false,
            IsDefaultForLyrics: false
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetBySlug(slug, category);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync<ConflictException>();
    }

    #endregion

    #region Exclusive Mutex

    [Fact]
    public async Task Handle_WithIsExclusive_ShouldUnsetCurrentExclusive()
    {
        // Arrange
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity category = CategoryFactory.Create(videoType);
        CategoryEntity currentExclusive = CategoryFactory.Create(videoType);
        currentExclusive.SetExclusive();

        var command = new AdminUpdateCategoryCommand(
            Id: category.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: TestConstants.Category.ValidSlug,
            Description: TestConstants.Category.ValidDescription,
            IsGossip: false,
            IsExclusive: true,
            IsDefaultForLyrics: false
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetBySlug(TestConstants.Category.ValidSlug, null);
        _categoryRepositoryMock.SetupGetExclusiveCategory(currentExclusive);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        currentExclusive.IsExclusive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithIsExclusive_SameCategory_ShouldNotClearSelf()
    {
        // Arrange
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity category = CategoryFactory.Create(videoType);
        category.SetExclusive();

        var command = new AdminUpdateCategoryCommand(
            Id: category.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: TestConstants.Category.ValidSlug,
            Description: TestConstants.Category.ValidDescription,
            IsGossip: false,
            IsExclusive: true,
            IsDefaultForLyrics: false
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetBySlug(TestConstants.Category.ValidSlug, null);
        _categoryRepositoryMock.SetupGetExclusiveCategory(category);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        category.IsExclusive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithIsExclusiveFalse_ShouldNotQueryExclusive()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);

        var command = new AdminUpdateCategoryCommand(
            Id: category.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: TestConstants.Category.ValidSlug,
            Description: TestConstants.Category.ValidDescription,
            IsGossip: false,
            IsExclusive: false,
            IsDefaultForLyrics: false
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetBySlug(TestConstants.Category.ValidSlug, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(x => x.GetExclusiveCategoryAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RenamingTheLyricsDefault_ShouldKeepTheFlagSet()
    {
        // Arrange
        ContentTypeEntity lyricsType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Lyrics));
        CategoryEntity category = CategoryFactory.Create(lyricsType);
        category.SetDefaultForLyrics();

        var command = new AdminUpdateCategoryCommand(
            Id: category.Id.ToString(),
            Name: TestConstants.Category.AnotherValidName,
            Slug: TestConstants.Category.ValidSlug,
            Description: TestConstants.Category.ValidDescription,
            IsGossip: false,
            IsExclusive: false,
            IsDefaultForLyrics: true
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetBySlug(TestConstants.Category.ValidSlug, null);
        _categoryRepositoryMock.SetupGetDefaultLyricsCategory(category);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        category.IsDefaultForLyrics.Should().BeTrue();
        category.Name.Should().Be(TestConstants.Category.AnotherValidName);
    }

    [Fact]
    public async Task Handle_WithIsDefaultForLyrics_ShouldUnsetCurrentDefault()
    {
        // Arrange
        ContentTypeEntity lyricsType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Lyrics));
        CategoryEntity category = CategoryFactory.Create(lyricsType);
        CategoryEntity currentDefault = CategoryFactory.Create(lyricsType);
        currentDefault.SetDefaultForLyrics();

        var command = new AdminUpdateCategoryCommand(
            Id: category.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: TestConstants.Category.ValidSlug,
            Description: TestConstants.Category.ValidDescription,
            IsGossip: false,
            IsExclusive: false,
            IsDefaultForLyrics: true
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetBySlug(TestConstants.Category.ValidSlug, null);
        _categoryRepositoryMock.SetupGetDefaultLyricsCategory(currentDefault);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        currentDefault.IsDefaultForLyrics.Should().BeFalse();
        category.IsDefaultForLyrics.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithIsDefaultForLyricsFalse_ShouldNotQueryCurrentDefault()
    {
        // Arrange
        ContentTypeEntity lyricsType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Lyrics));
        CategoryEntity category = CategoryFactory.Create(lyricsType);

        var command = new AdminUpdateCategoryCommand(
            Id: category.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: TestConstants.Category.ValidSlug,
            Description: TestConstants.Category.ValidDescription,
            IsGossip: false,
            IsExclusive: false,
            IsDefaultForLyrics: false
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetBySlug(TestConstants.Category.ValidSlug, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(
            x => x.GetDefaultLyricsCategoryAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WithIsDefaultForLyrics_NonLyricsContentType_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentTypeEntity articleType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Article));
        CategoryEntity category = CategoryFactory.Create(articleType);

        var command = new AdminUpdateCategoryCommand(
            Id: category.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: TestConstants.Category.ValidSlug,
            Description: TestConstants.Category.ValidDescription,
            IsGossip: false,
            IsExclusive: false,
            IsDefaultForLyrics: true
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetBySlug(TestConstants.Category.ValidSlug, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WithIsDefaultForLyrics_InactiveCategory_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentTypeEntity lyricsType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Lyrics));
        CategoryEntity category = CategoryFactory.Create(lyricsType);
        category.Deactivate();

        var command = new AdminUpdateCategoryCommand(
            Id: category.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: TestConstants.Category.ValidSlug,
            Description: TestConstants.Category.ValidDescription,
            IsGossip: false,
            IsExclusive: false,
            IsDefaultForLyrics: true
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetBySlug(TestConstants.Category.ValidSlug, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WithIsExclusive_NonVideoContentType_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentTypeEntity articleType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Article));
        CategoryEntity category = CategoryFactory.Create(articleType);

        var command = new AdminUpdateCategoryCommand(
            Id: category.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: TestConstants.Category.ValidSlug,
            Description: TestConstants.Category.ValidDescription,
            IsGossip: false,
            IsExclusive: true,
            IsDefaultForLyrics: false
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetBySlug(TestConstants.Category.ValidSlug, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WithIsExclusive_WhenCategoryIsInactive_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity inactive = CategoryFactory.CreateInactive(contentType.Id);

        var command = new AdminUpdateCategoryCommand(
            Id: inactive.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: TestConstants.Category.ValidSlug,
            Description: TestConstants.Category.ValidDescription,
            IsGossip: false,
            IsExclusive: true,
            IsDefaultForLyrics: false
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(inactive);
        _categoryRepositoryMock.SetupGetBySlug(TestConstants.Category.ValidSlug, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        var command = new AdminUpdateCategoryCommand(
            Id: nonExistentId.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: TestConstants.Category.ValidSlug,
            Description: TestConstants.Category.ValidDescription,
            IsGossip: false,
            IsExclusive: false,
            IsDefaultForLyrics: false
        );

        _categoryRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenSlugConflictsWithDifferentEntity_ShouldThrowConflictException()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        string conflictingSlug = TestConstants.Category.AnotherValidSlug;

        var command = new AdminUpdateCategoryCommand(
            Id: category.Id.ToString(),
            Name: TestConstants.Category.ValidName,
            Slug: conflictingSlug,
            Description: TestConstants.Category.ValidDescription,
            IsGossip: false,
            IsExclusive: false,
            IsDefaultForLyrics: false
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        CategoryEntity otherCategory = CategoryFactory.Create(contentType.Id, "Other Name", conflictingSlug);
        _categoryRepositoryMock.SetupGetBySlug(conflictingSlug, otherCategory);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
