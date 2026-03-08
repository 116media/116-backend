using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.Login;

/// <summary>
/// Unit tests for <see cref="PublicLoginAuthFactory"/>.
/// </summary>
public class PublicLoginAuthFactoryTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly PublicLoginAuthFactory _factory;

    public PublicLoginAuthFactoryTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _passwordServiceMock = MockPasswordService.Create();

        _factory = new PublicLoginAuthFactory(_authRepositoryMock.Object, _passwordServiceMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnAuthData()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();

        // Act
        PublicLoginAuthData result = await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFetchUserByCredentials()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();

        // Act
        await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesAndPermissionsByCredentialsOrThrow(credentials, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldVerifyPassword()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();

        // Act
        await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        _passwordServiceMock.Verify(x => x.Verify(password, user.PasswordHash), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WithUsername_ShouldWork()
    {
        // Arrange
        string credentials = "testuser";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();

        // Act
        PublicLoginAuthData result = await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task AuthenticateAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string credentials = "nonexistent@example.com";
        string password = "ValidPassword123!";

        _authRepositoryMock.SetupGetUserWithRolesByCredentialsNotFound(credentials);

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AuthenticateAsync_WhenPasswordInvalid_ShouldThrowAuthenticationException()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "WrongPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);
        _passwordServiceMock.SetupVerifyReturnsFalse();

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task AuthenticateAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();

        // Act
        await _factory.AuthenticateAsync(credentials, password, cts.Token);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesAndPermissionsByCredentialsOrThrow(credentials, cts.Token),
            Times.Once
        );
    }

    #endregion
}
