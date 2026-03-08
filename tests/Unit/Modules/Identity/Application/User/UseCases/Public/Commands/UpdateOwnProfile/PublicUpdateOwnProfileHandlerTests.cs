using _116.Core.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.Contracts;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile;

/// <summary>
/// Unit tests for <see cref="PublicUpdateOwnProfileHandler"/>.
/// </summary>
public class PublicUpdateOwnProfileHandlerTests : BaseHandlerTest
{
    private readonly Mock<IPublicUpdateProfileAuthFactory> _authFactoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicUpdateOwnProfileHandler _handler;

    public PublicUpdateOwnProfileHandlerTests()
    {
        _authFactoryMock = new Mock<IPublicUpdateProfileAuthFactory>();
        _fileRepositoryMock = MockFileRepository.Create();

        _handler = new PublicUpdateOwnProfileHandler(_authFactoryMock.Object, _fileRepositoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnUpdatedProfile()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        string newUserName = "newusername";

        PublicUpdateOwnProfileCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            Email: null,
            UserName: newUserName,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        PublicUpdateProfileAuthData authData = new(User: user);

        _authFactoryMock
            .Setup(x =>
                x.UpdateProfileAsync(
                    user.Id,
                    sessionId,
                    null,
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
        PublicUpdateOwnProfileResult result = await _handler.Handle(command, CancellationToken.None);

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
        string newUserName = "newusername";

        PublicUpdateOwnProfileCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            Email: null,
            UserName: newUserName,
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        PublicUpdateProfileAuthData authData = new(User: user);

        _authFactoryMock
            .Setup(x =>
                x.UpdateProfileAsync(
                    user.Id,
                    sessionId,
                    null,
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
                    null,
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

        PublicUpdateOwnProfileCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            Email: null,
            UserName: "newusername",
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        PublicUpdateProfileAuthData authData = new(User: user);

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

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        PublicUpdateOwnProfileCommand command = new(
            UserId: userId,
            SessionId: sessionId,
            Email: null,
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
    public async Task Handle_WhenEmailAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        PublicUpdateOwnProfileCommand command = new(
            UserId: userId,
            SessionId: sessionId,
            Email: "existing@example.com",
            UserName: null,
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
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new ConflictException("Email already exists."));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenUserNameAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        PublicUpdateOwnProfileCommand command = new(
            UserId: userId,
            SessionId: sessionId,
            Email: null,
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
        using CancellationTokenSource cts = new();

        PublicUpdateOwnProfileCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            Email: null,
            UserName: "newusername",
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        PublicUpdateProfileAuthData authData = new(User: user);

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
            x => x.UpdateProfileAsync(user.Id, sessionId, null, "newusername", null, null, null, null, cts.Token),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToFileRepository()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        Guid sessionId = Guid.NewGuid();
        using CancellationTokenSource cts = new();

        PublicUpdateOwnProfileCommand command = new(
            UserId: user.Id,
            SessionId: sessionId,
            Email: null,
            UserName: "newusername",
            CountryName: null,
            PartialPhoneNumber: null,
            CountryIsoCode: null,
            CountryDialCode: null
        );

        PublicUpdateProfileAuthData authData = new(User: user);

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
