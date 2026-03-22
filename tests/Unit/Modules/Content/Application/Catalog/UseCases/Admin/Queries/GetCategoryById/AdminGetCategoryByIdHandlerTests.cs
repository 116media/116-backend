using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCategoryById;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetCategoryById;

/// <summary>
/// Unit tests for <see cref="AdminGetCategoryByIdHandler"/>.
/// </summary>
public class AdminGetCategoryByIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly AdminGetCategoryByIdHandler _handler;

    public AdminGetCategoryByIdHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _handler = new AdminGetCategoryByIdHandler(_categoryRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenCategoryFound_ShouldReturnDto()
    {
        // Arrange
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        CategoryEntity category = CategoryFactory.CreateDefault(contentType.Id);

        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        var query = new AdminGetCategoryByIdQuery(Id: category.Id);

        // Act
        AdminGetCategoryByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().NotBeNull();
        result.Category.Id.Should().Be(category.Id);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _categoryRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        var query = new AdminGetCategoryByIdQuery(Id: nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
