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
/// Unit tests for <see cref="GetPackageByIdHandler"/>.
/// </summary>
public class GetPackageByIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IPackageRepository> _packageRepositoryMock;
    private readonly GetPackageByIdHandler _handler;

    public GetPackageByIdHandlerTests()
    {
        _packageRepositoryMock = MockPackageRepository.Create();
        _handler = new GetPackageByIdHandler(_packageRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenPackageFound_ShouldReturnDtoWithSlots()
    {
        // Arrange
        PackageEntity package = PackageFactory.CreateDefault();
        _packageRepositoryMock.SetupGetByIdWithSlotsOrThrow(package);

        var query = new GetPackageByIdQuery(Id: package.Id);

        // Act
        GetPackageByIdResult result = await _handler.Handle(query, CancellationToken.None);

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
        Guid nonExistentId = Guid.NewGuid();
        _packageRepositoryMock.SetupGetByIdWithSlotsOrThrowNotFound(nonExistentId);

        var query = new GetPackageByIdQuery(Id: nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
