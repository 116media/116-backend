using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Exceptions;
using _116.Identity.Domain.StateMachines;
using _116.Identity.Domain.ValueObjects;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IAuthRepository"/> verifying
/// user lookup, existence checks, persistence, and account status
/// validation against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class AuthRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task FindUserByIdOrThrow_ExistingId_ShouldReturnUser()
    {
        // Arrange
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.Create();
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IAuthRepository>();

        // Act
        var result = await repo.FindUserByIdOrThrow(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task FindUserByIdOrThrow_NonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var repo = Resolve<IAuthRepository>();
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = () => repo.FindUserByIdOrThrow(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetUserWithRolesByEmailOrThrow_ExistingEmail_ShouldReturnUserWithRoles()
    {
        // Arrange
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        var user = UserFactory.CreateWithRole(role);
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IAuthRepository>();
        Email email = user.Email!;

        // Act
        var result = await repo.GetUserWithRolesByEmailOrThrow(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(user.Email);
        result.UserRoles.Should().NotBeEmpty();
        result.UserRoles.First().Role.Should().NotBeNull();
    }

    [Fact]
    public async Task ExistsByEmailAsync_ExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.Create("exists-email@example.com");
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IAuthRepository>();
        var email = new Email("exists-email@example.com");

        // Act
        var exists = await repo.ExistsByEmailAsync(email);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_NonExistentEmail_ShouldReturnFalse()
    {
        // Arrange
        var repo = Resolve<IAuthRepository>();
        var email = new Email("nonexistent-auth@example.com");

        // Act
        var exists = await repo.ExistsByEmailAsync(email);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByUserNameAsync_ExistingUserName_ShouldReturnTrue()
    {
        // Arrange
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.Create();
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IAuthRepository>();

        // Act
        var exists = await repo.ExistsByUserNameAsync(user.UserName);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByUserNameAsync_NonExistentUserName_ShouldReturnFalse()
    {
        // Arrange
        var repo = Resolve<IAuthRepository>();

        // Act
        var exists = await repo.ExistsByUserNameAsync("nonexistent-username-xyz");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistUserToDatabase()
    {
        // Arrange
        var (repo, db) = CreateScopedRepository<IAuthRepository, IdentityDbContext>();
        var user = UserFactory.Create("add-async-auth@example.com");

        // Act
        await repo.AddAsync(user);
        await db.SaveChangesAsync();

        // Assert
        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var saved = await verifyContext.Users.FindAsync(user.Id);

        saved.Should().NotBeNull();
        saved!.Email.Should().Be("add-async-auth@example.com");
    }

    [Fact]
    public void IsUserAccountActive_ActiveUser_ShouldReturnTrue()
    {
        // Arrange
        var repo = Resolve<IAuthRepository>();
        var user = UserFactory.CreateVerifiedActive();

        // Act
        var result = repo.IsUserAccountActive(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsUserAccountActive_InactiveUser_ShouldThrowAuthorizationException()
    {
        // Arrange
        var repo = Resolve<IAuthRepository>();
        var user = UserFactory.CreateInactive();

        // Act
        var act = () => repo.IsUserAccountActive(user);

        // Assert
        act.Should().Throw<AccountInactiveException>();
    }

    [Fact]
    public void IsUserAccountVerified_VerifiedUser_ShouldReturnTrue()
    {
        // Arrange
        var repo = Resolve<IAuthRepository>();
        var user = UserFactory.CreateVerifiedActive();

        // Act
        var result = repo.IsUserAccountVerified(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsUserAccountVerified_UnverifiedUser_ShouldThrowAccountNotVerifiedException()
    {
        // Arrange
        var repo = Resolve<IAuthRepository>();
        var user = UserFactory.CreateUnverified();

        // Act
        var act = () => repo.IsUserAccountVerified(user);

        // Assert
        act.Should().Throw<AccountNotVerifiedException>();
    }

    [Fact]
    public async Task AssignVisitorRoleAsync_ExistingUserAndSeededRole_ShouldAttachTheRole()
    {
        // Arrange
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        var user = UserFactory.Create();
        seedContext.Roles.Add(visitorRole);
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IAuthRepository>();

        // Act
        await repo.AssignVisitorRoleAsync(user.Id);

        // Assert
        var attached = await repo.FindUserByIdOrThrow(user.Id);
        attached!.HasRole(visitorRole.Id).Should().BeTrue();
    }

    [Fact]
    public async Task AssignVisitorRoleAsync_CalledTwiceInOneScope_ShouldThrowTheRoleAlreadyAssignedRule()
    {
        // Arrange
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var visitorRole = RoleFactory.CreateWithId(Guid.NewGuid(), "Visitor");
        var user = UserFactory.Create();
        seedContext.Roles.Add(visitorRole);
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync();

        // One repository, so the second call sees the assignment the first one made
        var repo = Resolve<IAuthRepository>();
        await repo.AssignVisitorRoleAsync(user.Id);

        // Act
        var act = () => repo.AssignVisitorRoleAsync(user.Id);

        // Assert — the repository surfaces the domain rule; the strategy titles it a conflict
        (await act.Should().ThrowAsync<IdentityRuleException>())
            .Which.Code.Should()
            .Be(IdentityRuleCodes.RoleAlreadyAssignedToUser);
    }

    [Fact]
    public async Task AssignVisitorRoleAsync_WithoutTheVisitorRoleSeeded_ShouldThrowNotFoundException()
    {
        // Arrange
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.Create();
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IAuthRepository>();

        // Act
        var act = () => repo.AssignVisitorRoleAsync(user.Id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
