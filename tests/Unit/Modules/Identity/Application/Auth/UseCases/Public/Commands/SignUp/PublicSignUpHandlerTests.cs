using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp.Contracts;
using _116.Identity.Application.Session.Factories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SignUp;

/// <summary>
/// Unit tests for <see cref="PublicSignUpHandler"/>.
/// </summary>
public class PublicSignUpHandlerTests : BaseHandlerTest
{
    private readonly Mock<IPublicSignUpAuthFactory> _authFactoryMock;
    private readonly Mock<ISessionFactory> _sessionFactoryMock;
    private readonly PublicSignUpHandler _handler;

    public PublicSignUpHandlerTests()
    {
        _authFactoryMock = new Mock<IPublicSignUpAuthFactory>();
        _sessionFactoryMock = new Mock<ISessionFactory>();
        _handler = new PublicSignUpHandler(_authFactoryMock.Object, _sessionFactoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnAuthenticationResult()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "Password123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RolePermissionEntity> permissions = [];

        PublicSignUpCommand command = new(Email: email, UserName: userName, Password: password);
        PublicSignUpAuthData authData = new(User: user, UserPermissions: permissions);
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        _authFactoryMock
            .Setup(x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);

        // Act
        PublicSignUpResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AuthenticationResult.Should().NotBeNull();
        result.VerificationRequired.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldRegisterUser()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "Password123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RolePermissionEntity> permissions = [];

        PublicSignUpCommand command = new(Email: email, UserName: userName, Password: password);
        PublicSignUpAuthData authData = new(User: user, UserPermissions: permissions);
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        _authFactoryMock
            .Setup(x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authFactoryMock.Verify(
            x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateSession()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "Password123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RolePermissionEntity> permissions = [];

        PublicSignUpCommand command = new(Email: email, UserName: userName, Password: password);
        PublicSignUpAuthData authData = new(User: user, UserPermissions: permissions);
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        _authFactoryMock
            .Setup(x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _sessionFactoryMock.Verify(
            x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenEmailExists_ShouldThrowConflictException()
    {
        // Arrange
        string email = "existing@example.com";
        string userName = "newuser";
        string password = "Password123!";
        PublicSignUpCommand command = new(Email: email, UserName: userName, Password: password);

        _authFactoryMock
            .Setup(x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("Email already exists."));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenUserNameExists_ShouldThrowConflictException()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "existinguser";
        string password = "Password123!";
        PublicSignUpCommand command = new(Email: email, UserName: userName, Password: password);

        _authFactoryMock
            .Setup(x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("Username already exists."));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenRegistrationFails_ShouldNotCreateSession()
    {
        // Arrange
        string email = "existing@example.com";
        string userName = "newuser";
        string password = "Password123!";
        PublicSignUpCommand command = new(Email: email, UserName: userName, Password: password);

        _authFactoryMock
            .Setup(x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("Email already exists."));

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (ConflictException)
        {
            // Expected
        }

        // Assert
        _sessionFactoryMock.Verify(
            x =>
                x.CreateSessionAsync(
                    It.IsAny<UserEntity>(),
                    It.IsAny<List<RolePermissionEntity>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthFactory()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "Password123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RolePermissionEntity> permissions = [];
        using CancellationTokenSource cts = new();

        PublicSignUpCommand command = new(Email: email, UserName: userName, Password: password);
        PublicSignUpAuthData authData = new(User: user, UserPermissions: permissions);
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        _authFactoryMock
            .Setup(x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authFactoryMock.Verify(x => x.RegisterAsync(email, userName, password, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToSessionFactory()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "Password123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RolePermissionEntity> permissions = [];
        using CancellationTokenSource cts = new();

        PublicSignUpCommand command = new(Email: email, UserName: userName, Password: password);
        PublicSignUpAuthData authData = new(User: user, UserPermissions: permissions);
        SessionResult sessionResult = AuthTestHelpers.CreateDefaultSessionResult();

        _authFactoryMock
            .Setup(x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _sessionFactoryMock.Verify(x => x.CreateSessionAsync(user, permissions, cts.Token), Times.Once);
    }

    #endregion
}
