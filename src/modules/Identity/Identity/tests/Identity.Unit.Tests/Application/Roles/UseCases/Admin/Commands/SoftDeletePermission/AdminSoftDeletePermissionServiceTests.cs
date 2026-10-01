using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Roles.UseCases.Admin.Commands.SoftDeletePermission;

/// <summary>
/// Unit tests for <see cref="AdminSoftDeletePermissionService"/>: the load, the guard and the
/// clocked deletion stamp.
/// </summary>
public class AdminSoftDeletePermissionServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 30, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly AdminSoftDeletePermissionService _service;

    public AdminSoftDeletePermissionServiceTests()
    {
        _permissionRepositoryMock = MockPermissionRepository.Create();
        _service = new AdminSoftDeletePermissionService(
            _permissionRepositoryMock.Object,
            TestErrorsFactory.CreateIdentityI18n(),
            new FakeTimeProvider(Now)
        );
    }

    [Fact]
    public async Task SoftDeleteAsync_WithActivePermission_ShouldStampTheDeletionFromTheClock()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateDefault();
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        PermissionEntity result = await _service.SoftDeleteAsync(permission.Id, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(permission);
        permission.IsDeleted.Should().BeTrue();
        permission.IsActive.Should().BeFalse();
        permission.DeletedAt.Should().Be(Now.UtcDateTime);
    }

    [Fact]
    public async Task SoftDeleteAsync_WhenPermissionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var permissionId = Guid.NewGuid();
        _permissionRepositoryMock.SetupGetByIdOrThrowNotFound(permissionId);

        // Act
        Func<Task> act = async () => await _service.SoftDeleteAsync(permissionId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SoftDeleteAsync_WhenPermissionAlreadyDeleted_ShouldThrowConflictException()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateDeleted();
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        Func<Task> act = async () => await _service.SoftDeleteAsync(permission.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task SoftDeleteAsync_WithCancellationToken_ShouldPassToTheRepository()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateDefault();
        using CancellationTokenSource cts = new();
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        await _service.SoftDeleteAsync(permission.Id, cts.Token);

        // Assert
        _permissionRepositoryMock.Verify(x => x.GetPermissionByIdOrThrowAsync(permission.Id, cts.Token), Times.Once);
    }
}
