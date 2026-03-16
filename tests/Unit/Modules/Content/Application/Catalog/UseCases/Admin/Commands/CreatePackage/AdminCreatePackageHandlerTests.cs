using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage;

/// <summary>
/// Unit tests for <see cref="AdminCreatePackageHandler"/>.
/// </summary>
public class AdminCreatePackageHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IPackageRepository> _packageRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreatePackageHandler _handler;

    public AdminCreatePackageHandlerTests()
    {
        _packageRepositoryMock = MockPackageRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminCreatePackageHandler(_packageRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_ShouldCreatePackageAndReloadWithSlots()
    {
        // Arrange
        string name = TestConstants.Content.Package.ValidName;
        decimal price = TestConstants.Content.Package.ValidFlatPriceUsd;

        var command = new AdminCreatePackageCommand(
            Name: name,
            Description: TestConstants.Content.Package.ValidDescription,
            FlatPriceUsd: price
        );

        PackageEntity created = PackageFactory.Create(name);
        // The handler creates a new entity with Guid.NewGuid(), so we match any Guid
        _packageRepositoryMock
            .Setup(x => x.GetByIdWithSlotsOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        AdminCreatePackageResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Package.Should().NotBeNull();
        result.Package.Name.Should().Be(name);

        _packageRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldCallGetByIdWithSlotsOrThrowAfterCommit()
    {
        // Arrange
        var command = new AdminCreatePackageCommand(
            Name: TestConstants.Content.Package.ValidName,
            Description: null,
            FlatPriceUsd: TestConstants.Content.Package.ZeroFlatPriceUsd
        );

        PackageEntity created = PackageFactory.Create();
        _packageRepositoryMock
            .Setup(x => x.GetByIdWithSlotsOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _packageRepositoryMock.Verify(
            x => x.GetByIdWithSlotsOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion
}
