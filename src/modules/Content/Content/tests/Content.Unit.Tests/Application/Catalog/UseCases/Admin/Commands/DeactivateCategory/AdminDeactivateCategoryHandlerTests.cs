using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivateCategory;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Catalog.UseCases.Admin.Commands.DeactivateCategory;

/// <summary>
/// Unit tests for <see cref="AdminDeactivateCategoryHandler"/>.
/// </summary>
public class AdminDeactivateCategoryHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly AdminDeactivateCategoryHandler _handler;

    public AdminDeactivateCategoryHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileStorageMock = MockFileStorageService.Create();
        _handler = new AdminDeactivateCategoryHandler(
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            CreateCategoryDtoService(_fileStorageMock.Object),
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenActive_ShouldDeactivateAndReturnDto()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity active = CategoryFactory.Create(contentType.Id);
        var command = new AdminDeactivateCategoryCommand(Id: active.Id.ToString());

        _categoryRepositoryMock.SetupGetByIdOrThrow(active);

        // Act
        AdminDeactivateCategoryResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        active.IsActive.Should().BeFalse();
        result.Category.Id.Should().Be(active.Id);
        result.Category.IsActive.Should().BeFalse();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ShouldReloadAfterCommit()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity active = CategoryFactory.Create(contentType.Id);
        var command = new AdminDeactivateCategoryCommand(Id: active.Id.ToString());

        _categoryRepositoryMock.SetupGetByIdOrThrow(active);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(
            x => x.GetByIdOrThrowAsync(active.Id, It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenAlreadyInactive_ShouldThrowConflictException()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity inactive = CategoryFactory.CreateInactive(contentType.Id);
        var command = new AdminDeactivateCategoryCommand(Id: inactive.Id.ToString());

        _categoryRepositoryMock.SetupGetByIdOrThrow(inactive);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        inactive.IsActive.Should().BeFalse();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new AdminDeactivateCategoryCommand(Id: nonExistentId.ToString());

        _categoryRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
