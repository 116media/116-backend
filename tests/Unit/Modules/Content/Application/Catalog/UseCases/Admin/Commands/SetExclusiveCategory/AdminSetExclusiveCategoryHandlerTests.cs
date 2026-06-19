using _116.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
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

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory;

/// <summary>
/// Unit tests for <see cref="AdminSetExclusiveCategoryHandler"/>.
/// </summary>
public class AdminSetExclusiveCategoryHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminSetExclusiveCategoryHandler _handler;

    public AdminSetExclusiveCategoryHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminSetExclusiveCategoryHandler(
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileRepositoryMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenNoCurrentExclusive_ShouldSetExclusive()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);

        var command = new AdminSetExclusiveCategoryCommand(Id: category.Id.ToString());

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetExclusiveCategory(null);

        // Act
        AdminSetExclusiveCategoryResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().NotBeNull();
        category.IsExclusive.Should().BeTrue();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenDifferentCategoryIsExclusive_ShouldClearOldAndSetNew()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        CategoryEntity currentExclusive = CategoryFactory.Create(contentType.Id);
        currentExclusive.SetExclusive();

        var command = new AdminSetExclusiveCategoryCommand(Id: category.Id.ToString());

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetExclusiveCategory(currentExclusive);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        currentExclusive.IsExclusive.Should().BeFalse();
        category.IsExclusive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenSameCategoryIsAlreadyExclusive_ShouldNotClearSelf()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        category.SetExclusive();

        var command = new AdminSetExclusiveCategoryCommand(Id: category.Id.ToString());

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetExclusiveCategory(category);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        category.IsExclusive.Should().BeTrue();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenCategoryIsInactive_ShouldThrowBadRequestException()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity inactive = CategoryFactory.CreateInactive(contentType.Id);

        var command = new AdminSetExclusiveCategoryCommand(Id: inactive.Id.ToString());

        _categoryRepositoryMock.SetupGetByIdOrThrow(inactive);

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

        var command = new AdminSetExclusiveCategoryCommand(Id: nonExistentId.ToString());

        _categoryRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
