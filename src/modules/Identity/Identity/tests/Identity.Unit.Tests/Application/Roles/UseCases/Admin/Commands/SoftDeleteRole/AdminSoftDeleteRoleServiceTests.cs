using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole;

/// <summary>
/// Unit tests for <see cref="AdminSoftDeleteRoleService"/>: the load, the guard and the clocked
/// deletion stamp.
/// </summary>
public class AdminSoftDeleteRoleServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 30, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly AdminSoftDeleteRoleService _service;

    public AdminSoftDeleteRoleServiceTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _service = new AdminSoftDeleteRoleService(
            _roleRepositoryMock.Object,
            TestErrorsFactory.CreateIdentityI18n(),
            new FakeTimeProvider(Now)
        );
    }

    [Fact]
    public async Task SoftDeleteAsync_WithActiveRole_ShouldStampTheDeletionFromTheClock()
    {
        // Arrange
        RoleEntity role = RoleFactory.CreateDefault();
        _roleRepositoryMock.SetupGetByIdOrThrow(role);

        // Act
        RoleEntity result = await _service.SoftDeleteAsync(role.Id, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(role);
        role.IsDeleted.Should().BeTrue();
        role.IsActive.Should().BeFalse();
        role.DeletedAt.Should().Be(Now.UtcDateTime);
    }

    [Fact]
    public async Task SoftDeleteAsync_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleRepositoryMock.SetupGetByIdOrThrowNotFound(roleId);

        // Act
        Func<Task> act = async () => await _service.SoftDeleteAsync(roleId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SoftDeleteAsync_WhenRoleAlreadyDeleted_ShouldThrowConflictException()
    {
        // Arrange
        RoleEntity role = RoleFactory.CreateDeleted();
        _roleRepositoryMock.SetupGetByIdOrThrow(role);

        // Act
        Func<Task> act = async () => await _service.SoftDeleteAsync(role.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task SoftDeleteAsync_WithCancellationToken_ShouldPassToTheRepository()
    {
        // Arrange
        RoleEntity role = RoleFactory.CreateDefault();
        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdOrThrow(role);

        // Act
        await _service.SoftDeleteAsync(role.Id, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdOrThrowAsync(role.Id, cts.Token), Times.Once);
    }
}
