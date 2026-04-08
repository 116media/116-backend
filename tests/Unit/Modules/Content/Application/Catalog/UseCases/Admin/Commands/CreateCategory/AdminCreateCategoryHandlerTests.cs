using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;
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

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;

/// <summary>
/// Unit tests for <see cref="AdminCreateCategoryHandler"/>.
/// </summary>
public class AdminCreateCategoryHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreateCategoryHandler _handler;

    public AdminCreateCategoryHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminCreateCategoryHandler(
            _lookupRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateAndReturnCategory()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        string name = TestConstants.Content.Category.ValidName;
        string slug = TestConstants.Content.Category.ValidSlug;

        var command = new AdminCreateCategoryCommand(
            ContentTypeId: contentType.Id.ToString(),
            Name: name,
            Slug: slug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsFree: false
        );

        _lookupRepositoryMock.SetupGetContentTypeByIdOrThrow(contentType);
        _categoryRepositoryMock.SetupGetBySlug(slug, null);

        // Use It.IsAny<Guid> because the handler creates the entity with a new Guid internally
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
        string slug = TestConstants.Content.Category.ValidSlug;

        var command = new AdminCreateCategoryCommand(
            ContentTypeId: contentType.Id.ToString(),
            Name: TestConstants.Content.Category.ValidName,
            Slug: slug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsFree: true
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
        string slug = TestConstants.Content.Category.ValidSlug;

        var command = new AdminCreateCategoryCommand(
            ContentTypeId: contentType.Id.ToString(),
            Name: TestConstants.Content.Category.ValidName,
            Slug: slug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsFree: false
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

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenContentTypeNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        var command = new AdminCreateCategoryCommand(
            ContentTypeId: nonExistentId.ToString(),
            Name: TestConstants.Content.Category.ValidName,
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsFree: false
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
        const string slug = TestConstants.Content.Category.ValidSlug;

        var command = new AdminCreateCategoryCommand(
            ContentTypeId: contentType.Id.ToString(),
            Name: TestConstants.Content.Category.ValidName,
            Slug: slug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsFree: false
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
        string slug = TestConstants.Content.Category.ValidSlug;

        var command = new AdminCreateCategoryCommand(
            ContentTypeId: contentType.Id.ToString(),
            Name: TestConstants.Content.Category.ValidName,
            Slug: slug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsFree: false
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
