using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;
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

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;

/// <summary>
/// Unit tests for <see cref="UpdateCategoryHandler"/>.
/// </summary>
public class UpdateCategoryHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly UpdateCategoryHandler _handler;

    public UpdateCategoryHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new UpdateCategoryHandler(_categoryRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldUpdateAndReturnCategory()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        string newName = TestConstants.Content.Category.AnotherValidName;
        string newSlug = TestConstants.Content.Category.AnotherValidSlug;

        var command = new UpdateCategoryCommand(
            Id: category.Id,
            Name: newName,
            Slug: newSlug,
            Description: "Updated description"
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetBySlug(newSlug, null);

        CategoryEntity reloaded = CategoryFactory.Create(contentType.Id, newName, newSlug);
        _categoryRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category)
            .Verifiable();

        // Act
        UpdateCategoryResult result = await _handler.Handle(command, CancellationToken.None);

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
        string slug = TestConstants.Content.Category.ValidSlug;
        CategoryEntity category = CategoryFactory.Create(
            contentType.Id,
            TestConstants.Content.Category.ValidName,
            slug
        );

        var command = new UpdateCategoryCommand(Id: category.Id, Name: "Updated Name", Slug: slug, Description: null);

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        // Slug belongs to the same entity — no conflict
        _categoryRepositoryMock.SetupGetBySlug(slug, category);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync<ConflictException>();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        var command = new UpdateCategoryCommand(
            Id: nonExistentId,
            Name: TestConstants.Content.Category.ValidName,
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: null
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
        string conflictingSlug = TestConstants.Content.Category.AnotherValidSlug;

        var command = new UpdateCategoryCommand(
            Id: category.Id,
            Name: TestConstants.Content.Category.ValidName,
            Slug: conflictingSlug,
            Description: null
        );

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        // Different entity uses the same slug
        CategoryEntity otherCategory = CategoryFactory.Create(contentType.Id, "Other Name", conflictingSlug);
        _categoryRepositoryMock.SetupGetBySlug(conflictingSlug, otherCategory);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
