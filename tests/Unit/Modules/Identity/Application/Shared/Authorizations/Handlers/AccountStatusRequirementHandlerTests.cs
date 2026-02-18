using System.Security.Claims;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Authorizations.Handlers;
using _116.Identity.Application.Shared.Authorizations.Requirements;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Authorizations.Handlers;

public class AccountStatusRequirementHandlerTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly AccountStatusRequirementHandler _handler;

    public AccountStatusRequirementHandlerTests()
    {
        _authRepositoryMock = new Mock<IAuthRepository>();
        _handler = new AccountStatusRequirementHandler(_authRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithActiveUser_ShouldSucceed()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimsConstants.IsActive, "true"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        UserEntity userEntity = UserFactory.CreateWithId(userId);
        typeof(UserEntity).GetProperty(nameof(UserEntity.IsActive))!.SetValue(userEntity, true);
        _authRepositoryMock
            .Setup(x => x.FindUserByIdOrThrow(It.Is<Guid>(id => id == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userEntity);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithVerifiedUser_ShouldSucceed()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimsConstants.IsVerified, "true"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsVerified, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        UserEntity userEntity = UserFactory.CreateWithId(userId);
        typeof(UserEntity).GetProperty(nameof(UserEntity.IsVerified))!.SetValue(userEntity, true);
        _authRepositoryMock
            .Setup(x => x.FindUserByIdOrThrow(It.Is<Guid>(id => id == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userEntity);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithInactiveUser_ShouldNotSucceed()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimsConstants.IsActive, "true"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        UserEntity userEntity = UserFactory.CreateWithId(userId);
        typeof(UserEntity).GetProperty(nameof(UserEntity.IsActive))!.SetValue(userEntity, false);
        _authRepositoryMock
            .Setup(x => x.FindUserByIdOrThrow(It.Is<Guid>(id => id == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userEntity);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNoUserIdClaim_ShouldNotSucceed()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        _authRepositoryMock.Verify(
            x => x.FindUserByIdOrThrow(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task HandleRequirementAsync_WithInvalidUserIdFormat_ShouldNotSucceed()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "invalid-guid") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        _authRepositoryMock.Verify(
            x => x.FindUserByIdOrThrow(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task HandleRequirementAsync_WithDbConnectivityError_ShouldFallbackToJwtClaims()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimsConstants.IsActive, "true"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        _authRepositoryMock
            .Setup(x => x.FindUserByIdOrThrow(It.Is<Guid>(id => id == userId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Database timeout"));

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithDbError_AndNonMatchingClaim_ShouldNotSucceed()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimsConstants.IsActive, "false"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        _authRepositoryMock
            .Setup(x => x.FindUserByIdOrThrow(It.Is<Guid>(id => id == userId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Database timeout"));

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithTaskCanceledException_ShouldFallbackToJwtClaims()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimsConstants.IsVerified, "true"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsVerified, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        _authRepositoryMock
            .Setup(x => x.FindUserByIdOrThrow(It.Is<Guid>(id => id == userId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request cancelled"));

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue("should fallback to JWT claims on task cancellation");
    }

    [Fact]
    public async Task HandleRequirementAsync_WithOperationCanceledException_ShouldFallbackToJwtClaims()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimsConstants.IsActive, "true"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        _authRepositoryMock
            .Setup(x => x.FindUserByIdOrThrow(It.Is<Guid>(id => id == userId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException("Operation cancelled"));

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue("should fallback to JWT claims on operation cancellation");
    }

    // Note: NpgsqlException tests are difficult to test via unit tests due to internal constructors
    // The error handling logic for Npgsql errors is covered by integration tests

    [Fact]
    public async Task HandleRequirementAsync_WithNonConnectivityException_ShouldNotFallback()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimsConstants.IsActive, "true"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        _authRepositoryMock
            .Setup(x => x.FindUserByIdOrThrow(It.Is<Guid>(id => id == userId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Some other error"));

        // Act & Assert
        Func<Task> act = async () => await _handler.HandleAsync(context);
        await act.Should().ThrowExactlyAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNullUser_ShouldNotSucceed()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimsConstants.IsActive, "true"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        _authRepositoryMock
            .Setup(x => x.FindUserByIdOrThrow(It.Is<Guid>(id => id == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse("should not succeed when user is null");
    }

    [Fact]
    public async Task HandleRequirementAsync_WithUnknownClaimType_ShouldNotSucceed()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("CustomClaim", "true"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement("CustomClaim", "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        UserEntity userEntity = UserFactory.CreateWithId(userId);
        _authRepositoryMock
            .Setup(x => x.FindUserByIdOrThrow(It.Is<Guid>(id => id == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userEntity);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse("should not succeed for unknown claim types");
    }

    [Fact]
    public async Task HandleRequirementAsync_WithInvalidBooleanRequirementValue_ShouldNotSucceed()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimsConstants.IsActive, "true"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "invalid-boolean");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        UserEntity userEntity = UserFactory.CreateWithId(userId);
        typeof(UserEntity).GetProperty(nameof(UserEntity.IsActive))!.SetValue(userEntity, true);
        _authRepositoryMock
            .Setup(x => x.FindUserByIdOrThrow(It.Is<Guid>(id => id == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userEntity);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse("should not succeed when requirement value is not a valid boolean");
    }

    [Fact]
    public async Task HandleRequirementAsync_WithDbError_AndMissingClaim_ShouldNotSucceed()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            // Missing IsActive claim
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        _authRepositoryMock
            .Setup(x => x.FindUserByIdOrThrow(It.Is<Guid>(id => id == userId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Database timeout"));

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse("should not succeed when claim is missing in fallback");
    }

    [Fact]
    public async Task HandleRequirementAsync_WithDbError_AndEmptyClaim_ShouldNotSucceed()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimsConstants.IsActive, string.Empty),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        _authRepositoryMock
            .Setup(x => x.FindUserByIdOrThrow(It.Is<Guid>(id => id == userId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Database timeout"));

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse("should not succeed when claim value is empty in fallback");
    }

    [Fact]
    public async Task HandleRequirementAsync_WithDbError_AndCaseInsensitiveMatch_ShouldSucceed()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimsConstants.IsActive, "TRUE"), // Uppercase
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "true"); // Lowercase
        var context = new AuthorizationHandlerContext([requirement], user, null);

        _authRepositoryMock
            .Setup(x => x.FindUserByIdOrThrow(It.Is<Guid>(id => id == userId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Database timeout"));

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue("claim comparison should be case-insensitive");
    }

    [Fact]
    public async Task HandleRequirementAsync_WithEmptyUserIdClaim_ShouldNotSucceed()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, string.Empty) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse("should not succeed with empty user ID claim");
        _authRepositoryMock.Verify(
            x => x.FindUserByIdOrThrow(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
