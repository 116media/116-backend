using _116.Identity.Application.Shared.Cache;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IUserTokenStateRepository" /> against the real database:
/// projection reads, lazy provisioning, and the atomicity of the bumps.
/// </summary>
[Collection("Database")]
public class UserTokenStateRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    /// <summary>
    /// Seeds a user row (the test-host interceptor adds its token-state row in the same save).
    /// </summary>
    /// <returns>The id of the seeded user.</returns>
    private async Task<Guid> SeedUserAsync()
    {
        await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
        UserEntity user = UserFactory.Create();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.Id;
    }

    [Fact]
    public async Task GetAsync_ForAnExistingRecord_ReturnsTheProjection()
    {
        // Arrange
        Guid userId = await SeedUserAsync();
        var repository = Resolve<IUserTokenStateRepository>();

        // Act
        UserSecurityState? state = await repository.GetAsync(userId: userId, cancellationToken: CancellationToken.None);

        // Assert
        state.Should().NotBeNull();
        state!.Value.SecurityStamp.Should().NotBe(Guid.Empty);
        state.Value.TokenVersion.Should().Be(0);
    }

    [Fact]
    public async Task GetAsync_ForAMissingRecord_ReturnsNull()
    {
        // Arrange
        var repository = Resolve<IUserTokenStateRepository>();

        // Act
        UserSecurityState? state = await repository.GetAsync(
            userId: Guid.NewGuid(),
            cancellationToken: CancellationToken.None
        );

        // Assert
        state.Should().BeNull();
    }

    [Fact]
    public async Task GetOrCreateAsync_ForAUserWithoutARecord_ProvisionsAndPersistsOne()
    {
        // Arrange — strip the interceptor-provided record to reproduce a pre-migration account
        Guid userId = await SeedUserAsync();
        await using (IdentityDbContext arrangeContext = CreateDbContext<IdentityDbContext>())
        {
            await arrangeContext.UserTokenStates.Where(s => s.Id == userId).ExecuteDeleteAsync();
        }

        var repository = Resolve<IUserTokenStateRepository>();

        // Act
        UserSecurityState created = await repository.GetOrCreateAsync(
            userId: userId,
            cancellationToken: CancellationToken.None
        );

        // Assert — the record now exists and matches what was returned
        created.SecurityStamp.Should().NotBe(Guid.Empty);
        created.TokenVersion.Should().Be(0);

        await using IdentityDbContext verifyContext = CreateDbContext<IdentityDbContext>();
        UserTokenStateEntity persisted = await verifyContext.UserTokenStates.SingleAsync(s => s.Id == userId);
        persisted.SecurityStamp.Should().Be(created.SecurityStamp);
    }

    [Fact]
    public async Task GetOrCreateAsync_ForAUserWithARecord_ReturnsItUnchanged()
    {
        // Arrange
        Guid userId = await SeedUserAsync();
        var repository = Resolve<IUserTokenStateRepository>();
        UserSecurityState? existing = await repository.GetAsync(
            userId: userId,
            cancellationToken: CancellationToken.None
        );

        // Act
        UserSecurityState resolved = await repository.GetOrCreateAsync(
            userId: userId,
            cancellationToken: CancellationToken.None
        );

        // Assert
        resolved.Should().Be(existing!.Value);
    }

    [Fact]
    public async Task BumpTokenVersionAsync_Twice_LandsOnTwo()
    {
        // Arrange
        Guid userId = await SeedUserAsync();
        var repository = Resolve<IUserTokenStateRepository>();

        // Act — both bumps must land: the increment is set-based SQL, not read-modify-write
        await repository.BumpTokenVersionAsync(userId: userId, cancellationToken: CancellationToken.None);
        await repository.BumpTokenVersionAsync(userId: userId, cancellationToken: CancellationToken.None);

        // Assert
        await using IdentityDbContext verifyContext = CreateDbContext<IdentityDbContext>();
        UserTokenStateEntity state = await verifyContext.UserTokenStates.SingleAsync(s => s.Id == userId);
        state.TokenVersion.Should().Be(2);
    }

    [Fact]
    public async Task RotateSecurityStampAsync_ChangesTheStampAndReturnsIt()
    {
        // Arrange
        Guid userId = await SeedUserAsync();
        var repository = Resolve<IUserTokenStateRepository>();
        UserSecurityState? before = await repository.GetAsync(
            userId: userId,
            cancellationToken: CancellationToken.None
        );

        // Act
        Guid newStamp = await repository.RotateSecurityStampAsync(
            userId: userId,
            cancellationToken: CancellationToken.None
        );

        // Assert
        newStamp.Should().NotBe(before!.Value.SecurityStamp);

        await using IdentityDbContext verifyContext = CreateDbContext<IdentityDbContext>();
        UserTokenStateEntity state = await verifyContext.UserTokenStates.SingleAsync(s => s.Id == userId);
        state.SecurityStamp.Should().Be(newStamp);
        state.TokenVersion.Should().Be(before.Value.TokenVersion);
    }

    [Fact]
    public async Task BumpTokenVersionForRoleUsersAsync_BumpsEveryMemberAndOnlyMembers()
    {
        // Arrange — two users hold the role, one does not
        Guid memberOneId = await SeedUserAsync();
        Guid memberTwoId = await SeedUserAsync();
        Guid outsiderId = await SeedUserAsync();

        Guid roleId;
        await using (IdentityDbContext arrangeContext = CreateDbContext<IdentityDbContext>())
        {
            RoleEntity role = RoleFactory.Create();
            roleId = role.Id;
            arrangeContext.Roles.Add(role);
            arrangeContext.UserRoles.Add(UserRoleFactory.Create(memberOneId, role.Id));
            arrangeContext.UserRoles.Add(UserRoleFactory.Create(memberTwoId, role.Id));
            await arrangeContext.SaveChangesAsync();
        }

        var repository = Resolve<IUserTokenStateRepository>();

        // Act
        await repository.BumpTokenVersionForRoleUsersAsync(roleId: roleId, cancellationToken: CancellationToken.None);

        // Assert
        await using IdentityDbContext verifyContext = CreateDbContext<IdentityDbContext>();
        (await verifyContext.UserTokenStates.SingleAsync(s => s.Id == memberOneId)).TokenVersion.Should().Be(1);
        (await verifyContext.UserTokenStates.SingleAsync(s => s.Id == memberTwoId)).TokenVersion.Should().Be(1);
        (await verifyContext.UserTokenStates.SingleAsync(s => s.Id == outsiderId)).TokenVersion.Should().Be(0);
    }
}
