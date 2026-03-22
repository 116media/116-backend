using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById;

/// <summary>
/// Unit tests for <see cref="AdminGetPackageByIdHandler"/>.
/// </summary>
public class AdminGetPackageByIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IPackageRepository> _packageRepositoryMock;
    private readonly AdminGetPackageByIdHandler _handler;

    public AdminGetPackageByIdHandlerTests()
    {
        _packageRepositoryMock = MockPackageRepository.Create();
        _handler = new AdminGetPackageByIdHandler(_packageRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenPackageFound_ShouldReturnDtoWithSlots()
    {
        // Arrange
        PackageEntity package = PackageFactory.CreateDefault();
        _packageRepositoryMock.SetupGetByIdWithSlotsOrThrow(package);

        var query = new AdminGetPackageByIdQuery(Id: package.Id);

        // Act
        AdminGetPackageByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Package.Should().NotBeNull();
        result.Package.Id.Should().Be(package.Id);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPackageNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _packageRepositoryMock.SetupGetByIdWithSlotsOrThrowNotFound(nonExistentId);

        var query = new AdminGetPackageByIdQuery(Id: nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
