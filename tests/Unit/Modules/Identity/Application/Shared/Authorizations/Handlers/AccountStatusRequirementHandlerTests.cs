using System.Security.Claims;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Authorizations.Handlers;
using _116.Identity.Application.Shared.Authorizations.Requirements;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Authorizations.Handlers;

public class AccountStatusRequirementHandlerTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly AccountStatusRequirementHandler _handler;

    public AccountStatusRequirementHandlerTests()
    {
        _authRepositoryMock = new Mock<IAuthRepository>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
        _handler = new AccountStatusRequirementHandler(_authRepositoryMock.Object, _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithActiveUser_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimsConstants.IsActive, "true"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        UserEntity userEntity = new UserBuilder().WithId(userId).AsActive().Build();
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
        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimsConstants.IsVerified, "true"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsVerified, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        UserEntity userEntity = new UserBuilder().WithId(userId).AsVerified().Build();
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
        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimsConstants.IsActive, "true"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "true");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        UserEntity userEntity = new UserBuilder().WithId(userId).AsInactive().Build();
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

    // Note: NpgsqlException tests are difficult to test via unit tests due to internal constructors
    // The error handling logic for Npgsql errors is covered by integration tests

    [Fact]
    public async Task HandleRequirementAsync_WithNullUser_ShouldNotSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
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
        var userId = Guid.NewGuid();
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
        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtClaimsConstants.IsActive, "true"),
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "invalid-boolean");
        var context = new AuthorizationHandlerContext([requirement], user, null);

        UserEntity userEntity = new UserBuilder().WithId(userId).AsActive().Build();
        _authRepositoryMock
            .Setup(x => x.FindUserByIdOrThrow(It.Is<Guid>(id => id == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userEntity);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse("should not succeed when requirement value is not a valid boolean");
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

    [Theory]
    [InlineData(typeof(TimeoutException))]
    [InlineData(typeof(TaskCanceledException))]
    [InlineData(typeof(OperationCanceledException))]
    public async Task HandleRequirementAsync_WithDbConnectivityError_ShouldFailClosed(Type exceptionType)
    {
        // Arrange
        var userId = Guid.NewGuid();
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
            .ThrowsAsync((Exception)Activator.CreateInstance(exceptionType, "connectivity failure")!);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasFailed.Should().BeTrue("an unverifiable account status fails closed");
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_EvaluatedTwiceInOneRequest_ShouldQueryOnce()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        UserEntity userEntity = new UserBuilder().WithId(userId).AsActive().AsVerified().Build();

        _authRepositoryMock
            .Setup(x => x.FindUserByIdOrThrow(It.Is<Guid>(id => id == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userEntity);

        var activeRequirement = new AccountStatusRequirement(JwtClaimsConstants.IsActive, "true");
        var verifiedRequirement = new AccountStatusRequirement(JwtClaimsConstants.IsVerified, "true");
        var activeContext = new AuthorizationHandlerContext([activeRequirement], user, null);
        var verifiedContext = new AuthorizationHandlerContext([verifiedRequirement], user, null);

        // Act
        await _handler.HandleAsync(activeContext);
        await _handler.HandleAsync(verifiedContext);

        // Assert
        activeContext.HasSucceeded.Should().BeTrue();
        verifiedContext.HasSucceeded.Should().BeTrue();
        _authRepositoryMock.Verify(
            x => x.FindUserByIdOrThrow(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
