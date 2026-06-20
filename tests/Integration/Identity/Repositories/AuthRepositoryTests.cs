using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.ValueObjects;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Identity.Repositories;

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
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.Create();
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IAuthRepository>();

        var result = await repo.FindUserByIdOrThrow(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task FindUserByIdOrThrow_NonExistentId_ShouldThrowNotFoundException()
    {
        var repo = Resolve<IAuthRepository>();
        var nonExistentId = Guid.NewGuid();

        var act = () => repo.FindUserByIdOrThrow(nonExistentId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetUserWithRolesByEmailOrThrow_ExistingEmail_ShouldReturnUserWithRoles()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        var user = UserFactory.CreateWithRole(role);
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IAuthRepository>();
        Email email = user.Email!;

        var result = await repo.GetUserWithRolesByEmailOrThrow(email);

        result.Should().NotBeNull();
        result!.Email.Should().Be(user.Email);
        result.UserRoles.Should().NotBeEmpty();
        result.UserRoles.First().Role.Should().NotBeNull();
    }

    [Fact]
    public async Task ExistsByEmailAsync_ExistingEmail_ShouldReturnTrue()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.Create("exists-email@example.com");
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IAuthRepository>();
        var email = new Email("exists-email@example.com");

        var exists = await repo.ExistsByEmailAsync(email);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_NonExistentEmail_ShouldReturnFalse()
    {
        var repo = Resolve<IAuthRepository>();
        var email = new Email("nonexistent-auth@example.com");

        var exists = await repo.ExistsByEmailAsync(email);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByUserNameAsync_ExistingUserName_ShouldReturnTrue()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.Create();
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IAuthRepository>();

        var exists = await repo.ExistsByUserNameAsync(user.UserName);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByUserNameAsync_NonExistentUserName_ShouldReturnFalse()
    {
        var repo = Resolve<IAuthRepository>();

        var exists = await repo.ExistsByUserNameAsync("nonexistent-username-xyz");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistUserToDatabase()
    {
        var (repo, db) = CreateScopedRepository<IAuthRepository, IdentityDbContext>();

        var user = UserFactory.Create("add-async-auth@example.com");
        await repo.AddAsync(user);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var saved = await verifyContext.Users.FindAsync(user.Id);

        saved.Should().NotBeNull();
        saved!.Email.Should().Be("add-async-auth@example.com");
    }

    [Fact]
    public void IsUserAccountActive_ActiveUser_ShouldReturnTrue()
    {
        var repo = Resolve<IAuthRepository>();
        var user = UserFactory.CreateVerifiedActive();

        var result = repo.IsUserAccountActive(user);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsUserAccountActive_InactiveUser_ShouldThrowAuthorizationException()
    {
        var repo = Resolve<IAuthRepository>();
        var user = UserFactory.CreateInactive();

        var act = () => repo.IsUserAccountActive(user);

        act.Should().Throw<AccountInactiveException>();
    }

    [Fact]
    public void IsUserAccountVerified_VerifiedUser_ShouldReturnTrue()
    {
        var repo = Resolve<IAuthRepository>();
        var user = UserFactory.CreateVerifiedActive();

        var result = repo.IsUserAccountVerified(user);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsUserAccountVerified_UnverifiedUser_ShouldThrowAccountNotVerifiedException()
    {
        var repo = Resolve<IAuthRepository>();
        var user = UserFactory.CreateUnverified();

        var act = () => repo.IsUserAccountVerified(user);

        act.Should().Throw<AccountNotVerifiedException>();
    }
}
