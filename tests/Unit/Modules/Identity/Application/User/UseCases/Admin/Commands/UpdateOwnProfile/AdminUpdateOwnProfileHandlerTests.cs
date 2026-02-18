using _116.Core.Application.Shared.Repositories;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile.Contracts;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile;

/// <summary>
/// Unit tests for <see cref="AdminUpdateOwnProfileHandler"/>.
/// </summary>
public class AdminUpdateOwnProfileHandlerTests : BaseHandlerTest
{
    private readonly Mock<IAdminUpdateProfileAuthFactory> _authFactoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminUpdateOwnProfileHandler _handler;

    public AdminUpdateOwnProfileHandlerTests()
    {
        _authFactoryMock = new Mock<IAdminUpdateProfileAuthFactory>();
        _fileRepositoryMock = MockFileRepository.Create();

        _handler = new AdminUpdateOwnProfileHandler(_authFactoryMock.Object, _fileRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnUpdatedProfile()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        string newUserName = "newadminuser";
        List<RoleDto> roles = [AuthTestHelpers.CreateRoleDto(description: "Admin role")];
        List<PermissionDto> permissions = [];

        AdminUpdateOwnProfileCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            UserName: newUserName,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        AdminUpdateProfileAuthData authData = new(user, roles, permissions);

        _authFactoryMock
            .Setup(x =>
                x.UpdateProfileAsync(
                    user.Id,
                    sessionId,
                    newUserName,
                    null,
                    null,
                    null,
                    null,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(authData);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        AdminUpdateOwnProfileResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldCallAuthFactoryUpdateProfile()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        string newUserName = "newadminuser";
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];

        AdminUpdateOwnProfileCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            UserName: newUserName,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        AdminUpdateProfileAuthData authData = new(user, roles, permissions);

        _authFactoryMock
            .Setup(x =>
                x.UpdateProfileAsync(
                    user.Id,
                    sessionId,
                    newUserName,
                    null,
                    null,
                    null,
                    null,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(authData);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authFactoryMock.Verify(
            x =>
                x.UpdateProfileAsync(
                    user.Id,
                    sessionId,
                    newUserName,
                    null,
                    null,
                    null,
                    null,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldFetchAvatarFile()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];

        AdminUpdateOwnProfileCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            UserName: "newadminuser",
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        AdminUpdateProfileAuthData authData = new(user, roles, permissions);

        _authFactoryMock
            .Setup(x =>
                x.UpdateProfileAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(authData);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _fileRepositoryMock.Verify(
            x => x.GetAvatarFileAsync(user.AvatarFileId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithPhoneNumber_ShouldPassAllFields()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        string newUserName = "newadminuser";
        string countryName = "United States";
        string countryIsoCode = "US";
        string countryDialCode = "+1";
        string partialPhoneNumber = "5551234567";
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];

        AdminUpdateOwnProfileCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            UserName: newUserName,
            CountryName: countryName,
            PartialPhoneNumber: partialPhoneNumber,
            CountryIsoCode: countryIsoCode,
            CountryDialCode: countryDialCode
        );

        AdminUpdateProfileAuthData authData = new(user, roles, permissions);

        _authFactoryMock
            .Setup(x =>
                x.UpdateProfileAsync(
                    user.Id,
                    sessionId,
                    newUserName,
                    countryName,
                    countryIsoCode,
                    countryDialCode,
                    partialPhoneNumber,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(authData);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authFactoryMock.Verify(
            x =>
                x.UpdateProfileAsync(
                    user.Id,
                    sessionId,
                    newUserName,
                    countryName,
                    countryIsoCode,
                    countryDialCode,
                    partialPhoneNumber,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        AdminUpdateOwnProfileCommand command = new(
            UserId: userId,
            SessionId: sessionId,
            UserName: "newusername",
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        _authFactoryMock
            .Setup(x =>
                x.UpdateProfileAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new NotFoundException("User not found."));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserNameAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        AdminUpdateOwnProfileCommand command = new(
            UserId: userId,
            SessionId: sessionId,
            UserName: "existinguser",
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        _authFactoryMock
            .Setup(x =>
                x.UpdateProfileAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new ConflictException("Username already exists."));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthFactory()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];
        using CancellationTokenSource cts = new();

        AdminUpdateOwnProfileCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            UserName: "newusername",
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        AdminUpdateProfileAuthData authData = new(user, roles, permissions);

        _authFactoryMock
            .Setup(x =>
                x.UpdateProfileAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(authData);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authFactoryMock.Verify(
            x => x.UpdateProfileAsync(user.Id, sessionId, "newusername", null, null, null, null, cts.Token),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToFileRepository()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];
        using CancellationTokenSource cts = new();

        AdminUpdateOwnProfileCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            UserName: "newusername",
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        AdminUpdateProfileAuthData authData = new(user, roles, permissions);

        _authFactoryMock
            .Setup(x =>
                x.UpdateProfileAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(authData);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _fileRepositoryMock.Verify(x => x.GetAvatarFileAsync(user.AvatarFileId, cts.Token), Times.Once);
    }

    #endregion
}
