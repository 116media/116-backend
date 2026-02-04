using System.Security.Claims;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common.Builders.Entities;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="AuthRepository"/>.
/// </summary>
public class AuthRepositoryTests : IDisposable
{
    private readonly IdentityDbContext _context;
    private readonly AuthRepository _repository;

    public AuthRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new IdentityDbContext(options);
        _repository = new AuthRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region FindUserByIdOrThrow Tests

    [Fact]
    public async Task FindUserByIdOrThrow_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = new UserBuilder().Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindUserByIdOrThrow(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task FindUserByIdOrThrow_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.FindUserByIdOrThrow(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetUserWithRolesByEmailOrThrow Tests

    [Fact]
    public async Task GetUserWithRolesByEmailOrThrow_WhenUserExists_ShouldReturnUserWithRoles()
    {
        // Arrange
        var user = new UserBuilder().WithEmail("test@example.com").Build();
        var role = new RoleBuilder().WithName("Admin").Build();
        var userRole = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role.Id);

        _context.Users.Add(user);
        _context.Roles.Add(role);
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserWithRolesByEmailOrThrow(new Email("test@example.com"));

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
        result.UserRoles.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUserWithRolesByEmailOrThrow_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var email = new Email("nonexistent@example.com");

        // Act
        Func<Task> act = async () => await _repository.GetUserWithRolesByEmailOrThrow(email);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetUserWithRolesAndPermissionsByIdOrThrow Tests

    [Fact]
    public async Task GetUserWithRolesAndPermissionsByIdOrThrow_WhenUserExists_ShouldReturnUserWithRolesAndPermissions()
    {
        // Arrange
        var user = new UserBuilder().Build();
        var role = new RoleBuilder().WithName("Admin").Build();
        var permission = new PermissionBuilder().WithResource("article").WithAction("read").Build();
        var userRole = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role.Id);
        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission.Id);

        _context.Users.Add(user);
        _context.Roles.Add(role);
        _context.Permissions.Add(permission);
        _context.UserRoles.Add(userRole);
        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserWithRolesAndPermissionsByIdOrThrow(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.UserRoles.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUserWithRolesAndPermissionsByIdOrThrow_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetUserWithRolesAndPermissionsByIdOrThrow(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region ExistsByEmailAsync Tests

    [Fact]
    public async Task ExistsByEmailAsync_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        var user = new UserBuilder().WithEmail("exists@example.com").Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        bool exists = await _repository.ExistsByEmailAsync(new Email("exists@example.com"));

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenUserDoesNotExist_ShouldReturnFalse()
    {
        // Arrange & Act
        bool exists = await _repository.ExistsByEmailAsync(new Email("nonexistent@example.com"));

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    #region ExistsByUserNameAsync Tests

    [Fact]
    public async Task ExistsByUserNameAsync_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        var user = new UserBuilder().WithUserName("testuser").Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        bool exists = await _repository.ExistsByUserNameAsync("testuser");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByUserNameAsync_WhenUserDoesNotExist_ShouldReturnFalse()
    {
        // Arrange & Act
        bool exists = await _repository.ExistsByUserNameAsync("nonexistentuser");

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    // Note: GetUserByPhoneNumberAsync tests skipped as UserEntity doesn't have PhoneNumber property in current implementation

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddUserToContext()
    {
        // Arrange
        var user = new UserBuilder().WithEmail("new@example.com").Build();

        // Act
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "new@example.com");
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be("new@example.com");
    }

    #endregion

    #region IsUserAccountActive Tests

    [Fact]
    public void IsUserAccountActive_WhenUserIsActive_ShouldReturnTrue()
    {
        // Arrange
        var user = new UserBuilder().AsActive().Build();

        // Act
        bool isActive = _repository.IsUserAccountActive(user);

        // Assert
        isActive.Should().BeTrue();
    }

    [Fact]
    public void IsUserAccountActive_WhenUserIsInactive_ShouldThrowAccountInactiveException()
    {
        // Arrange
        var user = new UserBuilder().AsInactive().Build();

        // Act
        Action act = () => _repository.IsUserAccountActive(user);

        // Assert
        act.Should().Throw<AccountInactiveException>();
    }

    #endregion

    #region IsUserAccountVerified Tests

    [Fact]
    public void IsUserAccountVerified_WhenLocalUserIsVerified_ShouldReturnTrue()
    {
        // Arrange
        var user = new UserBuilder().WithAuthProvider(EnumAuthProvider.Local).AsVerified().Build();

        // Act
        bool isVerified = _repository.IsUserAccountVerified(user);

        // Assert
        isVerified.Should().BeTrue();
    }

    [Fact]
    public void IsUserAccountVerified_WhenLocalUserIsNotVerified_ShouldThrowAccountNotVerifiedException()
    {
        // Arrange - Default UserBuilder creates unverified users
        var user = new UserBuilder().WithAuthProvider(EnumAuthProvider.Local).Build();

        // Act
        Action act = () => _repository.IsUserAccountVerified(user);

        // Assert
        act.Should().Throw<AccountNotVerifiedException>();
    }

    [Fact]
    public void IsUserAccountVerified_WhenExternalUser_ShouldReturnTrueRegardlessOfVerificationStatus()
    {
        // Arrange - External users don't need verification
        var user = new UserBuilder().WithAuthProvider(EnumAuthProvider.Google).Build();

        // Act
        bool isVerified = _repository.IsUserAccountVerified(user);

        // Assert
        isVerified.Should().BeTrue();
    }

    #endregion

    #region GetSessionIdFromClaims Tests

    [Fact]
    public void GetSessionIdFromClaims_WhenValidSessionIdClaim_ShouldReturnSessionId()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var claims = new List<Claim> { new Claim("ref", sessionId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // Act
        var result = _repository.GetSessionIdFromClaims(claimsPrincipal);

        // Assert
        result.Should().Be(sessionId);
    }

    [Fact]
    public void GetSessionIdFromClaims_WhenSessionIdClaimMissing_ShouldThrowAuthenticationException()
    {
        // Arrange
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // Act
        Action act = () => _repository.GetSessionIdFromClaims(claimsPrincipal);

        // Assert
        act.Should().Throw<AuthenticationException>();
    }

    [Fact]
    public void GetSessionIdFromClaims_WhenSessionIdClaimInvalid_ShouldThrowAuthenticationException()
    {
        // Arrange
        var claims = new List<Claim> { new Claim("ref", "invalid-guid") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // Act
        Action act = () => _repository.GetSessionIdFromClaims(claimsPrincipal);

        // Assert
        act.Should().Throw<AuthenticationException>();
    }

    #endregion

    #region GetUserIdFromClaims Tests

    [Fact]
    public void GetUserIdFromClaims_WhenValidUserIdClaim_ShouldReturnUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // Act
        var result = _repository.GetUserIdFromClaims(claimsPrincipal);

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserIdFromClaims_WhenUserIdClaimMissing_ShouldThrowAuthenticationException()
    {
        // Arrange
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // Act
        Action act = () => _repository.GetUserIdFromClaims(claimsPrincipal);

        // Assert
        act.Should().Throw<AuthenticationException>();
    }

    [Fact]
    public void GetUserIdFromClaims_WhenUserIdClaimInvalid_ShouldThrowAuthenticationException()
    {
        // Arrange
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "invalid-guid") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // Act
        Action act = () => _repository.GetUserIdFromClaims(claimsPrincipal);

        // Assert
        act.Should().Throw<AuthenticationException>();
    }

    #endregion

    // Note: IsUserAdmin tests require UserRoles navigation property to be loaded from database.
    // These tests are better suited for integration tests with a real database context.

    #region ValidateUniqueCredentialsAsync Tests

    [Fact]
    public async Task ValidateUniqueCredentialsAsync_WhenCredentialsAreUnique_ShouldNotThrow()
    {
        // Arrange
        var email = new Email("unique@example.com");
        var userName = "uniqueuser";

        // Act
        Func<Task> act = async () => await _repository.ValidateUniqueCredentialsAsync(email, userName);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateUniqueCredentialsAsync_WhenEmailExists_ShouldThrowConflictException()
    {
        // Arrange
        var existingUser = new UserBuilder().WithEmail("existing@example.com").Build();
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var email = new Email("existing@example.com");
        var userName = "newuser";

        // Act
        Func<Task> act = async () => await _repository.ValidateUniqueCredentialsAsync(email, userName);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ValidateUniqueCredentialsAsync_WhenUserNameExists_ShouldThrowConflictException()
    {
        // Arrange
        var existingUser = new UserBuilder().WithUserName("existinguser").Build();
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var email = new Email("new@example.com");
        var userName = "existinguser";

        // Act
        Func<Task> act = async () => await _repository.ValidateUniqueCredentialsAsync(email, userName);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion

    // Note: AssignVisitorRoleAsync tests require domain method access which is better tested through integration tests

    // Note: SetPasswordForExternalUser tests require domain method access which is better tested through integration tests

    #region GetUserWithRolesAndPermissionsByEmailOrThrow Tests

    [Fact]
    public async Task GetUserWithRolesAndPermissionsByEmailOrThrow_WhenUserExists_ShouldReturnUserWithRolesAndPermissions()
    {
        // Arrange
        var user = new UserBuilder().WithEmail("test@example.com").Build();
        var role = new RoleBuilder().WithName("Admin").Build();
        var permission = new PermissionBuilder().WithResource("article").WithAction("read").Build();
        var userRole = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role.Id);
        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission.Id);

        _context.Users.Add(user);
        _context.Roles.Add(role);
        _context.Permissions.Add(permission);
        _context.UserRoles.Add(userRole);
        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserWithRolesAndPermissionsByEmailOrThrow(new Email("test@example.com"));

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
        result.UserRoles.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUserWithRolesAndPermissionsByEmailOrThrow_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var email = new Email("nonexistent@example.com");

        // Act
        Func<Task> act = async () => await _repository.GetUserWithRolesAndPermissionsByEmailOrThrow(email);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetUserWithRolesAndPermissionsByCredentialsOrThrow Tests

    [Fact]
    public async Task GetUserWithRolesAndPermissionsByCredentialsOrThrow_WithEmail_ShouldReturnUser()
    {
        // Arrange
        var user = new UserBuilder().WithEmail("credentials@example.com").Build();
        var role = new RoleBuilder().WithName("Admin").Build();
        var userRole = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role.Id);

        _context.Users.Add(user);
        _context.Roles.Add(role);
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserWithRolesAndPermissionsByCredentialsOrThrow("credentials@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("credentials@example.com");
    }

    [Fact]
    public async Task GetUserWithRolesAndPermissionsByCredentialsOrThrow_WithUserName_ShouldReturnUser()
    {
        // Arrange
        var user = new UserBuilder().WithUserName("testusername").Build();
        var role = new RoleBuilder().WithName("Admin").Build();
        var userRole = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role.Id);

        _context.Users.Add(user);
        _context.Roles.Add(role);
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserWithRolesAndPermissionsByCredentialsOrThrow("testusername");

        // Assert
        result.Should().NotBeNull();
        result!.UserName.Should().Be("testusername");
    }

    [Fact]
    public async Task GetUserWithRolesAndPermissionsByCredentialsOrThrow_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange & Act
        Func<Task> act = async () =>
            await _repository.GetUserWithRolesAndPermissionsByCredentialsOrThrow("nonexistent");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetUserWithSessionsByIdOrThrow Tests

    [Fact]
    public async Task GetUserWithSessionsByIdOrThrow_WhenUserExists_ShouldReturnUserWithSessions()
    {
        // Arrange
        var user = new UserBuilder().Build();
        var session = new SessionBuilder().WithUserId(user.Id).Build();

        // Set CreatedAt using reflection (SessionBuilder doesn't set it)
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(session, DateTime.UtcNow);

        _context.Users.Add(user);
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserWithSessionsByIdOrThrow(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Sessions.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUserWithSessionsByIdOrThrow_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetUserWithSessionsByIdOrThrow(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region IsSessionValidAsync Tests

    [Fact]
    public async Task IsSessionValidAsync_WhenSessionIsValid_ShouldReturnTrue()
    {
        // Arrange
        var user = new UserBuilder().Build();
        var session = new SessionBuilder().WithUserId(user.Id).Build();

        // Set CreatedAt using reflection (SessionBuilder doesn't set it)
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(session, DateTime.UtcNow);

        _context.Users.Add(user);
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.IsSessionValidAsync(session.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsSessionValidAsync_WhenSessionDoesNotExist_ShouldThrowRefreshTokenExpiryException()
    {
        // Arrange
        var nonExistentSessionId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.IsSessionValidAsync(nonExistentSessionId);

        // Assert
        await act.Should().ThrowAsync<RefreshTokenExpiryException>();
    }

    [Fact]
    public async Task IsSessionValidAsync_WhenSessionIsExpired_ShouldThrowRefreshTokenExpiryException()
    {
        // Arrange
        var user = new UserBuilder().Build();
        var session = new SessionBuilder().WithUserId(user.Id).AsExpired().Build();

        // Set CreatedAt using reflection (SessionBuilder doesn't set it)
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(session, DateTime.UtcNow);

        _context.Users.Add(user);
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await _repository.IsSessionValidAsync(session.Id);

        // Assert
        await act.Should().ThrowAsync<RefreshTokenExpiryException>();
    }

    #endregion

    #region GetUserByPhoneNumberAsync Tests

    [Fact]
    public async Task GetUserByPhoneNumberAsync_WhenUserExistsWithFullPhoneNumber_ShouldReturnUser()
    {
        // Arrange
        var user = new UserBuilder().Build();
        // Set phone number using reflection since UserBuilder doesn't have WithPhoneNumber
        var fullPhoneProperty = typeof(UserEntity).GetProperty("FullPhoneNumber");
        fullPhoneProperty!.SetValue(user, "+1234567890");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserByPhoneNumberAsync("+1234567890");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetUserByPhoneNumberAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange & Act
        var result = await _repository.GetUserByPhoneNumberAsync("+9999999999");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region IsUserAdmin Tests

    [Fact]
    public void IsUserAdmin_WhenUserIsAdmin_ShouldReturnTrue()
    {
        // Arrange
        var user = new UserBuilder().Build();
        var adminRole = new RoleBuilder().WithName("Admin").Build();
        var userRole = UserRoleEntity.Create(Guid.NewGuid(), user.Id, adminRole.Id);

        _context.Users.Add(user);
        _context.Roles.Add(adminRole);
        _context.UserRoles.Add(userRole);
        _context.SaveChanges();

        // Reload user with roles
        var userWithRoles = _context
            .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .First(u => u.Id == user.Id);

        // Act
        var result = _repository.IsUserAdmin(userWithRoles);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsUserAdmin_WhenUserIsNotAdmin_ShouldThrowAccessDeniedException()
    {
        // Arrange
        var user = new UserBuilder().Build();
        var visitorRole = new RoleBuilder().WithName("Visitor").Build();
        var userRole = UserRoleEntity.Create(Guid.NewGuid(), user.Id, visitorRole.Id);

        _context.Users.Add(user);
        _context.Roles.Add(visitorRole);
        _context.UserRoles.Add(userRole);
        _context.SaveChanges();

        // Reload user with roles
        var userWithRoles = _context
            .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .First(u => u.Id == user.Id);

        // Act
        Action act = () => _repository.IsUserAdmin(userWithRoles);

        // Assert
        act.Should().Throw<AccessDeniedException>();
    }

    #endregion

    #region AssignVisitorRoleAsync Tests

    [Fact]
    public async Task AssignVisitorRoleAsync_WhenVisitorRoleExists_ShouldNotThrow()
    {
        // Arrange
        var user = new UserBuilder().Build();
        var visitorRole = new RoleBuilder().WithName("Visitor").Build();

        _context.Users.Add(user);
        _context.Roles.Add(visitorRole);
        await _context.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await _repository.AssignVisitorRoleAsync(user.Id);

        // Assert - Method should complete without throwing
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AssignVisitorRoleAsync_WhenVisitorRoleDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var user = new UserBuilder().Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await _repository.AssignVisitorRoleAsync(user.Id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetOrCreateExternalUserAsync Tests

    [Fact]
    public async Task GetOrCreateExternalUserAsync_WhenUserDoesNotExist_ShouldCreateNewUser()
    {
        // Arrange
        var visitorRole = new RoleBuilder().WithName("Visitor").Build();
        _context.Roles.Add(visitorRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOrCreateExternalUserAsync(
            "external@example.com",
            "externaluser",
            new AuthProvider(EnumAuthProvider.Google)
        );

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("external@example.com");
        result.UserName.Should().Be("externaluser");
        result.AuthProvider.Should().Be(EnumAuthProvider.Google);
    }

    [Fact]
    public async Task GetOrCreateExternalUserAsync_WhenExternalUserExists_ShouldReturnExistingUser()
    {
        // Arrange
        var visitorRole = new RoleBuilder().WithName("Visitor").Build();
        var existingUser = UserEntity.CreateExternal(
            Guid.NewGuid(),
            "existinguser",
            EnumAuthProvider.Google,
            "external@example.com"
        );

        _context.Roles.Add(visitorRole);
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOrCreateExternalUserAsync(
            "external@example.com",
            "existinguser",
            new AuthProvider(EnumAuthProvider.Google)
        );

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingUser.Id);
        result.Email.Should().Be("external@example.com");
    }

    [Fact]
    public async Task GetOrCreateExternalUserAsync_WhenLocalUserExists_ShouldThrowConflictException()
    {
        // Arrange
        var localUser = new UserBuilder()
            .WithEmail("local@example.com")
            .WithAuthProvider(EnumAuthProvider.Local)
            .Build();

        _context.Users.Add(localUser);
        await _context.SaveChangesAsync();

        // Act
        Func<Task> act = async () =>
            await _repository.GetOrCreateExternalUserAsync(
                "local@example.com",
                "username",
                new AuthProvider(EnumAuthProvider.Google)
            );

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task GetOrCreateExternalUserAsync_WhenUpdatingUsername_ShouldUpdateIfNotTaken()
    {
        // Arrange
        var visitorRole = new RoleBuilder().WithName("Visitor").Build();
        var existingUser = UserEntity.CreateExternal(
            Guid.NewGuid(),
            "oldusername",
            EnumAuthProvider.Google,
            "external@example.com"
        );

        _context.Roles.Add(visitorRole);
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOrCreateExternalUserAsync(
            "external@example.com",
            "newusername",
            new AuthProvider(EnumAuthProvider.Google)
        );

        // Assert
        result.Should().NotBeNull();
        result!.UserName.Should().Be("newusername");
    }

    [Fact]
    public async Task GetOrCreateExternalUserAsync_WhenUpdatingUsernameToTakenOne_ShouldThrowConflictException()
    {
        // Arrange
        var visitorRole = new RoleBuilder().WithName("Visitor").Build();
        var existingUser1 = UserEntity.CreateExternal(
            Guid.NewGuid(),
            "user1",
            EnumAuthProvider.Google,
            "user1@example.com"
        );
        var existingUser2 = new UserBuilder().WithUserName("takenusername").Build();

        _context.Roles.Add(visitorRole);
        _context.Users.AddRange(existingUser1, existingUser2);
        await _context.SaveChangesAsync();

        // Act
        Func<Task> act = async () =>
            await _repository.GetOrCreateExternalUserAsync(
                "user1@example.com",
                "takenusername",
                new AuthProvider(EnumAuthProvider.Google)
            );

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task GetOrCreateExternalUserAsync_WhenUsernameIsNullOrWhitespace_ShouldNotUpdateUsername()
    {
        // Arrange
        var visitorRole = new RoleBuilder().WithName("Visitor").Build();
        var existingUser = UserEntity.CreateExternal(
            Guid.NewGuid(),
            "originalusername",
            EnumAuthProvider.Google,
            "external@example.com"
        );

        _context.Roles.Add(visitorRole);
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOrCreateExternalUserAsync(
            "external@example.com",
            null,
            new AuthProvider(EnumAuthProvider.Google)
        );

        // Assert
        result.Should().NotBeNull();
        result!.UserName.Should().Be("originalusername");
    }

    #endregion

    #region SetPasswordForExternalUser Tests

    [Fact]
    public void SetPasswordForExternalUser_WhenExternalUser_ShouldSetPasswordAndChangeToLocal()
    {
        // Arrange
        var externalUser = UserEntity.CreateExternal(
            Guid.NewGuid(),
            "externaluser",
            EnumAuthProvider.Google,
            "external@example.com"
        );

        // Act
        _repository.SetPasswordForExternalUser(externalUser, "hashedPassword123");

        // Assert
        externalUser.AuthProvider.Should().Be(EnumAuthProvider.Local);
        externalUser.PasswordHash.Should().Be("hashedPassword123");
    }

    [Fact]
    public void SetPasswordForExternalUser_WhenUserHasNoEmail_ShouldThrowBadRequestException()
    {
        // Arrange - Create user without email by using reflection
        var user = UserEntity.CreateExternal(Guid.NewGuid(), "user", EnumAuthProvider.Google, "temp@example.com");
        // Set email to null using reflection
        var emailProperty = typeof(UserEntity).GetProperty("Email");
        emailProperty!.SetValue(user, null);

        // Act
        Action act = () => _repository.SetPasswordForExternalUser(user, "hashedPassword");

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Fact]
    public void SetPasswordForExternalUser_WhenLocalUser_ShouldThrowBadRequestException()
    {
        // Arrange
        var localUser = new UserBuilder().WithAuthProvider(EnumAuthProvider.Local).Build();

        // Act
        Action act = () => _repository.SetPasswordForExternalUser(localUser, "hashedPassword");

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion
}
