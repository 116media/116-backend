using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Shared.Infrastructure.Interceptors;

/// <summary>
/// Verifies that the DispatchDomainEventsInterceptor dispatches and clears
/// domain events during SaveChanges.
/// </summary>
[Collection("Database")]
public class DispatchDomainEventsInterceptorTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SaveChanges_EntityWithNoDomainEvents_ShouldNotThrow()
    {
        await using var context = CreateDbContext<IdentityDbContext>();

        var permission = PermissionFactory.Create("dispatch", "read");
        context.Permissions.Add(permission);

        var act = () => context.SaveChangesAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SaveChanges_ShouldClearDomainEventsAfterDispatch()
    {
        await using var context = CreateDbContext<IdentityDbContext>();

        var role = RoleFactory.Create();
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        role.DomainEvents.Should().BeEmpty("the interceptor clears domain events once they are dispatched on save");

        await using var queryContext = CreateDbContext<IdentityDbContext>();
        RoleEntity? persisted = await queryContext.Roles.FindAsync(role.Id);
        persisted.Should().NotBeNull("the save should commit through the dispatch interceptor without faulting");
    }

    [Fact]
    public void SaveChanges_Synchronously_ShouldClearDomainEventsAfterDispatch()
    {
        // Arrange
        // Seeders and migrations save synchronously, so the sync path has to collect and
        // dispatch exactly like the async one.
        using var context = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        context.Roles.Add(role);

        // Act
        context.SaveChanges();

        // Assert
        role.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenTheSaveFails_ShouldLeaveTheAggregateClean()
    {
        // Arrange
        // Role names are unique, so a second role of the same name fails at the database. A
        // failed save must leave no reaction behind and no stale event for a later save.
        string name = $"dup-{Guid.NewGuid():N}"[..20];

        await using var context = CreateDbContext<IdentityDbContext>();
        context.Roles.Add(RoleFactory.Create(name));
        await context.SaveChangesAsync();

        await using var conflicting = CreateDbContext<IdentityDbContext>();
        RoleEntity duplicate = RoleFactory.Create(name);
        conflicting.Roles.Add(duplicate);

        // Act
        Func<Task> act = async () => await conflicting.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
        duplicate.DomainEvents.Should().BeEmpty();
    }
}
