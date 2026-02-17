using _116.Identity.Application.Roles.UseCases.Admin.Queries.GetPermissionById;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Queries.GetPermissionById;

/// <summary>
/// Unit tests for <see cref="AdminGetPermissionByIdHandler"/>.
/// </summary>
public class AdminGetPermissionByIdHandlerTests : BaseHandlerTest
{
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly AdminGetPermissionByIdHandler _handler;

    public AdminGetPermissionByIdHandlerTests()
    {
        _permissionRepositoryMock = MockPermissionRepository.Create();

        _handler = new AdminGetPermissionByIdHandler(_permissionRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidPermissionId_ShouldReturnPermission()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction,
            TestConstants.Permission.ValidDescription
        );

        AdminGetPermissionByIdQuery query = new(PermissionId: permission.Id.ToString());

        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        AdminGetPermissionByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Permission.Should().NotBeNull();
        result.Permission.Id.Should().Be(permission.Id);
        result.Permission.Resource.Should().Be(TestConstants.Permission.ValidResource);
        result.Permission.Action.Should().Be(TestConstants.Permission.ValidAction);
        result.Permission.Description.Should().Be(TestConstants.Permission.ValidDescription);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenPermissionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentPermissionId = Guid.NewGuid();
        AdminGetPermissionByIdQuery query = new(PermissionId: nonExistentPermissionId.ToString());

        _permissionRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentPermissionId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DTO Mapping Tests

    [Fact]
    public async Task Handle_WithActivePermission_ShouldMapIsActiveCorrectly()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        AdminGetPermissionByIdQuery query = new(PermissionId: permission.Id.ToString());

        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        AdminGetPermissionByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Permission.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithInactivePermission_ShouldMapIsActiveCorrectly()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        permission.Deactivate();

        AdminGetPermissionByIdQuery query = new(PermissionId: permission.Id.ToString());

        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        AdminGetPermissionByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Permission.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithDeletedPermission_ShouldMapIsDeletedCorrectly()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        permission.SoftDelete();

        AdminGetPermissionByIdQuery query = new(PermissionId: permission.Id.ToString());

        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        AdminGetPermissionByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Permission.IsDeleted.Should().BeTrue();
        result.Permission.DeletedAt.Should().NotBeNull();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        AdminGetPermissionByIdQuery query = new(PermissionId: permission.Id.ToString());

        using CancellationTokenSource cts = new();
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _permissionRepositoryMock.Verify(x => x.GetPermissionByIdOrThrowAsync(permission.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithLowercaseGuid_ShouldParseCorrectly()
    {
        // Arrange
        Guid permissionId = Guid.NewGuid();
        PermissionEntity permission = PermissionFactory.CreateWithId(
            permissionId,
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        AdminGetPermissionByIdQuery query = new(PermissionId: permissionId.ToString().ToLowerInvariant());

        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        AdminGetPermissionByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Permission.Id.Should().Be(permissionId);
    }

    [Fact]
    public async Task Handle_WithUppercaseGuid_ShouldParseCorrectly()
    {
        // Arrange
        Guid permissionId = Guid.NewGuid();
        PermissionEntity permission = PermissionFactory.CreateWithId(
            permissionId,
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );

        AdminGetPermissionByIdQuery query = new(PermissionId: permissionId.ToString().ToUpperInvariant());

        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        AdminGetPermissionByIdResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Permission.Id.Should().Be(permissionId);
    }

    #endregion
}
