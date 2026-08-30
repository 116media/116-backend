using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignUp.Contracts;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Unit.Tests.Common;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SignUp;

/// <summary>
/// Unit tests for <see cref="PublicSignUpHandler"/>. Signup issues no session and no tokens:
/// the handler only registers the account and returns the created user with the
/// verification-required flag.
/// </summary>
public class PublicSignUpHandlerTests : BaseHandlerTest
{
    private readonly Mock<IPublicSignUpAuthFactory> _authFactoryMock;
    private readonly PublicSignUpHandler _handler;

    public PublicSignUpHandlerTests()
    {
        _authFactoryMock = new Mock<IPublicSignUpAuthFactory>();
        _handler = new PublicSignUpHandler(_authFactoryMock.Object, Mapper);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnMappedUserWithoutTokens()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "Password123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RolePermissionEntity> permissions = [];

        PublicSignUpCommand command = new(Email: email, UserName: userName, Password: password);
        PublicSignUpAuthData authData = new(User: user, UserPermissions: permissions);

        _authFactoryMock
            .Setup(x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);

        // Act
        PublicSignUpResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.User.Id.Should().Be(user.Id);
        result.User.Email.Should().Be(user.Email);
        result.User.UserName.Should().Be(user.UserName);
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

        _authFactoryMock
            .Setup(x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authFactoryMock.Verify(
            x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldAlwaysRequireVerification()
    {
        // Arrange
        string email = "newuser@example.com";
        string userName = "newuser";
        string password = "Password123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RolePermissionEntity> permissions = [];

        PublicSignUpCommand command = new(Email: email, UserName: userName, Password: password);
        PublicSignUpAuthData authData = new(User: user, UserPermissions: permissions);

        _authFactoryMock
            .Setup(x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);

        // Act
        PublicSignUpResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.VerificationRequired.Should().BeTrue();
        _authFactoryMock.Verify(
            x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _authFactoryMock.VerifyNoOtherCalls();
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
    public async Task Handle_WhenRegistrationFails_ShouldPropagateException()
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
        _authFactoryMock.Verify(
            x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _authFactoryMock.VerifyNoOtherCalls();
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

        _authFactoryMock
            .Setup(x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authFactoryMock.Verify(x => x.RegisterAsync(email, userName, password, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldStillReturnMappedUser()
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

        _authFactoryMock
            .Setup(x => x.RegisterAsync(email, userName, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);

        // Act
        PublicSignUpResult result = await _handler.Handle(command, cts.Token);

        // Assert
        result.User.Id.Should().Be(user.Id);
        result.VerificationRequired.Should().BeTrue();
    }

    #endregion
}
