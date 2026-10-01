using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole.Contracts;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Tests.TestData;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Roles.UseCases.Admin.Commands.SoftDeleteRole;

/// <summary>
/// Unit tests for <see cref="AdminSoftDeleteRoleHandler"/>: the deletion call, the commit and the
/// response. The guard is covered by <c>AdminSoftDeleteRoleServiceTests</c>.
/// </summary>
public class AdminSoftDeleteRoleHandlerTests : BaseHandlerTest
{
    private readonly Mock<IAdminSoftDeleteRoleService> _softDeleteRoleServiceMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminSoftDeleteRoleHandler _handler;

    public AdminSoftDeleteRoleHandlerTests()
    {
        _softDeleteRoleServiceMock = new Mock<IAdminSoftDeleteRoleService>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminSoftDeleteRoleHandler(_softDeleteRoleServiceMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    private (RoleEntity Role, AdminSoftDeleteRoleCommand Command) ArrangeDeletion()
    {
        RoleEntity role = RoleFactory.CreateDefault();
        role.SoftDelete(DateTime.UtcNow);

        _softDeleteRoleServiceMock
            .Setup(x => x.SoftDeleteAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        return (role, new AdminSoftDeleteRoleCommand(RoleId: role.Id.ToString()));
    }

    [Fact]
    public async Task Handle_WithActiveRole_ShouldReturnTheDeletedRole()
    {
        // Arrange
        (RoleEntity role, AdminSoftDeleteRoleCommand command) = ArrangeDeletion();

        // Act
        AdminSoftDeleteRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Role.Id.Should().Be(role.Id);
        result.Role.Name.Should().Be(TestConstants.Role.ValidName);
        result.Role.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldCommitUnitOfWork()
    {
        // Arrange
        (_, AdminSoftDeleteRoleCommand command) = ArrangeDeletion();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenTheDeletionIsRefused_ShouldNotCommit()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _softDeleteRoleServiceMock
            .Setup(x => x.SoftDeleteAsync(roleId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(TestErrorsFactory.CreateIdentityI18n().User.RoleAlreadyDeleted());

        // Act
        Func<Task> act = async () =>
            await _handler.Handle(new AdminSoftDeleteRoleCommand(roleId.ToString()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToTheServiceAndUnitOfWork()
    {
        // Arrange
        (RoleEntity role, AdminSoftDeleteRoleCommand command) = ArrangeDeletion();
        using CancellationTokenSource cts = new();

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _softDeleteRoleServiceMock.Verify(x => x.SoftDeleteAsync(role.Id, cts.Token), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
    }
}
