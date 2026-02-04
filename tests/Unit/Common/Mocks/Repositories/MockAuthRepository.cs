using System.Security.Claims;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IAuthRepository"/>.
/// </summary>
public static class MockAuthRepository
{
    /// <summary>
    /// Creates a new mock instance of IAuthRepository.
    /// </summary>
    /// <returns>A configured Mock of IAuthRepository.</returns>
    public static Mock<IAuthRepository> Create()
    {
        Mock<IAuthRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Sets up FindUserByIdOrThrow to return the specified user.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="user">The user to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupFindUserByIdOrThrow(this Mock<IAuthRepository> mock, UserEntity user)
    {
        mock.Setup(x => x.FindUserByIdOrThrow(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        return mock;
    }

    /// <summary>
    /// Sets up FindUserByIdOrThrow to throw NotFoundException.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The user ID that should throw.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupFindUserByIdOrThrowNotFound(this Mock<IAuthRepository> mock, Guid userId)
    {
        mock.Setup(x => x.FindUserByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"User with ID '{userId}' was not found."));
        return mock;
    }

    /// <summary>
    /// Sets up GetUserWithRolesByEmailOrThrow to return the specified user.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="email">The email to match.</param>
    /// <param name="user">The user to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupGetUserWithRolesByEmailOrThrow(
        this Mock<IAuthRepository> mock,
        Email email,
        UserEntity user
    )
    {
        mock.Setup(x => x.GetUserWithRolesByEmailOrThrow(email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        return mock;
    }

    /// <summary>
    /// Sets up GetUserWithRolesByEmailOrThrow to throw NotFoundException.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="email">The email that should throw.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupGetUserWithRolesByEmailOrThrowNotFound(
        this Mock<IAuthRepository> mock,
        Email email
    )
    {
        mock.Setup(x => x.GetUserWithRolesByEmailOrThrow(email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"User with email '{email}' was not found."));
        return mock;
    }

    /// <summary>
    /// Sets up GetUserWithRolesAndPermissionsByIdOrThrow to return the specified user.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="user">The user to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupGetUserWithRolesAndPermissionsById(
        this Mock<IAuthRepository> mock,
        UserEntity user
    )
    {
        mock.Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        return mock;
    }

    /// <summary>
    /// Sets up GetUserWithRolesAndPermissionsByIdOrThrow to throw NotFoundException.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The user ID that should throw.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupGetUserWithRolesAndPermissionsByIdNotFound(
        this Mock<IAuthRepository> mock,
        Guid userId
    )
    {
        mock.Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"User with ID '{userId}' was not found."));
        return mock;
    }

    /// <summary>
    /// Sets up GetUserWithRolesAndPermissionsByCredentialsOrThrow to return the specified user.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="credentials">The credentials to match.</param>
    /// <param name="user">The user to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupGetUserWithRolesAndPermissionsByCredentials(
        this Mock<IAuthRepository> mock,
        string credentials,
        UserEntity user
    )
    {
        mock.Setup(x =>
                x.GetUserWithRolesAndPermissionsByCredentialsOrThrow(credentials, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(user);
        return mock;
    }

    /// <summary>
    /// Sets up GetUserWithRolesAndPermissionsByCredentialsOrThrow to throw NotFoundException.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="credentials">The credentials that should throw.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupGetUserWithRolesAndPermissionsByCredentialsNotFound(
        this Mock<IAuthRepository> mock,
        string credentials
    )
    {
        mock.Setup(x =>
                x.GetUserWithRolesAndPermissionsByCredentialsOrThrow(credentials, It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(new NotFoundException($"User with credentials '{credentials}' was not found."));
        return mock;
    }

    /// <summary>
    /// Sets up GetUserWithSessionsByIdOrThrow to return the specified user.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="user">The user to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupGetUserWithSessionsById(this Mock<IAuthRepository> mock, UserEntity user)
    {
        mock.Setup(x => x.GetUserWithSessionsByIdOrThrow(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        return mock;
    }

    /// <summary>
    /// Sets up ExistsByEmailAsync to return the specified result.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="email">The email to check.</param>
    /// <param name="exists">Whether the email exists.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupExistsByEmail(this Mock<IAuthRepository> mock, Email email, bool exists)
    {
        mock.Setup(x => x.ExistsByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(exists);
        return mock;
    }

    /// <summary>
    /// Sets up ExistsByUserNameAsync to return the specified result.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userName">The username to check.</param>
    /// <param name="exists">Whether the username exists.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupExistsByUserName(
        this Mock<IAuthRepository> mock,
        string userName,
        bool exists
    )
    {
        mock.Setup(x => x.ExistsByUserNameAsync(userName, It.IsAny<CancellationToken>())).ReturnsAsync(exists);
        return mock;
    }

    /// <summary>
    /// Sets up ValidateUniqueCredentialsAsync to succeed (no exception).
    /// Signature: ValidateUniqueCredentialsAsync(Email email, string userName, CancellationToken ct)
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupValidateUniqueCredentialsSuccess(this Mock<IAuthRepository> mock)
    {
        mock.Setup(x =>
                x.ValidateUniqueCredentialsAsync(It.IsAny<Email>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask);
        return mock;
    }

    /// <summary>
    /// Sets up IsUserAccountActive to return true.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupIsUserAccountActiveReturnsTrue(this Mock<IAuthRepository> mock)
    {
        mock.Setup(x => x.IsUserAccountActive(It.IsAny<UserEntity>())).Returns(true);
        return mock;
    }

    /// <summary>
    /// Sets up IsUserAccountVerified to return true.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupIsUserAccountVerifiedReturnsTrue(this Mock<IAuthRepository> mock)
    {
        mock.Setup(x => x.IsUserAccountVerified(It.IsAny<UserEntity>())).Returns(true);
        return mock;
    }

    /// <summary>
    /// Sets up IsSessionValidAsync to return true.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="sessionId">The session ID to validate.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupIsSessionValid(this Mock<IAuthRepository> mock, Guid sessionId)
    {
        mock.Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        return mock;
    }

    /// <summary>
    /// Sets up IsSessionValidAsync to return true for any session ID.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupIsSessionValidReturnsTrue(this Mock<IAuthRepository> mock)
    {
        mock.Setup(x => x.IsSessionValidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        return mock;
    }

    /// <summary>
    /// Sets up GetSessionIdFromClaims to return the specified session ID.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="sessionId">The session ID to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupGetSessionIdFromClaims(this Mock<IAuthRepository> mock, Guid sessionId)
    {
        mock.Setup(x => x.GetSessionIdFromClaims(It.IsAny<ClaimsPrincipal>())).Returns(sessionId);
        return mock;
    }

    /// <summary>
    /// Sets up GetUserIdFromClaims to return the specified user ID.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The user ID to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupGetUserIdFromClaims(this Mock<IAuthRepository> mock, Guid userId)
    {
        mock.Setup(x => x.GetUserIdFromClaims(It.IsAny<ClaimsPrincipal>())).Returns(userId);
        return mock;
    }

    /// <summary>
    /// Sets up IsUserAdmin to return true.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupIsUserAdminReturnsTrue(this Mock<IAuthRepository> mock)
    {
        mock.Setup(x => x.IsUserAdmin(It.IsAny<UserEntity>())).Returns(true);
        return mock;
    }

    /// <summary>
    /// Sets up IsUserAdmin to return false.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupIsUserAdminReturnsFalse(this Mock<IAuthRepository> mock)
    {
        mock.Setup(x => x.IsUserAdmin(It.IsAny<UserEntity>())).Returns(false);
        return mock;
    }

    /// <summary>
    /// Verifies that AddAsync was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="verifyUser">Optional predicate to verify the user.</param>
    public static void VerifyAddCalled(this Mock<IAuthRepository> mock, Func<UserEntity, bool>? verifyUser = null)
    {
        if (verifyUser is not null)
        {
            mock.Verify(
                x => x.AddAsync(It.Is<UserEntity>(u => verifyUser(u)), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
        else
        {
            mock.Verify(x => x.AddAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    /// <summary>
    /// Verifies that AssignVisitorRoleAsync was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The expected user ID.</param>
    public static void VerifyAssignVisitorRoleCalled(this Mock<IAuthRepository> mock, Guid userId)
    {
        mock.Verify(x => x.AssignVisitorRoleAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Sets up GetUserWithRolesAndPermissionsByEmailOrThrow to return the specified user for any email.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="user">The user to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupGetUserWithRolesByEmail(this Mock<IAuthRepository> mock, UserEntity user)
    {
        mock.Setup(x =>
                x.GetUserWithRolesAndPermissionsByEmailOrThrow(It.IsAny<Email>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(user);
        return mock;
    }

    /// <summary>
    /// Sets up GetUserWithRolesAndPermissionsByEmailOrThrow to throw NotFoundException for any email.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="email">The email that should throw (for documentation purposes).</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupGetUserWithRolesByEmailNotFound(
        this Mock<IAuthRepository> mock,
        Email email
    )
    {
        mock.Setup(x =>
                x.GetUserWithRolesAndPermissionsByEmailOrThrow(It.IsAny<Email>(), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(new NotFoundException($"User with email '{email.Value}' was not found."));
        return mock;
    }

    /// <summary>
    /// Sets up GetUserWithRolesAndPermissionsByCredentialsOrThrow to return the specified user for any credentials.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="user">The user to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupGetUserWithRolesByCredentials(
        this Mock<IAuthRepository> mock,
        UserEntity user
    )
    {
        mock.Setup(x =>
                x.GetUserWithRolesAndPermissionsByCredentialsOrThrow(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(user);
        return mock;
    }

    /// <summary>
    /// Sets up GetUserWithRolesAndPermissionsByCredentialsOrThrow to throw NotFoundException.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="credentials">The credentials that should throw.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupGetUserWithRolesByCredentialsNotFound(
        this Mock<IAuthRepository> mock,
        string credentials
    )
    {
        mock.Setup(x =>
                x.GetUserWithRolesAndPermissionsByCredentialsOrThrow(credentials, It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(new NotFoundException($"User with credentials '{credentials}' was not found."));
        return mock;
    }

    /// <summary>
    /// Sets up IsUserAdmin to throw AuthorizationException.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IAuthRepository> SetupIsUserAdminThrowsAuthorizationException(this Mock<IAuthRepository> mock)
    {
        mock.Setup(x => x.IsUserAdmin(It.IsAny<UserEntity>()))
            .Throws(new AuthorizationException("User is not an admin."));
        return mock;
    }

    /// <summary>
    /// Sets up default behaviors for the mock.
    /// </summary>
    private static void SetupDefaults(Mock<IAuthRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        mock.Setup(x => x.AssignVisitorRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}
